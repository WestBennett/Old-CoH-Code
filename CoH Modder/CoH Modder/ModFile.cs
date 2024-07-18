using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoH_Modder
{
    class ModFile
    {

        public static bool ValidModFile(string fileName)
        {
            try
            {
                ModFile m = new ModFile(fileName);
                return true;
            }
            catch 
            {
                return false;
            }
        }

        /// <summary>
        /// Just sets the cursor to the right one, depending on if we're waiting or not.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="Waiting"></param>
        public static void WaitCursor(System.Windows.Forms.Form form, bool Waiting)
        {
            form.Cursor = Waiting ? System.Windows.Forms.Cursors.WaitCursor : System.Windows.Forms.Cursors.Default;
        }

        public string ModName { get; set; }
        public string ModCategory { get; set; }
        public string ModAuthor { get; set; }
        public string ModDescription { get; set; }
        public int ModVersion { get; set; }
        public string ModFileName { get; set; }

        private string ModInfoFileName { get; set; } = "Mod.Info";

        public Dictionary<string, string> InstallFiles { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Import a file as a ModFile
        /// </summary>
        /// <param name="FileName"></param>
        public ModFile(string FileName)
        {
            try
            {
                ModFileName = FileName;
                using (ZipArchive archive = ZipFile.OpenRead(FileName))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (Path.GetFileName(entry.FullName.ToUpper()) == ModInfoFileName.ToUpper())
                        {
                            string tempFile = Path.GetTempFileName();
                            if (File.Exists(tempFile)) File.Delete(tempFile);
                            entry.ExtractToFile(tempFile);

                            using (Stream stream = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
                            {
                                DataTable input = new DataTable("ModInfo");
                                input.Columns.AddRange(new DataColumn[] {
                                    new DataColumn(nameof(ModName)),
                                    new DataColumn(nameof(ModCategory)),
                                    new DataColumn(nameof(ModAuthor)),
                                    new DataColumn(nameof(ModDescription)),
                                    new DataColumn(nameof(ModVersion)),
                                    new DataColumn("SourceFile"),
                                    new DataColumn("Destination")
                                    });
                                //use ReadXml to read the XML stream
                                input.ReadXml(stream);

                                foreach (DataRow dr in input.Rows)
                                {
                                    ModName = input.Rows[0][nameof(ModName)].ToString();
                                    if (ModName.Trim() == "") continue; //Trap to prevent bad mod info from getting into the sytem
                                    if (input.Columns.Contains(nameof(ModCategory))) ModCategory = input.Rows[0][nameof(ModCategory)].ToString();
                                    ModAuthor = input.Rows[0][nameof(ModAuthor)].ToString();
                                    ModDescription = input.Rows[0][nameof(ModDescription)].ToString();
                                    ModVersion = int.Parse(input.Rows[0][nameof(ModVersion)].ToString());
                                    break;
                                }

                                foreach (DataRow dr in input.Rows)
                                {
                                    InstallFiles.Add(dr["SourceFile"].ToString(), dr["Destination"].ToString());
                                }
                            }
                            File.Delete(tempFile);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid ModFile '" + FileName + "'", ex);
            }
        }

        /// <summary>
        /// Blank Constructor, for ModFile creation
        /// </summary>
        public ModFile() { }

        public bool Save()
        {
            try
            {
                //First, create the ModInfo file
                string ModInfoFile = Path.GetTempPath() + ModInfoFileName;
                ModVersion += 1;

                DataTable output = new DataTable("ModInfo");
                output.Columns.AddRange(new DataColumn[] {
                new DataColumn(nameof(ModName)),
                new DataColumn(nameof(ModCategory)),
                new DataColumn(nameof(ModAuthor)),
                new DataColumn(nameof(ModDescription)),
                new DataColumn(nameof(ModVersion)),
                new DataColumn("SourceFile"),
                new DataColumn("Destination")
                });

                foreach (KeyValuePair<string, string> kvp in InstallFiles)
                {
                    output.Rows.Add(ModName, ModCategory, ModAuthor, ModDescription, ModVersion, kvp.Key, kvp.Value);
                }

                output.WriteXml(ModInfoFile);

                //Next start creating the zip archive
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        //Add the ModInfo file to the archive
                        archive.CreateEntryFromFile(ModInfoFile, ModInfoFileName);
                        //Add all of the files to the zip archive
                        foreach (KeyValuePair<string, string> kvp in InstallFiles)
                        {
                            archive.CreateEntryFromFile(kvp.Key, Path.GetFileName(kvp.Key));
                        }
                    }

                    //Save the zip archive at its final location
                    if (File.Exists(ModFileName)) File.Delete(ModFileName);
                    using (FileStream fileStream = new FileStream(ModFileName, FileMode.Create))
                    {
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        memoryStream.CopyTo(fileStream);
                    }
                }
                if (File.Exists(ModInfoFile)) File.Delete(ModInfoFile);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to save file '" + ModFileName + "'", ex);
            }
        }
    }
}
