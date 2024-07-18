using HtmlAgilityPack;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using WHBennett;

namespace COH_WARC_Processor
{
    public partial class MainForm : Form
    {

        string currentFileName = "";
        bool processing = false;
        bool done = false;
        readonly FTPclient f;
        FTPdirectory fInputFiles;
        FTPdirectory fProcessingFiles;
        FTPdirectory fProcessedFiles;

        readonly string hostName = "";
        readonly string userName = "";
        readonly string password = "";

        public MainForm()
        {
            InitializeComponent();
            f  = new FTPclient(hostName, userName, password);
        }

        private void BtnProcess_Click(object sender, EventArgs e)
        {
            try
            {
                if (processing)
                {
                    done = true;
                    lblMessage.Visible = true;
                    return;
                }
                Cursor = Cursors.WaitCursor;

                do
                {
                    btnProcess.Text = "Stop After This Iteration";
                    processing = true;
                    List<string> filesToDelete = new List<string>();
                    GetFileLists();

                    if (fInputFiles.Count == 0)
                    {
                        tsslMain.Text = "Input files 0 - Are we done yet???";
                        done = true;
                        processing = false;
                        btnProcess.Enabled = false;
                        btnProcess.Text = "Await further instructions...";
                        return;
                    }

                    int fileNum = new Random().Next(fInputFiles.Count);
                    string remoteFileName = "";
                    bool debug = false;
                    if (debug)
                    {
                        string fileNameToTest = "boards.cityofheroes.com-threads-range-28127-20120906-000725.warc";
                        remoteFileName = @"C:\Project Spelunker\input\" + fileNameToTest;
                        UpdateStatus("Starting to process debugging file");
                    }
                    else
                    {
                        remoteFileName = @"/input/" + fInputFiles[fileNum].Filename;
                        UpdateStatus("Starting to process randomized file # " + fileNum + " of " + fInputFiles.Count);
                    }
                    currentFileName = Path.GetTempPath() + Path.GetFileName(remoteFileName);
                    UpdateStatus("Downloading file named '" + remoteFileName + "'. To the local system for processing at the following location:" +
                        Environment.NewLine + currentFileName);

                    if (remoteFileName.StartsWith("C:"))
                    {
                        //If it's a local file, just copy it
                        if (!File.Exists(currentFileName)) File.Copy(remoteFileName, currentFileName);
                    }
                    else
                    {
                        if (!f.Download(remoteFileName, currentFileName))
                        {
                            UpdateStatus("Failed to download file '" + remoteFileName + "', restarting loop.");
                            continue;
                        }
                    }
                    
                    int instanceNumber = 1;
                    string outputPath = Path.GetTempPath() + @"CoH_Forums_Output\" + instanceNumber + @"\";
                    if (Directory.Exists(outputPath))
                    {
                        bool exists = true;
                        do
                        {
                            instanceNumber++;
                            outputPath = Path.GetTempPath() + @"CoH_Forums_Output\" + instanceNumber + @"\";
                            exists = Directory.Exists(outputPath);
                        } while (exists);
                        
                    }

                    if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
                    UpdateStatus("Downloaded file '" + currentFileName + "'.");
                    filesToDelete.Add(currentFileName);
                    //Move the file on the remote server to the "Processing" folder, so that it's not picked up by anyone else
                    string processingFileName = @"/processing/" + Path.GetFileName(currentFileName);

                    //Rename the local file for proper handling going forward.
                    if (remoteFileName.StartsWith("C:")) remoteFileName = @"/input/" + Path.GetFileName(remoteFileName);
                    if (!f.FtpRename(remoteFileName, processingFileName)) throw new Exception("Failed to move file to process from '" + remoteFileName + "' to '" +
                        processingFileName + "'.");
                    UpdateStatus("Now extracting WARC data from file to '" + outputPath + "'");
                    WarcExtractionRecord wer = new WarcExtractionRecord(currentFileName, outputPath, ref this.tsslMain);
                    string zipFileName = Path.GetTempPath() + Path.GetFileNameWithoutExtension(currentFileName) + ".zip";
                    UpdateStatus("Extracted WARC files. Now zipping files for upload to file '" + zipFileName + "'.");
                    using ZipFile zip = new ZipFile(zipFileName);
                    //Create a "credits" file
                    string creditFile = WarcExtractionRecord.GetSanitizedFileName("CreditGoesTo" + txtUserName.Text + ".crd", Path.GetTempPath());
                    File.WriteAllText(creditFile, "File created with the assistance of user '" + txtUserName.Text + "', who created this file at " +
                        DateTime.Now.ToString("HH:mm:ss") + " on " + DateTime.Now.ToString("MM/dd/yyyy"));
                    zip.UpdateFile(creditFile, @"\");
                    filesToDelete.Add(creditFile);
                    foreach (KeyValuePair<WarcRecord, string> kvp in wer.ExtractedFiles)
                    {
                        if (chkVerbose.Checked) UpdateStatus("Adding file '" + kvp.Value + "' to zip file.");
                        zip.UpdateFile(kvp.Value, @"\");
                        filesToDelete.Add(kvp.Value);
                    }
                    UpdateStatus("Saving zip file '" + zipFileName + ".");
                    zip.Save();
                    string finalLocation = "/output/" + Path.GetFileName(zipFileName);
                    UpdateStatus("Zip file created successfully. Now uploading zip file to final destination at '" + finalLocation + "'.");
                    if (!f.Upload(zipFileName, finalLocation)) throw new Exception("Failed to upload final file '" + zipFileName + "' to its final location." +
                        Environment.NewLine +"If possible, please report this to the program author and arrange for the file to be manually obtained.");
                    filesToDelete.Add(zipFileName);
                    //Delete all local files we've created, and then move the file to the processed folder so that it's not processed again.
                    UpdateStatus("Zip file uploaded successfully, now deleting local temporary files that this program created.");
                    foreach (string file in filesToDelete)
                    {
                        if (chkVerbose.Checked) UpdateStatus("Deleting file '" + file + "'.");
                        if (File.Exists(file)) File.Delete(file);
                    }
                    if (Directory.Exists(outputPath)) Directory.Delete(outputPath, true);
                    string processedFile = @"/processed/" + Path.GetFileName(processingFileName);
                    if (!f.FtpRename(processingFileName, processedFile)) throw new Exception("Failed to move file '" + processingFileName + "' to location '" +
                        processedFile + "'. This must be done manually by the program author. Please report this issue to them. All other processing completed!");
                    UpdateStatus("Processing of file '" + currentFileName + "' complete!");
                } while (!done);
                processing = false;
                lblMessage.Visible = false;
            }
            catch (Exception ex)
            {
                if (currentFileName.Trim() != "")
                {
                    Exception exWithFile = new Exception("Error while processing file '" + Path.GetFileName(currentFileName) + "'", ex);
                    if (File.Exists(currentFileName)) File.Delete(currentFileName);
                    LogError(exWithFile);
                }
                else
                {
                    LogError(ex);
                }
            }
            finally
            {
                btnProcess.Enabled = false;
                btnProcess.Text = "Process";
                txtUserName.Text = "";
                Cursor = Cursors.Default;
            }
        }

        private void GetFileLists()
        {
            try
            {
                UpdateStatus("Getting file list...");
                fInputFiles = f.ListDirectoryDetail(@"/input/");
                fProcessingFiles = f.ListDirectoryDetail(@"/processing/");
                fProcessedFiles = f.ListDirectoryDetail(@"/processed/");
                int filesToUpload = 12710 - (fInputFiles.Count + fProcessingFiles.Count + fProcessedFiles.Count);
                if (filesToUpload < 0) filesToUpload = 0;
                int totalFiles = fInputFiles.Count + fProcessedFiles.Count + fProcessingFiles.Count;
                decimal percentDone = Math.Round((decimal)fProcessedFiles.Count / totalFiles * 100, 2);

                int processingCount = fProcessingFiles.Count + (processing ? 1 : 0);

                tsslMain.Text = totalFiles + " total files, " + filesToUpload + " file(s) to be uploaded, " + 
                    fInputFiles.Count + " file(s) uploaded and waiting to be processed, " +
                     processingCount + " file(s) processing, " +
                    fProcessedFiles.Count + " file(s) processed, " +
                    "Phase 1 of project is " + percentDone + "% complete.";
                UpdateStatus("Ready...");
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private void LogError(Exception ex)
        {
            txtStatus.Text = "There was an error in the application. Please copy and paste the following text in a message to the program author:" +
                Environment.NewLine + ex.ToString();
        }

        //private string[] GetFilesBySize(string inputDir)
        //{
        //    try
        //    {
        //        string[] files = Directory.GetFiles(inputDir, "*.warc");
        //        DataTable dt = new DataTable();
        //        dt.Columns.AddRange(new DataColumn[] {
        //        new DataColumn("FileName", typeof(string)),
        //        new DataColumn("FileSize", typeof(long))
        //        });
        //        foreach (string file in files)
        //        {
        //            FileInfo fi = new FileInfo(file);
        //            dt.Rows.Add(file, long.Parse(fi.Length.ToString()));
        //        }

        //        dt.DefaultView.Sort = "FileSize";
        //        dt = dt.DefaultView.ToTable();
        //        List<string> filesInOrder = new List<string>();
        //        foreach (DataRow dr in dt.Rows)
        //        {
        //            filesInOrder.Add(dr["FileName"].ToString());
        //        }
        //        return filesInOrder.ToArray();
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.ToString());
        //        return null;
        //    }
        //}

        private void UpdateStatus(string msg)
        {
            txtStatus.Text = msg + Environment.NewLine + txtStatus.Text;
            Refresh();
            Application.DoEvents();
        }

        //private string RemoveHeaderInfo(string[] lines, string fileName)
        //{
        //    try
        //    {
        //        StringBuilder sb = new StringBuilder();
        //        bool InHeader = true;
        //        int i;
        //        for (i = 0; i < lines.Length; i++)
        //        {
        //            while (InHeader)
        //            {
        //                //If the header doesn't contain an image, and it's a PHP file
        //                if (!string.Join("", lines).ToUpper().Replace("-", " ").Contains("CONTENT TYPE: IMAGE") && fileName.Contains(".php"))
        //                {
        //                    //For php files, check for the checksum digit to escape the header
        //                    if (lines[i].Trim().Length == 2)
        //                    {
        //                        InHeader = false;
        //                        i++;
        //                        break;
        //                    }
        //                }
        //                //Otherwise, just look for the first blank line
        //                else if (lines[i] == "\r")
        //                {
        //                    InHeader = false;
        //                    i++;
        //                    break;
        //                }
        //                i++;
        //                continue;
        //            }
        //            try
        //            {
        //                sb.AppendLine(lines[i]);
        //            }
        //            catch
        //            {
        //                Debug.WriteLine("");
        //            }

        //        }

        //        return sb.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Failed to remove header info from content.", ex);
        //    }
        //}

        //public bool WriteToDirectory(WarcRecord wr, string StartingDirectory, bool AlsoCreateMetaData = false)
        //{
        //    try
        //    {
        //        string[] lines = wr.Content.Split('\n');
        //        //Remove the repsonse header if there is one
        //        string content = (lines[0].ToUpper().StartsWith("HTTP/") ? RemoveHeaderInfo(lines, wr.WARC_Target_URI.AbsolutePath) : wr.Content);

        //        //Standardize all filenames inside of the file, if they are locally related to CoH
        //        content = StandardizeReferences(StartingDirectory, content);

        //        string newPathAndFile = StandardizeFileName(StartingDirectory, wr.WARC_Target_URI);
        //        string newDirectory = Path.GetDirectoryName(newPathAndFile);
        //        if (!IsValidFileName(newPathAndFile)) return false;
        //        if (newDirectory != "" && !Directory.Exists(newDirectory)) Directory.CreateDirectory(newDirectory);
        //        using (StreamWriter sw = new StreamWriter(newPathAndFile))
        //        {
        //            sw.Write(content);
        //        }
        //        if (AlsoCreateMetaData)
        //        {
        //            using StreamWriter sw = new StreamWriter(newPathAndFile + ".warcmetadata");
        //            if (wr.WARC_Record_ID != null) sw.WriteLine("WARC-RECORD-ID:" + wr.WARC_Record_ID.OriginalString);
        //            if (wr.Content_Length != 0) sw.WriteLine("CONTENT-LENGTH:" + wr.Content_Length.ToString());
        //            if (wr.WARC_Date != null) sw.WriteLine("WARC-DATE:" + wr.WARC_Date.ToString());
        //            if (wr.WARC_Type != null) sw.WriteLine("WARC-TYPE:" + wr.WARC_Type.ToString());
        //            if (wr.Content_Type != null) sw.WriteLine("CONTENT-TYPE:" + wr.Content_Type.ToString());
        //            if (wr.WARC_Concurrent_To != null && wr.WARC_Concurrent_To.Count > 0)
        //            {
        //                foreach (Uri u in wr.WARC_Concurrent_To)
        //                {
        //                    sw.WriteLine("WARC-CONCURRENT-TO:" + u.OriginalString);
        //                }
        //            }
        //            if (wr.WARC_Block_Digest != null) sw.WriteLine("WARC-BLOCK-DIGEST:" + wr.WARC_Block_Digest.Algorithm + ":" + wr.WARC_Block_Digest.Value);
        //            if (wr.WARC_Payload_Digest != null) sw.WriteLine("WARC-PAYLOAD-DIGEST:" + wr.WARC_Payload_Digest.Algorithm + ":" + wr.WARC_Payload_Digest.Value);
        //            if (wr.WARC_IP_Address != null) sw.WriteLine("WARC-IP-ADDRESS:" + wr.WARC_IP_Address.ToString());
        //            if (wr.WARC_Refers_To != null) sw.WriteLine("WARC-REFERS-TO:" + wr.WARC_Refers_To.OriginalString);
        //            if (wr.WARC_Target_URI != null) sw.WriteLine("WARC-TARGET-URI:" + wr.WARC_Target_URI.OriginalString);
        //            if (wr.WARC_Truncated != null) sw.WriteLine("WARC-TRUNCATED:" + wr.WARC_Truncated.ToString());
        //            if (wr.WARC_Warcinfo_ID != null) sw.WriteLine("WARC-WARCINFO-ID:" + wr.WARC_Warcinfo_ID.OriginalString);
        //            if (wr.WARC_Filename != null) sw.WriteLine("WARC-FILENAME:" + wr.WARC_Filename);
        //            if (wr.WARC_Profile != null) sw.WriteLine("WARC-PROFILE:" + wr.WARC_Profile.OriginalString);
        //            if (wr.WARC_Identified_Payload_Type != null) sw.WriteLine("WARC-IDENTIFIED-PAYLOAD-TYPE:" + wr.WARC_Identified_Payload_Type.ToString());
        //            if (wr.WARC_Segment_Number != null) sw.WriteLine("WARC-SEGMENT-NUMBER:" + wr.WARC_Segment_Number.ToString());
        //            if (wr.WARC_Segment_Origin_ID != null) sw.WriteLine("WARC-SEGMENT-ORIGIN-ID:" + wr.WARC_Segment_Origin_ID.OriginalString);
        //            if (wr.WARC_Segment_Total_Length != null) sw.WriteLine("WARC-SEGMENT-TOTAL-LENGTH:" + wr.WARC_Segment_Total_Length.ToString());
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Failed to write data to directory '" + StartingDirectory + "'", ex);
        //    }
        //}

        //private bool IsValidFileName(string newPathAndFile)
        //{
        //    try
        //    {
        //        string fileName = Path.GetTempPath() + Path.GetFileName(newPathAndFile);
        //        using (StreamWriter sw = new StreamWriter(fileName))
        //        {
        //            sw.Write("");
        //        }
        //        if (File.Exists(fileName)) {
        //            File.Delete(fileName);
        //            return true;
        //        }
        //        return false;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        private string StandardizeReferences(string StartingDirectory, string inputContent)
        {
            try
            {
                string outputContent = inputContent;
                //Fix the content links, if applicable
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(inputContent);
                foreach (HtmlNode node in GetHTMLNodeByTagAttributeLikeValues(htmlDoc.DocumentNode, "a", "href", new List<string>() { "?", "&" }))
                {
                    //Try to fix links in the content
                    string[] linkElements = null;
                    try
                    {
                        linkElements = node.OuterHtml.Split(new string[] { @"href=""" }, StringSplitOptions.None);
                    }
                    catch
                    {
                        //Skip these ones, becasuse there's nothing that we can do to save this if we can't get the OuterHtml
                        continue;
                    }

                    if (linkElements.Length < 2)
                    {
                        Debug.WriteLine("");
                    }
                    string link = linkElements[1].Split('"')[0];
                    if (link.Contains(".php?"))
                    {
                        Uri uriToCheck;
                        try
                        {
                             uriToCheck = new Uri(link);
                        }
                        catch
                        {
                            //if we fail to build the URI, try adding a prefix to it, and try one more time
                            try
                            {
                                uriToCheck = new Uri("http://boards.cityofheroes.com/" + link);
                            }
                            catch
                            {
                                throw new Exception("Failed to parse link '" + link + "'");
                            }
                        }

                        string newlink = StandardizeFileName(StartingDirectory, uriToCheck);
                        HtmlNode newNode = null;
                        try
                        {
                            newNode = HtmlNode.CreateNode(node.OuterHtml.Replace(link, newlink));
                        }
                        catch
                        {
                            //Ignore this error, since this link would be unsalvageable
                        }

                        if (newNode != null)
                        {
                            try
                            {
                                node.ParentNode.ReplaceChild(newNode, node);
                            }
                            catch (Exception ex)
                            {
                                using StreamWriter sw = new StreamWriter(StartingDirectory + "Errors.log", true);
                                sw.Write(ex.ToString() + Environment.NewLine + Environment.NewLine + inputContent);
                            }
                            
                            //htmlDoc.LoadHtml(ReplaceNode(htmlDoc.DocumentNode, node, newNode).OuterHtml);
                        }
                    }
                }
                return htmlDoc.DocumentNode.OuterHtml;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse inputContent of '" + inputContent + "'", ex);
            }
        }

        private HtmlNode ReplaceNode(HtmlNode parentNode, HtmlNode oldNode, HtmlNode newNode)
        {
            try
            {
                HtmlNode returnValue = parentNode;
                foreach (HtmlNode node in parentNode.ChildNodes)
                {
                    if (node.OuterHtml == oldNode.OuterHtml)
                    {
                        //Since the Agility Pack's ReplaceChild method doesn't work, we'll have to do it manually ourselves
                        HtmlAgilityPack.HtmlDocument newHtml = new HtmlAgilityPack.HtmlDocument();
                        newHtml.LoadHtml(FindTopParent(parentNode).OuterHtml.Replace(oldNode.OuterHtml, newNode.OuterHtml));
                        return newHtml.DocumentNode;
                    }
                    if (node.HasChildNodes) returnValue = ReplaceNode(node, oldNode, newNode);
                    if (returnValue != null) return returnValue;
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to replace node '" + oldNode.OuterHtml + "' with new node '" + newNode.OuterHtml + "'", ex);
            }
        }

        private HtmlNode FindTopParent (HtmlNode childNode)
        {
            if (childNode.ParentNode == null) return childNode;
            return FindTopParent(childNode.ParentNode);
        }

        public static string ConvertToWindowsFileName(string urlText)
        {
            List<string> urlParts = new List<string>();
            string rt = "";
            Regex r = new Regex(@"[a-zA-Z0-9_]+", RegexOptions.IgnoreCase);
            foreach (Match m in r.Matches(urlText))
            {
                urlParts.Add(m.Value);
            }
            for (int i = 0; i < urlParts.Count; i++)
            {
                rt += urlParts[i];
                rt += "_";
            }
            return rt.Replace("_amp_", "");
        }

        static private HtmlNodeCollection GetHTMLNodeByTagAttributeLikeValues(HtmlNode ParentNode, string tag, string attribute, List<string> values)
        {
            try
            {
                HtmlNodeCollection returnValue = new HtmlNodeCollection(ParentNode);
                foreach (string value in values)
                {
                    foreach (HtmlNode n in GetHTMLNodeByTagAttributeLikeValue(ParentNode, tag, attribute, value))
                    {
                        returnValue.Add(n);
                    }
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get like values.", ex);
            }
        }

        static private HtmlNodeCollection GetHTMLNodeByTagAttributeLikeValue(HtmlNode ParentNode, string tag, string attribute, string value)
        {
            try
            {
                HtmlNodeCollection returnValue = new HtmlNodeCollection(ParentNode);
                HtmlNodeCollection nodes = GetAllNodes(ParentNode);

                foreach (HtmlNode node in nodes)
                {
                    if (node.Name != tag) continue;
                    if (!node.Attributes.Contains(attribute)) continue;
                    if (!node.Attributes[attribute].Value.ToString().ToUpper().Contains(value.ToUpper())) continue;
                    returnValue.Add(node);
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load HTML element due to malformed HTML: " +
                    Environment.NewLine + ParentNode.InnerHtml.ToString(), ex);
            }
        }

        static private HtmlNodeCollection GetAllNodes(HtmlNode ParentNode)
        {
            HtmlNodeCollection returnValue = new HtmlNodeCollection(ParentNode);
            foreach (HtmlNode node in ParentNode.ChildNodes)
            {
                returnValue.Add(node);
                GetChildNodes(node);
            }

            void GetChildNodes(HtmlNode node)
            {
                foreach (HtmlNode n in node.ChildNodes)
                {
                    returnValue.Add(n);
                    if (n.HasChildNodes)
                    {
                        GetChildNodes(n);
                    }
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Force all filenames and URI references to point to the same starting path and file naming scheme
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private string StandardizeFileName(string StartingDirectory, Uri PathAndFile)
        {
            try
            {
                string returnValue = SuperTrim(Uri.UnescapeDataString(PathAndFile.AbsolutePath));
                //change all forward slashes into backslashes
                if (returnValue.Contains("/")) returnValue = returnValue.Replace("/", @"\").Replace(@"\\", @"\");

                //stop filenames from ending with an underscore
                if (Path.GetFileNameWithoutExtension(returnValue).EndsWith("_")) returnValue = Path.GetDirectoryName(returnValue) + @"\"
                    + Path.GetFileNameWithoutExtension(returnValue).Substring(
                    0, Path.GetFileNameWithoutExtension(returnValue).Length - 1) +
                        Path.GetExtension(returnValue);

                if (returnValue.StartsWith(@"\")) returnValue = returnValue.Substring(1);

                return StartingDirectory + returnValue;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to standardize filename '" + PathAndFile.AbsolutePath + ", Query: " + PathAndFile.Query + "'", ex);
            }
        }

        private string SuperTrim(string v)
        {
            do
            {
                v = v.Trim('/');
                v = v.Trim('"');
                if (v.StartsWith(@"\")) v = v.Substring(1);
                if (v.EndsWith(@"\")) v = v.Substring(0, v.Length - 1);

            } while (v.StartsWith("/") || v.EndsWith("/") || v.StartsWith(@"\") || v.EndsWith(@"\") || v.StartsWith(@"""") || v.EndsWith(@""""));
            return v;
        }

        private void TxtUserName_TextChanged(object sender, EventArgs e)
        {
            btnProcess.Enabled = txtUserName.Text.Trim() != "";
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            GetFileLists();
        }

        private void tmrFroze_Tick(object sender, EventArgs e)
        {
            tsslMain.Text = "Program Not Frozen And Still Running As Of " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss");
            Application.DoEvents();
        }
    }
}
