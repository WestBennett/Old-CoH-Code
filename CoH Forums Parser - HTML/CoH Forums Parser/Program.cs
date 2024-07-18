using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CoH_Forums_Parser
{
    class Program
    {

        const string ConnectionString = ""; //MySql Connection string

        const int Version = 9001;
        static string WorkingDirectory;
        static string OutputPath;
        static string ProcessedDirectory;
        const string SettingsFileName = "Settings.ini";

        [STAThread]
        static void Main(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg == "GetUsersInThreads")
                {
                    GetAllUsersInThreads();
                    return;
                }
            }

            string FileInProcess = "";

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Choose the folder where you have the uncompressed .warc files at";
            if (fbd.ShowDialog() == DialogResult.Cancel) return;
            if (!Directory.Exists(fbd.SelectedPath)) return;
            List<string> filesToProcess = Directory.GetFiles(fbd.SelectedPath, "*.warc").ToList();

            if (filesToProcess.Count == 0)
            {
                MessageBox.Show("No .warc files found to process in this directory!", "No Files Found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            WorkingDirectory = fbd.SelectedPath;
            OutputPath = WorkingDirectory + @"\output\";
            if (!Directory.Exists(OutputPath)) Directory.CreateDirectory(OutputPath);
            ProcessedDirectory = WorkingDirectory + @"\processed\";
            if (!Directory.Exists(ProcessedDirectory)) Directory.CreateDirectory(ProcessedDirectory);

            string UserName = "";
            if (File.Exists(WorkingDirectory + SettingsFileName))
            {
                List<string> Settings = File.ReadLines(WorkingDirectory + SettingsFileName).ToList();
                foreach (string setting in Settings)
                {
                    if (setting.Trim() == "") continue;
                    if (setting.StartsWith("UserName: "))
                    {
                        UserName = setting.Split(new string[] { ": " }, StringSplitOptions.None)[1].Trim();
                        if (UserName == "")
                        {
                            MessageBox.Show("Cannot continue without a valid UserName.",
                                "No UserName Entered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
            }
            else
            {
                UserName = Interaction.InputBox(
                "Please input an anonymous UserName to store in the database as the parser of your files. " +
                "This is required to give you credit for your efforts.", "Input UserName", "");
                if (UserName == "")
                {
                    MessageBox.Show("Cannot continue without a UserName.", "No UserName Entered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                File.WriteAllText(WorkingDirectory + SettingsFileName, "UserName: " + UserName);
            }

            List<string> filesToSkip = Directory.GetFiles(fbd.SelectedPath, "*.warcSkip").ToList();

            string IPaddress;
            IPaddress = new WebClient().DownloadString("http://icanhazip.com");
            int FileNum = 0;

            do
            {
                try
                {

                    Application.DoEvents();

                    string AppName = Application.ProductName;
                    if (!UpToDateVersion(AppName))
                    {
                        MessageBox.Show("Program is out of date. Please find and download the latest version.", "Old Version", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    //Move all already processed files, and re-get the list
                    List<string> filesProcessed = GetData("SELECT * FROM files").Rows.OfType<DataRow>()
                        .Select(dr => dr.Field<string>("FileName")).ToList();
                    filesToProcess = Directory.GetFiles(fbd.SelectedPath, "*.warc").ToList();
                    foreach (string fileToCheck in filesToProcess)
                    {
                        if (filesProcessed.Contains(Path.GetFileNameWithoutExtension(fileToCheck)))
                        {
                            try
                            {
                                File.Move(fileToCheck, ProcessedDirectory + Path.GetFileName(fileToCheck));
                            }
                            catch
                            {
                                //Don't care, it's already deleted
                            }

                            Console.WriteLine("Moved already processed file '" + fileToCheck + "'");
                        }
                        else
                        {
                            //Do nothing
                            Debug.WriteLine("");
                        }
                    }
                    filesToProcess = Directory.GetFiles(fbd.SelectedPath, "*.warc").ToList();

                    //Go to a random file
                    Random r = new Random();
                    int FileNumToProcess = r.Next(0, filesToProcess.Count - 1);
                    string file = filesToProcess[FileNumToProcess];

                    FileInProcess = file;

                    FileNum++;

                    if (FileProcessed(Path.GetFileNameWithoutExtension(file)))
                    {
                        Console.WriteLine("File " + FileInProcess + " has already been processed, skipping and moving.");
                        File.Move(file, ProcessedDirectory + Path.GetFileName(file));
                        filesToProcess.Remove(file);
                        continue;
                    }

                    if (filesToSkip.Contains(file.Replace(".warc", ".warcSkip")))
                    {
                        Console.WriteLine("File " + FileInProcess + " has already been processed, skipping and moving.");
                        File.Move(file, ProcessedDirectory + Path.GetFileName(file));
                        filesToProcess.Remove(file);
                        continue;
                    }

                    Console.WriteLine("Now processing file # " + (FileNumToProcess + 1) + " of " + filesToProcess.Count + ". Filename - " +
                        Path.GetFileNameWithoutExtension(file));

                    bool FoundNewHeader = false;
                    bool InContent = false;
                    bool FinishingHeader = false;
                    int SkipLine = 0;
                    string URL = null;
                    DateTime? Date = null;

                    StringBuilder Content = new StringBuilder();

                    int LineNum = 0;
                    int NumLines = File.ReadLines(file).Count();
                    using (StreamReader sr = new StreamReader(file))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            string LineToUse = line;
                            LineNum++;

                            Console.WriteLine("Now processing file # " + (FileNumToProcess + 1) + " of " + filesToProcess.Count +
                                 ". Filename - " + Path.GetFileNameWithoutExtension(file) + ", line " + LineNum + " of " + NumLines);

                            //Overwrite the line if we're counting it as a new header element
                            if (FoundNewHeader)
                            {
                                FoundNewHeader = false;
                                LineToUse = "WARC/1.0";
                            }

                            //WARC/1.0 line means we've reached another record, Downloaded means the end of the file
                            if ((LineToUse.StartsWith("WARC/1.0") || LineToUse.StartsWith("Downloaded: ")) && InContent)
                            {
                                ProcessContent(URL, Date, Content);
                                //Allows a bypass to overwrite the next line on the next loop
                                FoundNewHeader = true;
                                //Reset all appropriate variables
                                InContent = false;
                                FinishingHeader = false;
                                SkipLine = 0;
                                URL = null;
                                Date = null;
                                Content = new StringBuilder();
                            }
                            else if (LineToUse.StartsWith("WARC-Target-URI: "))
                            {
                                //Get the URL
                                URL = line.Split(new string[] { "WARC-Target-URI: " }, StringSplitOptions.None)[1];
                            }
                            else if (LineToUse.StartsWith("WARC-Date: "))
                            {
                                //Get the date scanned
                                string date = LineToUse.Split(new string[] { "WARC-Date: " }, StringSplitOptions.None)[1].Replace("T", " ").Replace("Z", "");
                                Date = DateTime.Parse(date);
                            }
                            else if (LineToUse.StartsWith("Transfer-Encoding:"))
                            {
                                //Note that we've finished the header, so that we can start skipping the next two lines
                                FinishingHeader = true;
                            }
                            else if (FinishingHeader)
                            {
                                //Skip the first line
                                if (SkipLine == 0)
                                {
                                    SkipLine++;
                                    continue;
                                }
                                else
                                {
                                    //Skip the second line and mark that  we're now in the Content
                                    FinishingHeader = false;
                                    InContent = true;
                                    FoundNewHeader = false;
                                    SkipLine = 0;
                                }
                            }
                            else if (InContent)
                            {
                                //Here's the main content
                                Content.AppendLine(LineToUse);
                            }
                            else
                            {
                                //We don't care about the rest of the header informationn
                            }
                        }
                    }
                    //Skip this file next time, since we're done processing it
                    if (!FileProcessed(Path.GetFileNameWithoutExtension(file)))
                    {
                        MarkFileProcessed(Path.GetFileNameWithoutExtension(file), UserName, IPaddress);
                        File.Move(file, ProcessedDirectory + Path.GetFileName(file));
                        filesToProcess.Remove(file);
                    }
                }
                catch (Exception ex)
                {
                    string ErrorFile = WorkingDirectory + @"\Errors\";
                    if (!Directory.Exists(ErrorFile)) Directory.CreateDirectory(ErrorFile);
                    ErrorFile += "Errors.log";
                    if (FileInProcess != "")
                    {
                        File.Move(FileInProcess, Path.GetDirectoryName(ErrorFile) + @"\" + Path.GetFileName(FileInProcess));
                        File.AppendAllText(ErrorFile, DateTime.Now.ToString() + " | " + ex.ToString() + Environment.NewLine);
                        File.AppendAllText(ErrorFile, "Error with file '" + FileInProcess +
                            @"' File has been moved to the \Errors\ directory . Noting and resetting loop.");
                    }
                    else
                    {
                        File.AppendAllText(ErrorFile, DateTime.Now.ToString() + " | " + ex.ToString() + Environment.NewLine);
                        File.AppendAllText(ErrorFile, "Error outside of the file loop. Noting and resetting loop.");
                    }
                    continue;
                }
            } while (filesToProcess.Count > 0);
        }

        private static void GetAllUsersInThreads()
        {
            for (int i = 0; i < 5000000; i++)
            {
                Application.DoEvents();
                Console.WriteLine("Thread found. Processing thread " + i + " of 5,000,000");
                DataTable allThreads = GetData("SELECT * FROM threads t WHERE t.ThreadID = " + i);
                if (allThreads == null || allThreads.Rows.Count == 0) continue;
                FindUsersInThread(i, allThreads.Rows[0]["ContentHTML"].ToString());
            }

            Debug.WriteLine("");

        }

        private static void ProcessContent(string URL, DateTime? date, StringBuilder content)
        {
            try
            {
                if (Regex.Matches(URL, "http://").Count > 1) return;

                    //Process the content, and place it into the appropriate tables
                    string Content = content.ToString().Replace("http://boards.cityofheroes.com/", "");

                //We only care about files that were actually on the server
                if (!URL.Contains("boards.cityofheroes.com") && !URL.Contains("boards.cityofvillains.com")) return;

                string newFileName = URL.Replace("http://boards.cityofheroes.com/", "").Replace("/", @"\");

                //Fix the filename, if applicable
                if (newFileName.ToUpper().Contains(".PHP"))
                {
                    newFileName = ConvertToWindowsFileName(newFileName) + ".html";
                }
                else
                {
                    newFileName = newFileName.Split('?')[0];
                }

                if (!Directory.Exists(Path.GetDirectoryName(newFileName)) && Path.GetDirectoryName(newFileName) != "")
                {
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(newFileName));
                    }
                    catch
                    {
                        //If we can't make the path, just try to save the file
                        newFileName = OutputPath + Path.GetFileName(newFileName);
                    }
                }

                //Fix the content links, if applicable
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(Content);
                foreach (HtmlNode node in GetHTMLNodeByTagAttributeLikeValue(htmlDoc.DocumentNode, "a", "href", "?"))
                {
                    //Try to fix links in the content
                    if (node.OuterHtml.Split(new string[] { @"href=""" }, StringSplitOptions.None)[1].Split('"')[0].Contains(".php?")) {
                        string link = node.OuterHtml.Split(new string[] { @"href=""" }, StringSplitOptions.None)[1].Split('"')[0];
                        string newlink = ConvertToWindowsFileName(link) + ".html";
                        if (newlink.Contains("_amp_"))
                        {
                            newlink = newlink.Replace("_amp_", "");
                        }
                        Content = Content.Replace(link, newlink);
                    } 

                    if (node.OuterHtml.Contains("boards.cityofheroes.com") || node.OuterHtml.Contains("boards.cityofvillains.com"))
                    {
                        Content = Content.Replace(node.OuterHtml, ConvertToWindowsFileName(node.OuterHtml) + ".html").Replace(
                            "http://boards.cityofheroes.com/", "").Replace("http://boards.cityofvillains.com/", "");
                    }
                }

                File.WriteAllText(OutputPath + newFileName, Content);
                //Skip all this below, as we're just getting the file itself

                return;

                //If it's a user page, then add the user
                if (URL.StartsWith("http://boards.cityofheroes.com/member.php?u="))
                {
                    AddUser(URL, date, Content);
                    return;
                }
                //If it's a thread page, then add the thread and all of the posts in it
                else if (URL.StartsWith("http://boards.cityofheroes.com/showthread.php") && !Content.Contains("No Thread specified"))
                {
                    AddThread(URL, date, content);
                    return;
                }
                //If it's a solitary post, add it
                else if (URL.StartsWith("http://boards.cityofheroes.com/showpost.php"))
                {
                    string postID = URL.Split(new string[] { "p=" }, StringSplitOptions.None)[1].Split('&')[0];
                    //HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                    htmlDoc.LoadHtml(content.ToString());
                    int ThreadID = GetThreadIDFromPost(content.ToString(), htmlDoc.DocumentNode);
                    if (ThreadID == -1)
                    {
                        //Add as page, since we couldn't get the ThreadID
                        AddDBPage("Unable-To-Parse-Post-" + Path.GetTempFileName(), date, content.ToString());
                    }
                    else
                    {
                        AddPost(htmlDoc.DocumentNode.SelectNodes("*"), ThreadID, URL, date, Content);
                    }
                }
                else
                {
                    //If it's anything else, just add it to the generic pages table
                    AddDBPage(URL, date, content.ToString());
                    return;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error in ProcessContent", ex);
            }
        }

        private static int GetThreadIDFromPost(string v, HtmlNode ParentNode)
        {
            string ThreadID = "";
            foreach (HtmlNode node in GetHTMLNodeByTagAttribute(ParentNode, "a", "href"))
            {
                if (!node.OuterHtml.Contains("showthread.php")) continue;
                try
                {
                    ThreadID = node.OuterHtml.Split(new string[] { "showthread.php?p=" }, StringSplitOptions.None)[1].Split('#')[0];
                }
                catch
                {
                    return -1;
                }
                break;
            }
            if (ThreadID == "") return 0;
            ThreadID = ThreadID.Split('\n')[0].Split('\r')[0];
            int ReturnThread;
            try
            {
                ReturnThread = int.Parse(ThreadID);
            }
            catch
            {
                return -1;
            }

            return ReturnThread;
        }

        private static void AddThread(string URL, DateTime? date, StringBuilder content)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(content.ToString());

            //Get the Header information to make sure that formatting shows
            HtmlNode head = htmlDoc.DocumentNode.SelectSingleNode("/html/head");
            string Content = content.ToString();

            //Now, get the posts
            HtmlNodeCollection startNodes = htmlDoc.DocumentNode.SelectNodes("//comment()[contains(., 'BEGIN TEMPLATE: postbit_legacy')]");
            HtmlNodeCollection endNodes = htmlDoc.DocumentNode.SelectNodes("//comment()[contains(., 'END TEMPLATE: postbit_legacy')]");

            if (startNodes == null || startNodes.Count == 0) return;

            StringBuilder InnerHTML = new StringBuilder();

            //Enumerate through the nodes
            for (int i = 0; i < startNodes.Count - 1; i++)
            {
                int startNodeIndex = startNodes[i].ParentNode.ChildNodes.IndexOf(startNodes[i]);
                int endNodeIndex = 0;
                try
                {
                    endNodeIndex = endNodes[i].ParentNode.ChildNodes.IndexOf(endNodes[i]);
                }
                catch
                {
                    //If there's an error here, then we can't parse this thread, since the comments aren't linked up. Instead, just add it as a Page blindly
                    AddDBPage(URL, date, content.ToString());
                    return;
                }

                IEnumerable<HtmlNode> nodes = startNodes[i].ParentNode.ChildNodes.Where((n, index) => index >= startNodeIndex && index <= endNodeIndex).Select(n => n);
                if (nodes == null || nodes.Count() == 0) continue;
                int PostID = GetPostID(nodes);
                if (PostID == -1)
                {
                    AddDBPage("Unable-To-Parse-Post-" + Path.GetTempFileName(), date, Content);
                    return;
                }
                //We found no post ID, no post in this set of nodes or If the post has already been logged, don't log it again!
                if (PostID == 0) continue;

                //Log the post
                AddPost(nodes, int.Parse(URL.Split(new string[] { URL.Contains("t=") ? "t=" : "p=" }, StringSplitOptions.None)[1].Split('&')[0]), URL, date, Content);
            }

            int ThreadID = int.Parse(URL.Split(new string[] { URL.Contains("t=") ? "t=" : "p=" }, StringSplitOptions.None)[1].Split('&')[0]);
            if (!Content.Contains("<title>"))
            {
                AddDBPage(URL, date, content.ToString());
                return;
            }
            string Title = Content.Split(new string[] { "<title>" }, StringSplitOptions.None)[1].Split(new string[] { " - " }, StringSplitOptions.None)[0].Trim();
            AddDBThread(ThreadID, date, Content, Title, URL);
            FindUsersInThread(ThreadID, Content);

            return;
        }

        private static void FindUsersInThread(int threadID, string content)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(content.ToString());

            //Get the Header information to make sure that formatting shows
            foreach (HtmlNode node in GetHTMLNodeByTagAttributeLikeValue(htmlDoc.DocumentNode, "a", "href", "member.php?u="))
            {
                string User = node.OuterHtml.Split(new string[] { "u=" }, StringSplitOptions.None)[1].Split('"')[0];
                if (User.Contains('\r'))
                {
                    int test = 0;
                    foreach (string possibleNum in User.Split('\r'))
                    {
                        if (int.TryParse(possibleNum, out test)) break;
                    }
                    if (test != 0) User = test.ToString();
                }
                Console.WriteLine("Adding User " + int.Parse(User) + " in thread " + threadID);
                Application.DoEvents();
                AddDBUserInThread(int.Parse(User), threadID);
            }
        }

        private static void AddUser(string URL, DateTime? date, string content)
        {
            string UserName;
            try
            {
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(content.ToString());

                int UserID = int.Parse(URL.Split(new string[] { "u=" }, StringSplitOptions.None)[1]);
                UserName = htmlDoc.DocumentNode.InnerHtml.Split(new string[] { "ile: " }, StringSplitOptions.None)[1];
                UserName = UserName.Split(new string[] { "</title>" }, StringSplitOptions.None)[0];

                AddDBUser(UserID, UserName, content.ToString(), URL);

                return;
            }
            catch
            {
                //Just add as a page, if we can't parse it
                AddDBPage(URL, date, content);
            }
        }

        private static void AddPost(IEnumerable<HtmlNode> nodes, int ThreadID, string URL, DateTime? dateForLogging, string contentForLogging)
        {
            StringBuilder sb = new StringBuilder();
            //Get the PostID
            int PostID = GetPostID(nodes);
            if (PostID == -1)
            {
                AddDBPage("Unable-To-Parse-Post-" + Path.GetTempFileName(), dateForLogging, contentForLogging);
                return;
            }
            string Content = nodes.First().ParentNode.InnerHtml;
            //Get the Date, should be the first thead element
            string date = "";
            try
            {
                date = GetHTMLNodeByTagAttributeValue((nodes.First()).ParentNode, "td", "class", "thead")[0].InnerHtml.Trim();
            }
            catch
            {
                //Not a valid Page even, ignore it and move on
                return;
            }

            DateTime Date = DateTime.MinValue;
            //try to parse the date
            if (date.Split('\n').Length > 1)
            {
                foreach (string possibleDate in date.Split('\n'))
                {
                    string stringToUse = possibleDate.Trim();
                    //Bugfix for some of these weird entries that I found that have "Date until Date" I don't know what they're for, so we'll just take the earlier date
                    if (stringToUse.Contains(" until ")) stringToUse = possibleDate.Split(new string[] { " until " }, StringSplitOptions.None)[0].Trim();

                    if (!DateTime.TryParse(stringToUse, out Date)) continue;
                    break;
                }
            }
            if (Date == DateTime.MinValue)
            {
                //If we can't find a Datetime, just count it as a page.
                AddDBPage(URL, DateTime.Now, Content);
                return;
            }

            string content = nodes.First().ParentNode.InnerHtml;

            //Get the UserName and ID - add user if it doesn't exist
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(content.ToString());
            //Find by the style, depending on the user's name color
            try
            {
                string UserName = GetHTMLNodeByTagAttributeValue(htmlDoc.DocumentNode, "span", "style",
                    content.Contains("color: White; font-weight: bold;") ? "color: White; font-weight: bold;" :
                    content.Contains("color: red; font-weight: bold;") ? "color: red; font-weight: bold;" :
                    content.Contains("color: GoldenRod; font-weight: bold;") ? "color: GoldenRod; font-weight: bold;" : null)[0].InnerHtml;
            }
            catch
            {
                //Just add it as a page if we can't parse the UserName
                AddDBPage("Unable-To-Parse-Post-" + Path.GetTempFileName(), Date, content);
            }

            string userID = content.Split(new string[] { "u=" }, StringSplitOptions.None)[1].Split(new string[] { @""">" }, StringSplitOptions.None)[0];
            int UserID = 0;
            if (userID.Contains('\r'))
            {
                foreach (string userid in userID.Split('\r'))
                {
                    if (!int.TryParse(userid, out UserID)) continue;
                    userID = userid;
                    break;
                }
            }
            UserID = int.Parse(userID.Split(' ')[0].Replace(@"""", "").Replace("=", ""));
            //Get the content
            foreach (HtmlNode node in nodes)
            {
                string HTML = node.InnerHtml;
                sb.AppendLine(HTML);
            }
            string PostData = sb.ToString();

            AddDBPost(ThreadID, PostID, UserID, Date, PostData, URL);

            return;
        }

        private static int GetPostID(IEnumerable<HtmlNode> nodes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (HtmlNode node in nodes)
            {
                string HTML = node.InnerHtml;
                sb.AppendLine(HTML);
            }
            string content = sb.ToString();
            List<string> elements = content.Split('"').ToList();
            foreach (string element in elements)
            {
                if (element.StartsWith("post_message_"))
                {
                    return int.Parse(element.Replace("post_message_", ""));
                }
                else if (element.StartsWith("showpost.php?p="))
                {
                    int PostID = 0;
                    string fixedElement = element.Replace("showpost.php?p=", "").Split('&')[0];
                    if (element.Contains('\r'))
                    {
                        foreach (string e in element.Split('\r'))
                        {
                            if (!int.TryParse(e, out PostID)) continue;
                            fixedElement = e;
                            break;
                        }
                    }
                    if (!int.TryParse(fixedElement, out PostID))
                    {
                        //Try a fallback method to find it elsewhere
                        if (!content.Contains("td_post_"))
                        {
                            return 0;
                        }
                        fixedElement = content.Split(new string[] { "td_post_" }, StringSplitOptions.None)[1].Split('"')[0];
                    }

                    int RetVal;
                    try
                    {
                        RetVal = int.Parse(fixedElement.Replace("showpost.php?p=", "").Split('&')[0]);
                    }
                    catch
                    {
                        return -1;
                    }

                    return RetVal;
                }
            }
            return 0;
        }

        static private HtmlNodeCollection GetHTMLNodeByTagAttributeValue(HtmlNode ParentNode, string tag, string attribute, string value)
        {
            try
            {
                HtmlNodeCollection returnValue = new HtmlNodeCollection(ParentNode);
                HtmlNodeCollection nodes = GetAllNodes(ParentNode);

                foreach (HtmlNode node in nodes)
                {
                    if (node.Name != tag) continue;
                    if (!node.Attributes.Contains(attribute)) continue;
                    if (node.Attributes[attribute].Value.ToString() != value) continue;
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

        static private HtmlNodeCollection GetHTMLNodeByTagAttribute(HtmlNode ParentNode, string tag, string attribute)
        {
            try
            {
                HtmlNodeCollection returnValue = new HtmlNodeCollection(ParentNode);
                HtmlNodeCollection nodes = GetAllNodes(ParentNode);

                foreach (HtmlNode node in nodes)
                {
                    if (node.Name != tag) continue;
                    if (!node.Attributes.Contains(attribute)) continue;
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

        static private HtmlNodeCollection GetHTMLNodeByTag(HtmlNode ParentNode, string tag)
        {
            try
            {
                HtmlNodeCollection returnValue = new HtmlNodeCollection(ParentNode);
                HtmlNodeCollection nodes = GetAllNodes(ParentNode);

                foreach (HtmlNode node in nodes)
                {
                    if (node.Name != tag) continue;
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

        static private void AddDBPage(string url, DateTime? DateSaved, string ContentHTML)
        {
            using (MySqlConnection s = new MySqlConnection(ConnectionString))
            {
                using (MySqlCommand c = new MySqlCommand("AddPage", s))
                {
                    c.CommandType = CommandType.StoredProcedure;
                    TripleCheckOpen(s);
                    c.Parameters.AddWithValue(nameof(url), url);
                    c.Parameters.AddWithValue(nameof(DateSaved), DateSaved);
                    c.Parameters.AddWithValue(nameof(ContentHTML), ContentHTML);
                    try
                    {
                        TripleCheckExecute(c, false);
                    }
                    catch
                    {
                        //Don't care
                    }
                }
            }
        }

        static private void AddDBPost(int ThreadID, int PostID, int UserID, DateTime? DatePosted, string ContentHTML, string URL)
        {
            using (MySqlConnection s = new MySqlConnection(ConnectionString))
            {
                using (MySqlCommand c = new MySqlCommand("AddPost", s))
                {
                    c.CommandType = CommandType.StoredProcedure;
                    TripleCheckOpen(s);
                    c.Parameters.AddWithValue(nameof(ThreadID), ThreadID);
                    c.Parameters.AddWithValue(nameof(PostID), PostID);
                    c.Parameters.AddWithValue(nameof(UserID), UserID);
                    c.Parameters.AddWithValue(nameof(DatePosted), DatePosted);
                    c.Parameters.AddWithValue(nameof(ContentHTML), ContentHTML);
                    c.Parameters.AddWithValue(nameof(URL), URL);
                    try
                    {
                        TripleCheckExecute(c, false);
                    }
                    catch
                    {
                        //Don't care
                    }
                }
            }
        }

        static private void AddDBThread(int ThreadID, DateTime? DateSaved, string ContentHTML, string Title, string URL)
        {
            using (MySqlConnection s = new MySqlConnection(ConnectionString))
            {
                using (MySqlCommand c = new MySqlCommand("AddThread", s))
                {
                    c.CommandType = CommandType.StoredProcedure;
                    TripleCheckOpen(s);
                    c.Parameters.AddWithValue(nameof(ThreadID), ThreadID);
                    c.Parameters.AddWithValue(nameof(DateSaved), DateSaved);
                    c.Parameters.AddWithValue(nameof(ContentHTML), ContentHTML);
                    c.Parameters.AddWithValue(nameof(Title), Title);
                    c.Parameters.AddWithValue(nameof(URL), URL);
                    try
                    {
                        TripleCheckExecute(c, false);
                    }
                    catch
                    {
                        //Don't care
                    }
                }
            }
        }

        static private void AddDBUser(int UserID, string UserName, string ContentHTML, string URL)
        {
            using (MySqlConnection s = new MySqlConnection(ConnectionString))
            {
                using (MySqlCommand c = new MySqlCommand("AddUser", s))
                {
                    c.CommandType = CommandType.StoredProcedure;
                    TripleCheckOpen(s);
                    c.Parameters.AddWithValue(nameof(UserID), UserID);
                    c.Parameters.AddWithValue(nameof(UserName), UserName);
                    c.Parameters.AddWithValue(nameof(ContentHTML), ContentHTML);
                    c.Parameters.AddWithValue(nameof(URL), URL);
                    try
                    {
                        TripleCheckExecute(c, false);
                    }
                    catch
                    {
                        //Don't care
                    }
                }
            }
        }

        static private void AddDBUserInThread(int User, int Thread)
        {
            using (MySqlConnection s = new MySqlConnection(ConnectionString))
            {
                using (MySqlCommand c = new MySqlCommand("AddUserInThread", s))
                {
                    c.CommandType = CommandType.StoredProcedure;
                    TripleCheckOpen(s);
                    c.Parameters.AddWithValue(nameof(User), User);
                    c.Parameters.AddWithValue(nameof(Thread), Thread);
                    try
                    {
                        TripleCheckExecute(c, false);
                    }
                    catch
                    {
                        //Don't care
                    }
                }
            }
        }

        //static private bool DBPageExists(string Page)
        //{
        //    bool ReturnValue = false;
        //    using (MySqlConnection s = new MySqlConnection(ConnectionString))
        //    {
        //        using (MySqlCommand c = new MySqlCommand("PageExists", s))
        //        {
        //            c.CommandType = CommandType.StoredProcedure;
        //            TripleCheckOpen(s);
        //            c.Parameters.AddWithValue('@' + nameof(Page), Page);
        //            c.Parameters.Add(new MySqlParameter("retval", MySqlDbType.Int32));
        //            c.Parameters["retval"].Direction = ParameterDirection.ReturnValue;
        //            TripleCheckExecute(c);
        //            ReturnValue = int.Parse(c.Parameters["retval"].Value.ToString()) == 1 ? true : false;
        //        }
        //    }
        //    return ReturnValue;
        //}

        //static private bool DBPostExists(int Post)
        //{
        //    bool ReturnValue = false;
        //    using (MySqlConnection s = new MySqlConnection(ConnectionString))
        //    {
        //        using (MySqlCommand c = new MySqlCommand("PostExists", s))
        //        {
        //            c.CommandType = CommandType.StoredProcedure;
        //            TripleCheckOpen(s);
        //            c.Parameters.AddWithValue('@' + nameof(Post), Post);
        //            c.Parameters.Add(new MySqlParameter("retval", MySqlDbType.Int32));
        //            c.Parameters["retval"].Direction = ParameterDirection.ReturnValue;
        //            TripleCheckExecute(c);
        //            ReturnValue = int.Parse(c.Parameters["retval"].Value.ToString()) == 1 ? true : false;
        //        }
        //    }
        //    return ReturnValue;
        //}

        //static private bool DBThreadExists(int Thread)
        //{
        //    bool ReturnValue = false;
        //    using (MySqlConnection s = new MySqlConnection(ConnectionString))
        //    {
        //        using (MySqlCommand c = new MySqlCommand("ThreadExists", s))
        //        {
        //            c.CommandType = CommandType.StoredProcedure;
        //            TripleCheckOpen(s);
        //            c.Parameters.AddWithValue('@' + nameof(Thread), Thread);
        //            c.Parameters.Add(new MySqlParameter("retval", MySqlDbType.Int32));
        //            c.Parameters["retval"].Direction = ParameterDirection.ReturnValue;
        //            TripleCheckExecute(c);
        //            ReturnValue = int.Parse(c.Parameters["retval"].Value.ToString()) == 1 ? true : false;
        //        }
        //    }
        //    return ReturnValue;
        //}

        //static private bool DBUserExists(int User)
        //{
        //    bool ReturnValue = false;
        //    using (MySqlConnection s = new MySqlConnection(ConnectionString))
        //    {
        //        using (MySqlCommand c = new MySqlCommand("UserExists", s))
        //        {
        //            c.CommandType = CommandType.StoredProcedure;
        //            TripleCheckOpen(s);
        //            c.Parameters.AddWithValue('@' + nameof(User), User);
        //            c.Parameters.Add(new MySqlParameter("retval", MySqlDbType.Int32));
        //            c.Parameters["retval"].Direction = ParameterDirection.ReturnValue;
        //            TripleCheckExecute(c);
        //            ReturnValue = int.Parse(c.Parameters["retval"].Value.ToString()) == 1 ? true : false;
        //        }
        //    }
        //    return ReturnValue;
        //}

        //static private bool DBUserInThreadExists(int User, int Thread)
        //{
        //    bool ReturnValue = false;
        //    using (MySqlConnection s = new MySqlConnection(ConnectionString))
        //    {
        //        using (MySqlCommand c = new MySqlCommand("UserInThreadExists", s))
        //        {
        //            c.CommandType = CommandType.StoredProcedure;
        //            TripleCheckOpen(s);
        //            c.Parameters.AddWithValue('@' + nameof(User), User);
        //            c.Parameters.AddWithValue('@' + nameof(Thread), Thread);
        //            c.Parameters.Add(new MySqlParameter("retval", MySqlDbType.Int32));
        //            c.Parameters["retval"].Direction = ParameterDirection.ReturnValue;
        //            TripleCheckExecute(c);
        //            ReturnValue = int.Parse(c.Parameters["retval"].Value.ToString()) == 1 ? true : false;
        //        }
        //    }
        //    return ReturnValue;
        //}

        static private bool FileProcessed(string File)
        {
            bool ReturnValue = false;
            using (MySqlConnection s = new MySqlConnection(ConnectionString))
            {
                using (MySqlCommand c = new MySqlCommand("FileProcessed", s))
                {
                    c.CommandType = CommandType.StoredProcedure;
                    TripleCheckOpen(s);
                    c.Parameters.AddWithValue('@' + nameof(File), File);
                    c.Parameters.Add(new MySqlParameter("retval", MySqlDbType.Int32));
                    c.Parameters["retval"].Direction = ParameterDirection.ReturnValue;
                    TripleCheckExecute(c);
                    ReturnValue = int.Parse(c.Parameters["retval"].Value.ToString()) == 1 ? true : false;
                }
            }
            return ReturnValue;
        }

        static private bool UpToDateVersion(string ProgramName)
        {
            bool ReturnValue = false;
            using (MySqlConnection s = new MySqlConnection(ConnectionString))
            {
                using (MySqlCommand c = new MySqlCommand("CheckVersion", s))
                {
                    c.CommandType = CommandType.StoredProcedure;
                    TripleCheckOpen(s);
                    c.Parameters.AddWithValue('@' + nameof(ProgramName), ProgramName);
                    c.Parameters.Add(new MySqlParameter("retval", MySqlDbType.Int32));
                    c.Parameters["retval"].Direction = ParameterDirection.ReturnValue;
                    TripleCheckExecute(c);
                    ReturnValue = Version >= int.Parse(c.Parameters["retval"].Value.ToString()) ? true : false;
                }
            }
            return ReturnValue;
        }

        static private void MarkFileProcessed(string FileName, string ProcessedBy, string ProcessedByIP)
        {
            using (MySqlConnection s = new MySqlConnection(ConnectionString))
            {
                using (MySqlCommand c = new MySqlCommand("MarkFileProcessed", s))
                {
                    c.CommandType = CommandType.StoredProcedure;
                    TripleCheckOpen(s);
                    c.Parameters.AddWithValue(nameof(FileName), FileName);
                    c.Parameters.AddWithValue(nameof(ProcessedBy), ProcessedBy);
                    c.Parameters.AddWithValue(nameof(ProcessedByIP), ProcessedByIP);
                    TripleCheckExecute(c, false);
                }
            }
        }

        public static DataTable GetData(string command)
        {
            DataTable dtResult = new DataTable();
            try
            {
                using (MySqlConnection s = new MySqlConnection(ConnectionString))
                {

                    using (MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(command, s))
                    {
                        myDataAdapter.Fill(dtResult);
                        return dtResult;
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error obtaining data.", ex);
            }
        }

        public static bool TripleCheckOpen(MySqlConnection c)
        {
            Exception exReport;
            int Attempts = 0;
            do
            {
                try
                {
                    c.Open();
                    return true;
                }
                catch (Exception ex)
                {
                    //Ignore until we've tried three times.
                    exReport = ex;
                }
                Attempts++;
            } while (Attempts < 3);

            throw new Exception("Failed to connect to server over three times in a row. Server may be down. Please try again later.", exReport);
        }

        public static bool TripleCheckExecute(MySqlCommand c, bool scalar = true)
        {
            Exception exReport;
            int Attempts = 0;
            do
            {
                try
                {
                    if (scalar)
                    {
                        c.ExecuteScalar();
                    }
                    else
                    {
                        c.ExecuteNonQuery();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    //Ignore until we've tried three times.
                    exReport = ex;
                }
                Attempts++;
            } while (Attempts < 3);

            throw new Exception("Failed to connect to server over three times in a row. Server may be down. Please try again later.", exReport);
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
                rt = rt + urlParts[i];
                rt = rt + "_";
            }
            return rt;
        }
    }
}
