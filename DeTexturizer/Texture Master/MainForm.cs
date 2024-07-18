using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Ionic.Utils;

namespace DeTexturizer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void BtnChooseFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Choose the Image or Texture File To Open",
                Filter = "Texture Files|*.texture|XML Files|*.xml|Image Files|*.jpg;*.jpeg;*.tga;*.dds|All Files|*.*"
            };
            if (ofd.ShowDialog() != DialogResult.OK || !File.Exists(ofd.FileName)) return;

            FolderBrowserDialogEx fbd = new FolderBrowserDialogEx()
            {
                Description = "Where would you like to save the output to?",
                SelectedPath = Path.GetDirectoryName(ofd.FileName),
                ShowFullPathInEditBox = true,
                ShowEditBox = true
            };
            if (fbd.ShowDialog() != DialogResult.OK) return;

            string returnValue;

            if (ofd.FileName.EndsWith(".texture"))
            {
                returnValue = ExtractTextureFile(ofd.FileName, fbd.SelectedPath + (fbd.SelectedPath.EndsWith(@"\") ? "" : @"\"));
            }
            else
            {
                returnValue = EncodeImageFile(ofd.FileName, fbd.SelectedPath + (fbd.SelectedPath.EndsWith(@"\") ? "" : @"\"));
            }

            MessageBox.Show(returnValue, "Results", MessageBoxButtons.OK,
                returnValue.ToUpper().Contains("FAIL") ? MessageBoxIcon.Error : MessageBoxIcon.Information);
        }

        private string EncodeImageFile(string inputFile, string outputFile = null)
        {
            string returnValue = "";
            try
            {
                DataSet ds = GetDataSet(inputFile);
                DataRow dr = ds.Tables[0].Rows[0];
                int headerLength = int.Parse(dr[0].ToString());
                int imageFileLength = int.Parse(dr[1].ToString()); //Length that the image file should be
                int width = int.Parse(dr[2].ToString()); //Width of the Image
                int height = int.Parse(dr[3].ToString()); //Height of the Image

                // No clue what these things are
                int unknownInt = int.Parse(dr[4].ToString());
                float unknownFloat1 = float.Parse(dr[5].ToString());
                float unknownFloat2 = float.Parse(dr[6].ToString());
                byte unknownByte = byte.Parse(dr[7].ToString()); //appears to be a boolean?
                string unknownString = dr[8].ToString(); //TX2???

                // The file name. This contains the relative path from the data directory
                // (not including that folder's name) and the file name
                string fileRelativePathAndName = dr[9].ToString();

                //The actual file itself
                byte[] fileBytes = Convert.FromBase64String(dr["FileBytes"].ToString());

                if (outputFile == null) outputFile = Path.GetDirectoryName(inputFile) + Path.GetFileName(fileRelativePathAndName);
                if (Path.GetDirectoryName(outputFile) + @"\" == outputFile) outputFile +=
                        Path.GetFileNameWithoutExtension(fileRelativePathAndName.Replace("/", @"\")) + ".texture";

                if (File.Exists(outputFile))
                {
                    if (File.Exists(outputFile + ".bak")) File.Delete(outputFile + ".bak");
                    File.Move(outputFile, outputFile + ".bak");
                    returnValue += "Original existing file found and backed up at '" + outputFile + ".bak'" +
                        Environment.NewLine;
                }

                //Verify that the Header is the right length, and the file is the right length, otherwise error out
                using (BinaryWriter bw = new BinaryWriter(new FileStream(outputFile, FileMode.Create)))
                {
                    bw.Write(headerLength);
                    bw.Write(fileBytes.Length);
                    bw.Write(width);
                    bw.Write(height);
                    bw.Write(unknownInt);
                    bw.Write(unknownFloat1);
                    bw.Write(unknownFloat2);
                    bw.Write(unknownByte);
                    //Note that we have to remember to encode these strings in ASCII to prevent .NET from adding extra
                    //junk to it. Reference - https://stackoverflow.com/questions/12889850/binary-writer-inserts-extra-character-in-writing
                    bw.Write(Encoding.ASCII.GetBytes(unknownString));
                    bw.Write(Encoding.ASCII.GetBytes(fileRelativePathAndName));
                    bw.Write(Encoding.ASCII.GetBytes("\0")); //Null character to terminate the file name
                    //If we have the extra "unknown junk" that was found earlier, then insert it back in to maintain file integrity
                    if (dr.Table.Columns.Contains("UnknownBytes") && dr["UnknownBytes"] != null)
                    {
                        bw.Write(Convert.FromBase64String(dr["UnknownBytes"].ToString()));
                    }

                    bw.Write(fileBytes);
                }

                returnValue += "Re-encoded file '" + inputFile + "' into texture file '" + outputFile + "'";

                return returnValue;

            }
            catch (Exception ex)
            {
                return "Failed to encode image file back to texture file due to the following error: " + ex.ToString();
            }
        }

        private string ExtractTextureFile(string inputFile, string outputFile = null)
        {
            try
            {
                DataSet ds = GetDataSet(inputFile);
                //Delete the XML file if it exists, as it's no longer needed since we either already have the data, or are getting it from a texture
                if (Path.GetFileNameWithoutExtension(outputFile) == null || Path.GetFileNameWithoutExtension(outputFile).Trim() == "") outputFile =
                        Path.GetDirectoryName(inputFile) + @"\" + Path.GetFileNameWithoutExtension(inputFile);
                if (File.Exists(outputFile + ".xml")) File.Delete(outputFile + ".xml");
                
                if (chkExportXML.Checked)
                {
                    //Remove the FileBytes, since we're writing the actual XML file here
                    DataSet dsCopy = ds.Copy();
                    if (dsCopy.Tables[0].Columns.Contains("FileBytes")) dsCopy.Tables[0].Columns.Remove("FileBytes");
                    //Add back in the filename if it's missing for some reason

                    dsCopy.WriteXml(outputFile + ".xml");
                }
                
                //Set the default output file to the same directory as the original file
                string fileRelativePathAndName = ds.Tables[0].Rows[0]["FileRelativePathAndName"].ToString();
                byte[] fileBytes = Convert.FromBase64String(ds.Tables[0].Rows[0]["FileBytes"].ToString());
                outputFile = Path.GetDirectoryName(inputFile) + @"\" + Path.GetFileName(fileRelativePathAndName);
                if (outputFile.Contains("/")) outputFile.Replace("/", @"\");

                //Delete file if it already exists
                if (File.Exists(outputFile)) File.Delete(outputFile);

                File.WriteAllBytes(outputFile, fileBytes);

                return "Files Created:" + Environment.NewLine +
                    "Image file - " + outputFile + (chkExportXML.Checked ? Environment.NewLine +
                    "Texture Header XML File - " + outputFile + ".xml" : "");
            }
            catch (Exception ex)
            {
                return "Failed due to the following error - " + ex.ToString();
            }

        }

        private DataSet GetDataSet(string inputFile)
        {
            try
            {
                //Input file could be a texture, an XML file, or an image file
                string XMLFile = null;
                string imageFile = null;
                string textureFile = null;
                DataSet ds = null;

                switch (Path.GetExtension(inputFile).ToUpper())
                {
                    case ".XML":
                        //Read data from the XML file, except the actual file bytes, which we'll read from the image file, so make sure that it's there
                        XMLFile = inputFile;
                        ds = new DataSet();
                        ds.ReadXml(XMLFile);
                        //Pull the image data from the image file in the same directory
                        imageFile = Path.GetDirectoryName(XMLFile) + @"\" + Path.GetFileName(ds.Tables[0].Rows[0]["FileRelativePathAndName"].ToString());
                        if (!File.Exists(imageFile)) throw new Exception("Failed to find image file at location '" + imageFile + "'." +
                            "If an XML file is chosen, then an image file with the same filename must exist in the same directory.");
                        //Insert the new image file
                        if (!ds.Tables[0].Columns.Contains("FileBytes")) ds.Tables[0].Columns.Add("FileBytes");
                        ds.Tables[0].Rows[0]["FileBytes"] = Convert.ToBase64String(File.ReadAllBytes(imageFile));
                        return ds;
                    case ".TEXTURE":
                        return GetDataSetFromTextureFile(inputFile);
                    default:
                        //Assume it's the image file, and look for either the texture file in the same location or the XML file
                        imageFile = inputFile;
                        XMLFile = Path.GetDirectoryName(imageFile) + @"\" + Path.GetFileNameWithoutExtension(imageFile) + ".xml";
                        textureFile = Path.GetDirectoryName(imageFile) + @"\" + Path.GetFileNameWithoutExtension(imageFile) + ".texture";
                        if (!File.Exists(XMLFile) && !File.Exists(textureFile)) throw new Exception("Cannot encode an image file without either an XML file " +
                            "or a .texture file of the same name in the same directory.");
                        //If the XML file exists, use that to override whatever there might be in the texture file
                        if (File.Exists(XMLFile))
                        {
                            //Pull all data from the XML file, except insert the image data into it
                            ds = new DataSet();
                            ds.ReadXml(XMLFile);
                            //Insert the new image file
                            if (!ds.Tables[0].Columns.Contains("FileBytes")) ds.Tables[0].Columns.Add("FileBytes");
                            ds.Tables[0].Rows[0]["FileBytes"] = Convert.ToBase64String(File.ReadAllBytes(imageFile));
                            return ds;
                        }
                        else
                        {
                            //Pull the data from the Texture file, and fill in the file data with the data from the image file
                            ds = GetDataSetFromTextureFile(textureFile);
                            //Insert the new image file
                            ds.Tables[0].Rows[0]["FileBytes"] = Convert.ToBase64String(File.ReadAllBytes(imageFile));
                            return ds;
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }

        private DataSet GetDataSetFromTextureFile(string inputFile)
        {
            try
            {
                DataSet ds;
                //Read all data directly from the texture
                byte[] unknownBytes = null;
                using (BinaryReader b = new BinaryReader(File.Open(inputFile, FileMode.Open)))
                {
                    //Read the Texture Header information
                    int textureFileLength = (int)b.BaseStream.Length; //Length of the entire file
                    int headerLength = b.ReadInt32();//Length of the header itself, this should equal the total byte value of all of the variables below,
                    int imageFileLength = b.ReadInt32(); //Length that the image file should be
                    int width = b.ReadInt32(); //Width of the Image
                    int height = b.ReadInt32(); //Height of the Image

                    //No clue what these things are
                    int unknownInt = b.ReadInt32();
                    float unknownFloat1 = b.ReadSingle();
                    float unknownFloat2 = b.ReadSingle();
                    byte unknownByte = b.ReadByte(); //appears to be a boolean?
                    string unknownString = ReadSpecifiedLengthString(b, 3); //TX2???

                    //Read the file name. This contains the relative path from the data directory
                    //(not including that folder's name) and the file name
                    string fileRelativePathAndName = ReadNullTerminatedString(b);

                    //Write the actual file itself - read the data until you find the "DDS" string, this is the beginning of the actual DDS file
                    byte[] fileBytes = b.ReadBytes(textureFileLength - int.Parse(b.BaseStream.Position.ToString()));

                    //Convert a copy of the bytes to a string, and see if it contains a "DDS" reference
                    string testString = new string(Encoding.Default.GetString(fileBytes).ToCharArray());
                    if (testString.Contains("DDS"))
                    {
                        //If it does, then split off all data up until the DDS reference as an extra unknown file for re-import later
                        int ddsStart = 0;
                        for (int i = 0; i < fileBytes.Length - 1; i++)
                        {
                            if (Encoding.ASCII.GetString(new byte[] { fileBytes[i], fileBytes[i + 1], fileBytes[i + 2] }) == "DDS")
                            {
                                ddsStart = i;
                                break;
                            }
                        }

                        //Store these bits for later to be re-insterted into the texture file from the XML file
                        List<byte> temp = new List<byte>();
                        for (int i = 0; i < ddsStart; i++)
                        {
                            temp.Add(fileBytes[i]);
                        }
                        unknownBytes = temp.ToArray();

                        //Then the remaining can be written as the DDS file
                        List<byte> validFileTemp = new List<byte>();
                        for (int i = ddsStart; i < fileBytes.Length; i++)
                        {
                            validFileTemp.Add(fileBytes[i]);
                        }
                        fileBytes = validFileTemp.ToArray();
                    }
                    ds = new DataSet();
                    //Also write the header information for later use in recompiling
                    DataTable dt = new DataTable();
                    dt.Columns.AddRange(new DataColumn[] {
                                new DataColumn("HeaderLength"),
                                new DataColumn("ImageFileLength"),
                                new DataColumn("Width"),
                                new DataColumn("Height"),
                                new DataColumn("UnknownInt"),
                                new DataColumn("UnknownFloat1"),
                                new DataColumn("UnknownFloat2"),
                                new DataColumn("UnknownByte"),
                                new DataColumn("UnknownString"),
                                new DataColumn("FileRelativePathAndName"),
                                new DataColumn("UnknownBytes"),
                                new DataColumn("FileBytes")
                            });

                    //Note that we'll have to call Convert.FromBase64String later to get it back!
                    dt.Rows.Add(headerLength, imageFileLength, width, height, unknownInt, unknownFloat1, unknownFloat2,
                        unknownByte, unknownString, fileRelativePathAndName, unknownBytes == null ? null : Convert.ToBase64String(unknownBytes),
                        fileBytes == null ? null : Convert.ToBase64String(fileBytes));

                    ds.Tables.Add(dt);
                }
                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get DataSet from image file", ex);
            }
        }

        static string ReadNullTerminatedString(BinaryReader stream)
        {
            string str = "";
            char ch;
            while ((ch = stream.ReadChar()) != 0)
                str += ch;
            return str;
        }

        static string ReadSpecifiedLengthString(BinaryReader stream, int length)
        {
            string str = "";
            char ch;
            for (int i = 0; i < length; i++)
            {
                ch = stream.ReadChar();
                str += ch;
            }
            return str;
        }

        private void BtnConvertDirectory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialogEx inputDirectory = new FolderBrowserDialogEx
            {
                Description = "Choose the Directory With Files You Wish To Convert",
                ShowFullPathInEditBox = true,
                ShowEditBox = true
            };
            if (inputDirectory.ShowDialog() != DialogResult.OK || !Directory.Exists(inputDirectory.SelectedPath)) return;

            FolderBrowserDialogEx outputDirectory = new FolderBrowserDialogEx()
            {
                Description = "Where would you like to save the output to?",
                SelectedPath = inputDirectory.SelectedPath,
                ShowFullPathInEditBox = true,
                ShowEditBox = true
            };
            if (outputDirectory.ShowDialog() != DialogResult.OK) return;

            string returnValue = "";

            DialogResult dr = MessageBox.Show("Please click 'Yes' to Texturize images in this directory, or 'No' to Detexturize " +
                ".texture files in this directory into image files.", "Texturize/Detexturize",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (dr == DialogResult.Cancel) return;

            if (dr == DialogResult.No)
            {
                foreach (string fileName in Directory.GetFiles(inputDirectory.SelectedPath, "*.texture"))
                {
                    returnValue += ExtractTextureFile(fileName,
                    outputDirectory.SelectedPath + (outputDirectory.SelectedPath.EndsWith(@"\") ? "" : @"\")) +
                    Environment.NewLine;
                }
            }
            else
            {
                foreach (string fileName in Directory.GetFiles(inputDirectory.SelectedPath, "*.*").
                    Where(fileName => !fileName.EndsWith(".texture") && !fileName.EndsWith(".xml")))
                {
                    returnValue += EncodeImageFile(fileName,
                        outputDirectory.SelectedPath + (outputDirectory.SelectedPath.EndsWith(@"\") ? "" : @"\")) +
                    Environment.NewLine;
                }
            }

            string tempFile = Path.GetTempFileName();
            using (StreamWriter sw = new StreamWriter(tempFile))
            {
                sw.WriteLine(returnValue);
            }

            if (File.Exists(tempFile)) Process.Start(tempFile);
        }
    }
}
