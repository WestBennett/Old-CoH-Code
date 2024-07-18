using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using COH_WARC_Processor;

namespace ScanSpelunker
{
    class Program
    {
        public readonly static string urlBase = @"http://cohforums.cityofplayers.com";
        public readonly static string fileDir = urlBase + "/files/";
        public readonly static string MainSourceDir = @"C:\inetpub\wwwroot";

        public static readonly string hostName = "";
        public static readonly string userName = "";
        public static readonly string password = "";

        public static FTPclient f;
        public static FTPdirectory fInputFiles;
        public static FTPdirectory fProcessingFiles;
        public static FTPdirectory fProcessedFiles;

        static void Main()
        {
            try
            {

                bool success = FixAllHyperlinks();

                return;

                string[] dirs = Directory.GetDirectories(MainSourceDir);
                // UserName, Tuple: FileName-and-Relative-Path, File-Size
                List<Tuple<string, string, long>> credits = new List<Tuple<string, string, long>>();
                for (int j = 0; j < dirs.Length - 1; j++)
                {
                    string dir = dirs[j];
                    string[] dirParts = dir.Split(new string[] { @"\" }, StringSplitOptions.None);
                    string directoryName = dirParts[dirParts.Length - 1];
                    string userName = "";

                    string[] files = Directory.GetFiles(dir);

                    //Find the username first
                    string[] creditFiles = Directory.GetFiles(dir, "CreditGoesTo*");
                    if (creditFiles.Length != 1)
                    {
                        Debug.WriteLine("Didn't find a single file");
                    }

                    userName = Path.GetFileNameWithoutExtension(creditFiles[0]).Replace("CreditGoesTo", "");
                    if (userName.Contains("_renamed_")) userName = userName.Split(new string[] { "_renamed_" }, StringSplitOptions.None)[0];

                    for (int i = 0; i < files.Length - 1; i++)
                    {
                        string file = files[i];

                        string newFileName = "";
                        if (Path.GetFileNameWithoutExtension(file).Length > 100) newFileName =
                                Path.GetDirectoryName(file) + @"\" + Path.GetFileNameWithoutExtension(file).Substring(0, 100) + Path.GetExtension(file);

                        if (newFileName != "")
                        {
                            //Try to rename the file, to prevent errors in processing
                            try
                            {
                                if (File.Exists(newFileName)) File.Delete(newFileName);
                                LessIO.FileSystem.Copy(new LessIO.Path(file), new LessIO.Path(newFileName));
                                LessIO.FileSystem.RemoveFile(new LessIO.Path(file));
                            }
                            catch (Exception ex)
                            {
                                string foundFileName = Path.GetFileName(file);
                                string replaceFileName = Path.GetFileNameWithoutExtension(file).Substring(0, 100) + Path.GetExtension(file);
                                Debug.WriteLine(ex.ToString());
                            }
                            file = newFileName;
                        }

                        //Write to the console so that we know it's working
                        string message = "Directory " + (j + 1) + " of " + dirs.Length + ", file " + (i + 1) + " of " + files.Length + ". " +
                             "Directory percent: " + Math.Round(decimal.Parse((j + 1).ToString()) / dirs.Length * 100, 4) + "%, files percent " +
                             Math.Round((decimal.Parse((i + 1).ToString())) / files.Length * 100, 4) + "%";
                        Console.WriteLine(message);

                        if (!Path.GetFileName(file).StartsWith("CreditGoesTo"))
                        {
                            //Add it to the dictionary of found files
                            string relativePath = directoryName + @"\" + Path.GetFileName(file);

                            FileInfo fi = new FileInfo(file);
                            try
                            {
                                credits.Add(new Tuple<string, string, long>(userName, relativePath, fi.Length));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
                            }
                        }
                    }
                }

                //Create the Dictionary file of all files
                SortedDictionary<string, int> numFiles = new SortedDictionary<string, int>();
                SortedDictionary<string, double> fileSizes = new SortedDictionary<string, double>();
                DataTable dtFileNames = new DataTable();
                dtFileNames.Columns.AddRange(new DataColumn[] { new DataColumn("Name"), new DataColumn("FileName") });

                foreach (Tuple<string, string, long> credit in credits)
                {
                    //Collect the total number of files
                    if (!numFiles.ContainsKey(credit.Item1)) numFiles.Add(credit.Item1, 1);
                    else numFiles[credit.Item1] = numFiles[credit.Item1] + 1;

                    //Collect the file list
                    dtFileNames.Rows.Add(credit.Item1, credit.Item2);

                    //Collect the fle sizes
                    if (!fileSizes.ContainsKey(credit.Item1)) fileSizes.Add(credit.Item1, credit.Item3);
                    else fileSizes[credit.Item1] = fileSizes[credit.Item1] + credit.Item3;

                }

                var fileNumberSort = from kvp in numFiles
                                     orderby kvp.Value, kvp.Key
                                     select kvp;

                var fileSizeSort = from kvp in fileSizes
                                   orderby kvp.Value, kvp.Key
                                   select kvp;

                DataTable allFileNames = dtFileNames.AsEnumerable()
                   .OrderBy(r => r.Field<string>("Name"))
                   .ThenBy(r => r.Field<string>("FileName"))
                   .CopyToDataTable();

                Debug.WriteLine("");

                //Create the Credits HTML file
                using (StreamWriter sw = new StreamWriter(@"C:\Project Spelunker\Credits_Phase_1.html"))
                {
                    sw.WriteLine("<html><head><title>Credits For Phase 1</title></head><body>");

                    //Show all names by total file sizes
                    sw.WriteLine("<H1>Credits For Phase 1 - By Total Number of Files</H1><br/><br/>");

                    sw.WriteLine("<table><th><td><b>Name</b></td><td><b>Total Number of Files Processed</b></td></th>");
                    foreach (KeyValuePair<string, int> kvp in fileNumberSort)
                    {
                        sw.WriteLine("<tr><td>" + kvp.Key + "</td><td>" + kvp.Value + "</td></tr>");
                    }
                    sw.WriteLine("</table><br/><br/>");

                    //Show all names by total number of files
                    sw.WriteLine("<H1>Credits For Phase 1 - By Total Bytes</H1><br/><br/>");

                    sw.WriteLine("<table><th><td><b>Name</b></td><td><b>Total Number of Bytes Processed</b></td></th>");
                    foreach (KeyValuePair<string, double> kvp in fileSizeSort)
                    {
                        sw.WriteLine("<tr><td>" + kvp.Key + "</td><td>" + kvp.Value + "</td></tr>");
                    }
                    sw.WriteLine("</table><br/><br/>");

                    //Show the full list of files processed
                    sw.WriteLine("<H1>Credits For Phase 1 - All Files By User</H1><br/><br/>");

                    sw.WriteLine("<table><th><td><b>Name</b></td><td><b>FileName</b></td></th>");
                    foreach (DataRow dr in allFileNames.Rows)
                    {
                        sw.WriteLine("<tr><td>" + dr["Name"] + "</td><td>" + dr["FileName"] + "</td></tr>");
                    }
                    sw.WriteLine("</table><br/><br/>");

                    sw.WriteLine("</body></html>");
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private static bool RemoveDuplicates()
        {
            try
            {
                string phase2Dir = @"C:\Project Spelunker\phase 2\";
                if (File.Exists(@"c:\temp\phase2.log")) File.Delete(@"c:\temp\phase2.log");
                Dictionary<string, string> allFiles = new Dictionary<string, string>();

                string[] dirs = Directory.GetDirectories(phase2Dir);

                for (int dir = 0; dir < dirs.Length - 1; dir++)
                {
                    WriteLog("Processing Directory " + (dir + 1) + " of " + dirs.Length);
                    string directory = dirs[dir];

                    string[] files = Directory.GetFiles(directory);

                    for (int fileNum = 0; fileNum < files.Length - 1; fileNum++)
                    {
                        string file = files[fileNum];
                        string newFileNameAndPath = GetSanitizedFileName(Path.GetFileName(file), directory);
                        string newFileName = Path.GetFileName(newFileNameAndPath);

                        //Rename the file if it doesn't have this name
                        if (newFileNameAndPath != file)
                        {
                            WriteLog("Renaming file '" + file + "' to '" + newFileName + "'.");
                            if (LessIO.FileSystem.Exists(new LessIO.Path(newFileNameAndPath)))
                            {
                                LessIO.FileSystem.RemoveFile(new LessIO.Path(newFileNameAndPath));
                            }
                            if (LessIO.FileSystem.Exists(new LessIO.Path(file)))
                            {
                                LessIO.FileSystem.Copy(new LessIO.Path(file), new LessIO.Path(newFileNameAndPath));
                                LessIO.FileSystem.RemoveFile(new LessIO.Path(file));
                            };
                        }

                        if (allFiles.ContainsKey(newFileName))
                        {
                            WriteLog("Deleting file '" + file + "' as duplicate of file '" + allFiles[newFileName]);
                            //Delete the extra file
                            if (LessIO.FileSystem.Exists(new LessIO.Path(file))) LessIO.FileSystem.RemoveFile(new LessIO.Path(file));
                        }
                        else
                        {
                            //Add it to the dictionary
                            allFiles.Add(newFileName, newFileNameAndPath);
                        }
                    }
                }

                //Write the final dictionary to the Dictionary file
                using (StreamWriter sw = new StreamWriter(@"c:\temp\Coh_HTMLs.txt"))
                {
                    foreach (KeyValuePair<string, string> kvp in allFiles)
                    {
                        if (!kvp.Value.EndsWith("html")) continue;
                        string relativePath = kvp.Value.Replace(@"C:\Project Spelunker\phase 2\", "").Replace(@"\", "/");
                        sw.WriteLine(kvp.Key + "," + relativePath);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        private static void WriteLog(string message)
        {
            try
            {
                Console.WriteLine(message);
                using (StreamWriter sw = new StreamWriter(@"c:\temp\phase2.log", true))
                {
                    sw.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Removes unwanted characters from a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string RemoveUnwantedChars(string input)
        {
            try
            {
                string newOutput = null;
                StringBuilder sb = new StringBuilder();
                List<string> acceptableWeirdos = new List<string>() { "_", ".", "/", @"\" };
                foreach (char c in input.ToCharArray())
                {
                    string s = c.ToString();
                    if (!acceptableWeirdos.Contains(s) && !char.IsLetter(c) && !char.IsNumber(c)) continue;
                    sb.Append(s);
                }
                newOutput = sb.ToString();
                return newOutput;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to remove unwanted chars", ex);
            }
        }

        /// <summary>
        /// Sanitize the file name to be useful as an actual file name
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetSanitizedFileName(string input, string outputDir)
        {
            try
            {
                string inputToUse = input;

                //If there's a querystring, rearrange it to put the query inside of the name of the file
                if (inputToUse.Contains("?") && inputToUse.Contains(".") && inputToUse.IndexOf("?") > inputToUse.IndexOf("."))
                {
                    if (inputToUse.EndsWith(".")) inputToUse = inputToUse.Trim('.');
                    if (inputToUse.Contains("...")) inputToUse = inputToUse.Replace("...", ".");

                    string prefix = inputToUse.Split('?')[0]; //Before the QueryString
                    string suffix = inputToUse.Replace(prefix, ""); //The QueryString

                    //If the suffix includes a hash, remove it and everything after it. We just can't be that specific, it's too complicated
                    if (suffix.Contains("#"))
                    {
                        suffix = suffix.Split('#')[0];
                    }

                    //Get the last index of the period to find the extension
                    int indexOfLastPeriod = inputToUse.LastIndexOf('.');
                    string extension = inputToUse.Substring(indexOfLastPeriod + 1);
                    if (extension.Length > 3) extension = extension.Substring(0, 3);

                    try
                    {
                        if (prefix.Contains(extension)) prefix = prefix.Replace(extension, ""); //Remove the extension from the prefix
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                    if (suffix.Contains(extension)) suffix = suffix.Replace(extension, ""); //Remove the extension from the suffix
                    if (extension == "php") extension = "html"; //Replace PHP with HTML
                    if (prefix.EndsWith(".")) prefix = prefix.Substring(0, prefix.Length - 1); //Drop the last period if it ends with one
                    if (suffix.EndsWith(".")) suffix = suffix.Substring(0, suffix.Length - 1); //Drop the last period if it ends with one
                    inputToUse = prefix + "_" + suffix + "." + extension;
                }

                string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
                string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

                string output = System.Text.RegularExpressions.Regex.Replace(inputToUse, invalidRegStr, "_");

                output = RemoveUnwantedChars(output);

                output = outputDir + Path.GetFileName(output);

                //Fix ampersand marks
                if (output.Contains("amppost")) output = output.Replace("amppost", "post");

                //Reduce the filename itself to 100 characters, so that we don't conflict with the path
                if (Path.GetFileNameWithoutExtension(output).Length > 100) output =
                        Path.GetDirectoryName(output) + @"\" + Path.GetFileNameWithoutExtension(output).Substring(0, 100) + Path.GetExtension(output);

                if (output.Length > 250)
                {
                    if (Path.GetExtension(output) == null || Path.GetExtension(output).Trim() == "") output = output.Substring(0, 250);
                    else
                    {
                        string extension = Path.GetExtension(output);
                        if (extension.Length > 4) extension = extension.Substring(0, 4); //Reduce extensions that are too long first, to prevent other issues
                        //Take out any extra dots that might be causing issues in the filename
                        if (Path.GetFileNameWithoutExtension(output).Contains(".")) output = Path.GetFileNameWithoutExtension(output).Replace(".", "") +
                                extension;
                        output = outputDir + (outputDir.EndsWith(@"\") ? "" : @"\") + Path.GetFileName(output);
                        //Escape this sequence if the file is already short enough after fixing the extension
                        if (output.Length > 250)
                        {
                            //If the file name is too long then trim everything up to the extension to be shorter than 250

                            //First, take off the extension by removing anything after the last period, then trim that extension to three characters
                            string[] elements = output.Split('.');
                            string ext = elements[elements.Length - 1];
                            if (ext.Length > 4) ext = ext.Substring(0, 4); //lob off the rest
                            string theRest = output.Substring(0, output.LastIndexOf(ext)).Trim('.');
                            if (theRest.Length > 250) theRest = theRest.Substring(0, 250); //Shorten theRest
                            output = theRest + "." + ext;
                            if (output.Length > 250)
                            {
                                string toReduce = Path.GetDirectoryName(output) + Path.GetFileNameWithoutExtension(output);
                                int lengthWithoutExtension = toReduce.Length - 1;
                                do
                                {
                                    toReduce = Path.GetDirectoryName(output) + @"\" + Path.GetFileNameWithoutExtension(output);
                                    output = toReduce.Substring(0, lengthWithoutExtension) +
                                        Path.GetExtension(output);
                                    lengthWithoutExtension = (Path.GetDirectoryName(output) + @"\" + Path.GetFileNameWithoutExtension(output)).Length - 1;
                                } while (output.Length > 250);
                            }
                        }
                    }
                }

                if (output.Contains(".php")) output = output.Replace(".php", ".html");

                return output;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get filename from string '" + input + "'.", ex);
            }
        }

        private static bool FixAllHyperlinks()
        {
            try
            {
                f = new FTPclient(hostName, userName, password);

                //Get the list of all HTML files from the server
                string htmlsFile = Path.GetTempPath() + "CoH_HTMLs.txt";
                if (!File.Exists(htmlsFile))
                {
                    string htmls = "http://cohforums.cityofplayers.com/files/Coh_HTMLs.txt";
                    using (System.Net.WebClient wc = new System.Net.WebClient()) wc.DownloadFile(htmls, htmlsFile);
                }

                IEnumerable<string> htmlsContent = File.ReadLines(htmlsFile);
                Dictionary<string, string> htmlFiles = new Dictionary<string, string>();
                foreach (string line in htmlsContent)
                {
                    string[] kvp = line.Split(',');
                    if (!htmlFiles.ContainsKey(kvp[0])) htmlFiles.Add(kvp[0], kvp[1]);
                }

                //Do the following steps "forever", or until the user has cancelled the process or until the dictionary is empty
                bool cancel = false;
                bool reWroteLocalFile = false;
                do
                {
                    //Get the list of all already processed or processing files on the server and remove them from our dictionary
                    fProcessingFiles = f.ListDirectoryDetail(@"/processing/");
                    fProcessedFiles = f.ListDirectoryDetail(@"/processed/");
                    foreach (FTPfileInfo ffi in fProcessingFiles)
                    {
                        string fn = ffi.FullName.Replace("/processing/", "");

                        if (htmlFiles.ContainsKey(fn)) htmlFiles.Remove(htmlFiles[fn]);
                    }

                    foreach (FTPfileInfo ffi in fProcessedFiles)
                    {
                        string fn = ffi.FullName.Replace("/processed/", "");

                        if (htmlFiles.ContainsKey(fn)) htmlFiles.Remove(htmlFiles[fn]);
                    }

                    //Rewrite the local file if we haven't yet done it, to make it faster to load next time
                    if (!reWroteLocalFile)
                    {
                        //Resave the Dictionary file so that it loads that much faster next time.
                        if (File.Exists(htmlsFile)) File.Delete(htmlsFile);
                        using (StreamWriter sw = new StreamWriter(htmlsFile, true))
                        {
                            foreach (KeyValuePair<string, string> kvp in htmlFiles)
                            {
                                sw.WriteLine(kvp.Key + "," + kvp.Value);
                            }
                        }
                        reWroteLocalFile = true; //set the flag so we don't write again until program restart
                    }

                    //Pull a random file
                    int fileNum = new Random().Next(htmlFiles.Count);
                    List<string> keys = new List<string>();
                    foreach (KeyValuePair<string, string> kvp in htmlFiles) keys.Add(kvp.Key);
                    string key = keys[fileNum];

                    if (!htmlFiles.ContainsKey(key)) continue; //Just skip if this one isn't in the list

                    HtmlAgilityPack.HtmlWeb hw = new HtmlAgilityPack.HtmlWeb();
                    string webPageToLoad = fileDir + htmlFiles[key];
                    HtmlAgilityPack.HtmlDocument hd = hw.Load(webPageToLoad);
                    string newFileName = Path.GetTempPath() + keys[fileNum];
                    hd.Save(newFileName);

                    //If we've successfully loaded the document, copy it over to the processing directory
                    if (!f.Upload(new FileInfo(newFileName), "/processing/" + Path.GetFileName(newFileName)))
                    {
                        Console.WriteLine("Failed to upload file '" + newFileName);
                        continue; //Move onto the next one
                    }

                    //Now, start processing the file, get all of the HTML nodes and replace them (if appropriate)
                    foreach(HtmlAgilityPack.HtmlNode n in hd.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        //Check if the sanitized filename matches one in the key list
                        HtmlAgilityPack.HtmlAttribute a = n.Attributes["href"];
                        string FixedURL = GetSanitizedFileName(a.Value, "http_boards.cityofheroes.com_");
                        //If so, replace it with the relative fixed URL
                        if (keys.Contains(FixedURL))
                        {
                            a.Value = htmlFiles[FixedURL];
                        }
                    }

                    //Save the resulting file
                    hd.Save(newFileName);

                    //Upload the resulting file to the correct relative directory (creating the directory if it doesn't exist)
                    if (!f.FtpFileExists(Path.GetFileName(newFileName))) f.FtpCreateDirectory(Path.GetFileName(newFileName));

                    if (!f.Upload(new FileInfo(newFileName), "/processed/" + newFileName))
                    {
                        Console.WriteLine("Failed to upload file '" + newFileName);
                        continue; //Move onto the next one
                    }

                    //Move the completed remote file to the completed pile



                } while (htmlFiles.Count > 0 && !cancel);




                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        private static bool GetURLs(string sourceDir)
        {
            try
            {

                //Write to the console so that we know it's working
                Console.WriteLine("Handling directory '" + sourceDir + "'.");

                //Get all files in the directory
                string[] files = Directory.GetFiles(sourceDir);

                string siteMapName = sourceDir + @"\sitemap.txt";
                if (File.Exists(siteMapName)) File.Delete(siteMapName);

                //Add a single URL for each file
                foreach (string file in files)
                {
                    using (StreamWriter sw = new StreamWriter(siteMapName, true))
                    {
                        string newFileName = file.Replace(MainSourceDir, urlBase).Replace(@"\", "/");
                        sw.WriteLine(newFileName);

                    };
                }

                //Add this sitemap to the robots file
                string robotsName = MainSourceDir + @"\robots.txt";
                if (sourceDir == MainSourceDir && File.Exists(robotsName))
                {
                    File.Delete(robotsName);
                    using (StreamWriter sw = new StreamWriter(robotsName, true))
                    {
                        sw.WriteLine(@"User-agent: *
Allow: /

                        ");
                    }
                }
                using (StreamWriter sw = new StreamWriter(robotsName, true))
                {
                    sw.WriteLine("Sitemap: " + sourceDir.Replace(MainSourceDir, urlBase).Replace(@"\", "/") + @"/sitemap.txt");
                }

                //Finally, check for subdirectories and process them as well
                string[] dirs = Directory.GetDirectories(sourceDir);
                if (dirs.Length > 0)
                {
                    foreach (string dir in dirs) GetURLs(dir);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        private static bool CreateSiteMapIndexFile(string rootDir,string urlRoot)
        {
            try
            {
                if (!rootDir.EndsWith(@"\")) rootDir += @"\";
                string siteMapsDir = rootDir + @"sitemaps\";
                if (!Directory.Exists(siteMapsDir)) Directory.CreateDirectory(siteMapsDir);

                //Get all directories
                string[] dirs = Directory.GetDirectories(rootDir, "*", SearchOption.AllDirectories);

                string siteMapIndexName = "";
                if (File.Exists(siteMapIndexName)) File.Delete(siteMapIndexName);

                XNamespace xn = "http://www.sitemaps.org/schemas/sitemap/0.9";
                XElement smi = new XElement(xn + "sitemapindex");
                XDocument doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), smi);
                XElement root = doc.Element(xn + "sitemapindex");

                int numFiles = 0;
                int siteMapIndexNum = 0;
                int siteMapNum = 0;
                //Add a single URL for each sitemap file
                foreach (string dir in dirs)
                {
                    //Start generating sitemap files on the fly, break it up to every 50,000 files
                    string[] files = Directory.GetFiles(dir);

                    foreach (string file in files)
                    {
                        numFiles++;
                        if (siteMapNum == 0 || numFiles > 50000)
                        {
                            //Add this sitemap to the sitemap index file
                            if (siteMapNum > 500 throw new Exception("Can no longer generate "))

                            //Start a new sitemap
                            siteMapNum++;
                            numFiles = 1;
                        }
                        else
                        {
                            using (StreamWriter sw = new StreamWriter(siteMapsDir + "SiteMap" + siteMapNum + ".txt"))
                            {
                                sw.WriteLine(file);
                            }
                        }
                    }

                    XElement sm = new XElement(xn + "sitemap");
                    sm.Add(new XElement(xn + "loc", file));
                    root.Add(sm);
                }

                doc.Save(siteMapIndexName);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }
    }
}
