using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace COH_WARC_Processor
{
    // A wrapper class for .NET 2.0 FTP
    // This class does not hold open an FTP connection but 
    // instead is stateless: for each FTP request it 
    // connects, performs the request and disconnects.

    public class FTPclient
    {
        // Blank constructor
        // Hostname, username and password must be set manually</remarks>
        public FTPclient()
        {
        }

        // Constructor just taking the hostname
        // Hostname is either ftp://ftp.host.com or ftp.host.com form

        public FTPclient(string Hostname)
        {
            _hostname = Hostname;
        }

        // Constructor taking hostname, username and password
        // Hostname is either ftp://ftp.host.com or ftp.host.com form
        // Username Leave blank to use 'anonymous' but set password to your email
        // Password

        public FTPclient(string Hostname, string Username, string Password)
        {
            _hostname = Hostname;
            _username = Username;
            _password = Password;
        }

        // Return a simple directory listing
        // directory = Directory to list, e.g. /pub
        // returns A list of filenames and directories as a List(of String)
        // For a detailed directory listing, use ListDirectoryDetail

        public List<string> ListDirectory(string directory = "")
        {
            // return a simple list of filenames in directory
            System.Net.FtpWebRequest ftp = GetRequest(GetDirectory(directory));

            // Set request to do simple list
            ftp.Method = WebRequestMethods.Ftp.ListDirectory;

            string str = GetStringResponse(ftp);

            // replace CRLF to CR, remove last instance
            str = str.Replace("\r\n", "\r").TrimEnd((char)13);

            // split the string into a list

            List<string> result = new List<string>();

            result.AddRange(str.Split((char)13));

            return result;
        }


        // Return a detailed directory listing
        // directory - Directory to list, e.g. /pub/etc
        // returns :An FTPDirectory object

        public FTPdirectory ListDirectoryDetail(string directory = "")
        {
            System.Net.FtpWebRequest ftp = GetRequest(GetDirectory(directory));

            // Set request to do simple list
            ftp.Method = System.Net.WebRequestMethods.Ftp.ListDirectoryDetails;

            string str = GetStringResponse(ftp);

            // replace CRLF to CR, remove last instance
            str = str.Replace("\r\n", "\r").TrimEnd((char)13);

            // split the string into a list

            return new FTPdirectory(str, _lastDirectory);
        }

        // Copy a local file to the FTP server
        // localFilename - Full path of the local file
        // targetFilename - Target filename, if required
        // If the target filename is blank, the source filename is used
        // (assumes current directory). Otherwise use a filename to specify a name
        // or a full path and filename if required.</remarks>

        public bool Upload(string localFilename, string targetFilename = "")
        {

            // 1. check source
            if (!File.Exists(localFilename))
                throw new ApplicationException("File " + localFilename + " not found");

            // copy to FI
            FileInfo fi = new FileInfo(localFilename);

            return Upload(fi, targetFilename);
        }


        // Upload a local file to the FTP server
        // fi - Source file
        // targetFilename - Target filename (optional)

        public bool Upload(FileInfo fi, string targetFilename = "")
        {

            // copy the file specified to target file: target file can be full path or just filename (uses current dir)

            // 1. check target
            string target;

            if (targetFilename.Trim() == "")

                // Blank target: use source filename & current dir
                target = this.CurrentDirectory + fi.Name;
            else if (targetFilename.Contains("/"))

                // If contains / treat as a full path
                target = AdjustDir(targetFilename);
            else

                // otherwise treat as filename only, use current directory
                target = CurrentDirectory + targetFilename;

            string URI = Hostname + target;

            // perform copy

            System.Net.FtpWebRequest ftp = GetRequest(URI);



            // Set request to upload a file in binary

            ftp.Method = System.Net.WebRequestMethods.Ftp.UploadFile;

            ftp.UseBinary = true;



            // Notify FTP of the expected size

            ftp.ContentLength = fi.Length;



            // create byte array to store: ensure at least 1 byte!

            const int BufferSize = 2048;
            byte[] content = new byte[2048];
            int dataRead;

            int max;
            try
            {
                max = int.Parse(fi.Length.ToString());
            }
            catch (Exception ex)
            {
                max = 2147483647;
            }
            //Create a temporary windows form to show the progress
            System.Windows.Forms.Form progress = new System.Windows.Forms.Form
            {
                AutoSize = true,
                AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink,
                TopMost = true
            };
            System.Windows.Forms.ProgressBar pg = new System.Windows.Forms.ProgressBar
            {
                Maximum = max,
                Minimum = 0
            };
            progress.Controls.Add(pg);
            progress.Show();
            if (System.Windows.Forms.Application.OpenForms.Count > 0)
            {
                progress.Top = System.Windows.Forms.Application.OpenForms[0].Top + 5;
                progress.Left = System.Windows.Forms.Application.OpenForms[0].Left + 5;
            }
            progress.TopMost = false;

            // open file for reading 

            using (FileStream fs = fi.OpenRead())
            {
                try
                {

                    // open request to send

                    using (Stream rs = ftp.GetRequestStream())
                    {
                        do
                        {
                            dataRead = fs.Read(content, 0, BufferSize);
                            rs.Write(content, 0, dataRead);
                            try
                            {
                                pg.Value += dataRead;
                            }
                            catch (Exception ex)
                            {
                                pg.Value = 0;
                            }

                            progress.Refresh();
                            System.Windows.Forms.Application.DoEvents();
                        }
                        while (!(dataRead < BufferSize));

                        rs.Close();
                    };

                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to upload file.", ex);
                }

                finally
                {
                    // ensure file closed
                    fs.Close();
                }
            }

            progress.Hide();
            progress.Dispose();
            return true;
        }





        // Copy a file from FTP server to local
        // sourceFilename - Target filename, if required
        // localFilename - Full path of the local file
        // Target can be blank (use same filename), or just a filename
        // (assumes current directory) or a full path and filename

        public bool Download(string sourceFilename, string localFilename, bool PermitOverwrite = false)
        {

            // 2. determine target file

            FileInfo fi = new FileInfo(localFilename);

            return this.Download(sourceFilename, fi, PermitOverwrite);
        }



        // Version taking an FtpFileInfo

        public bool Download(FTPfileInfo file, string localFilename, bool PermitOverwrite = false)
        {
            return this.Download(file.FullName, localFilename, PermitOverwrite);
        }



        // Another version taking FtpFileInfo and FileInfo

        public bool Download(FTPfileInfo file, FileInfo localFI, bool PermitOverwrite = false)
        {
            return this.Download(file.FullName, localFI, PermitOverwrite);
        }



        // Version taking string/FileInfo

        public bool Download(string sourceFilename, FileInfo targetFI, bool PermitOverwrite = false)
        {
            try
            {
                // 1. check target

                if (targetFI.Exists & !(PermitOverwrite))
                    throw new ApplicationException("Target file already exists");



                // 2. check source

                string target;

                if (sourceFilename.Trim() == "")
                    throw new ApplicationException("File not specified");
                else if (sourceFilename.Contains("/"))

                    // treat as a full path

                    target = AdjustDir(sourceFilename);
                else

                    // treat as filename only, use current directory

                    target = CurrentDirectory + sourceFilename;



                string URI = Hostname + target;



                // 3. perform copy

                System.Net.FtpWebRequest ftp = GetRequest(URI);



                // Set request to download a file in binary mode

                ftp.Method = System.Net.WebRequestMethods.Ftp.DownloadFile;

                ftp.UseBinary = true;


                int max;
                try
                {
                    max = int.Parse(GetFileSize(sourceFilename).ToString());
                }
                catch (Exception ex)
                {
                    max = 2147483647;
                }
                
                //Create a temporary windows form to show the progress
                System.Windows.Forms.Form progress = new System.Windows.Forms.Form
                {
                    AutoSize = true,
                    AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink,
                    TopMost = true
                };
                System.Windows.Forms.ProgressBar pg = new System.Windows.Forms.ProgressBar
                {
                    Maximum = max,
                    Minimum = 0
                };
                progress.Controls.Add(pg);
                progress.Show();
                if (System.Windows.Forms.Application.OpenForms.Count > 0)
                {
                    progress.Top = System.Windows.Forms.Application.OpenForms[0].Top + 5;
                    progress.Left = System.Windows.Forms.Application.OpenForms[0].Left + 5;
                }
                progress.TopMost = false;

                // open request and get response stream

                using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {

                        // loop to read & write to file

                        using (FileStream fs = targetFI.OpenWrite())
                        {
                            try
                            {
                                byte[] buffer = new byte[2048];

                                int read = 0;

                                do
                                {
                                    read = responseStream.Read(buffer, 0, buffer.Length);
                                    fs.Write(buffer, 0, read);
                                    
                                    try
                                    {
                                        pg.Value += read;
                                    }
                                    catch (Exception ex)
                                    {
                                        pg.Value = 0;
                                    }
                                    progress.Refresh();
                                    System.Windows.Forms.Application.DoEvents();
                                }
                                while (read != 0);

                                responseStream.Close();

                                fs.Flush();

                                fs.Close();
                            }
                            catch
                            {

                                // catch error and delete file only partially downloaded

                                fs.Close();

                                // delete target file as it's incomplete

                                targetFI.Delete();

                                throw;
                            }
                        }

                        responseStream.Close();
                    }

                    response.Close();
                }

                progress.Hide();
                progress.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to download file.", ex);
            }
        }

        // Delete remote file
        // filename - filename or full path

        public bool FtpDelete(string filename)
        {

            // Determine if file or full path

            string URI = this.Hostname + GetFullPath(filename);



            System.Net.FtpWebRequest ftp = GetRequest(URI);

            // Set request to delete

            ftp.Method = System.Net.WebRequestMethods.Ftp.DeleteFile;

            try
            {

                // get response but ignore it

                string str = GetStringResponse(ftp);
            }
            catch
            {
                return false;
            }

            return true;
        }



        // Determine if file exists on remote FTP site
        // filename - Filename (for current dir) or full path
        // Note this only works for files

        public bool FtpFileExists(string filename)
        {

            // Try to obtain filesize: if we get error msg containing "550"

            // the file does not exist

            try
            {
                long size = GetFileSize(filename);

                return true;
            }


            catch (Exception ex)
            {

                // only handle expected not-found exception

                if (ex is System.Net.WebException)
                {

                    // file does not exist/no rights error = 550

                    if (ex.Message.Contains("550"))

                        // clear 

                        return false;
                    else
                        throw;
                }
                else
                    throw;
            }
        }



        // Determine size of remote file
        // filename
        // Throws an exception if file does not exist

        public long GetFileSize(string filename)
        {
            string path;

            if (filename.Contains("/"))
                path = AdjustDir(filename);
            else
                path = this.CurrentDirectory + filename;

            string URI = this.Hostname + path;

            System.Net.FtpWebRequest ftp = GetRequest(URI);

            // Try to get info on file/dir?

            ftp.Method = System.Net.WebRequestMethods.Ftp.GetFileSize;
            _ = this.GetStringResponse(ftp);

            return GetSize(ftp);
        }

        public string GetFileLastModified(string filename)
        {
            string path;

            if (filename.Contains("/"))
                path = AdjustDir(filename);
            else
                path = this.CurrentDirectory + filename;

            string URI = this.Hostname + path;

            System.Net.FtpWebRequest ftp = GetRequest(URI);

            // Try to get info on file/dir?

            ftp.Method = WebRequestMethods.Ftp.GetDateTimestamp;
            FtpWebResponse resp = (FtpWebResponse)ftp.GetResponse();

            return resp.LastModified.ToString();
        }


        public bool FtpRename(string sourceFilename, string newName)
        {

            // Does file exist?

            string source = GetFullPath(sourceFilename);

            if (!FtpFileExists(source))
                throw new FileNotFoundException("File " + source + " not found");



            // build target name, ensure it does not exist

            string target = GetFullPath(newName);

            if (target == source)
                throw new ApplicationException("Source and target are the same");
            else if (FtpFileExists(target))
                throw new ApplicationException("Target file " + target + " already exists");



            // perform rename

            string URI = this.Hostname + source;



            System.Net.FtpWebRequest ftp = GetRequest(URI);

            // Set request to delete

            ftp.Method = System.Net.WebRequestMethods.Ftp.Rename;
            ftp.RenameTo = target;

            try
            {

                // get response but ignore it

                string str = GetStringResponse(ftp);
            }
            catch
            {
                return false;
            }

            return true;
        }



        public bool FtpCreateDirectory(string dirpath)
        {

            // perform create

            string URI = this.Hostname + AdjustDir(dirpath);

            System.Net.FtpWebRequest ftp = GetRequest(URI);

            // Set request to MkDir

            ftp.Method = System.Net.WebRequestMethods.Ftp.MakeDirectory;

            try
            {

                // get response but ignore it

                string str = GetStringResponse(ftp);
            }
            catch
            {
                return false;
            }

            return true;
        }



        public bool FtpDeleteDirectory(string dirpath)
        {

            // perform remove

            string URI = this.Hostname + AdjustDir(dirpath);

            System.Net.FtpWebRequest ftp = GetRequest(URI);

            // Set request to RmDir

            ftp.Method = System.Net.WebRequestMethods.Ftp.RemoveDirectory;

            try
            {

                // get response but ignore it

                string str = GetStringResponse(ftp);
            }
            catch
            {
                return false;
            }

            return true;
        }





        // Get the basic FtpWebRequest object with the

        // common settings and security

        private FtpWebRequest GetRequest(string URI)
        {

            // create request

            FtpWebRequest result = (FtpWebRequest)FtpWebRequest.Create(URI);

            // Set the login details

            result.Credentials = GetCredentials();
            result.EnableSsl = _useSSL;

            // Do not keep alive (stateless mode)

            result.KeepAlive = false;

            return result;
        }




        // Get the credentials from username/password

        private System.Net.ICredentials GetCredentials()
        {
            return new System.Net.NetworkCredential(Username, Password);
        }



        // returns a full path using CurrentDirectory for a relative file reference


        private string GetFullPath(string file)
        {
            if (file.Contains("/"))
                return AdjustDir(file);
            else
                return this.CurrentDirectory + file;
        }


        // Amend an FTP path so that it always starts with /
        // path - Path to adjust

        private string AdjustDir(string path)
        {
            return (path.StartsWith("/") ? "" : "/") + path;
        }



        private string GetDirectory(string directory = "")
        {
            string URI;

            if (directory == "")
            {

                // build from current

                URI = Hostname + this.CurrentDirectory;

                _lastDirectory = this.CurrentDirectory;
            }
            else
            {
                if (!directory.StartsWith("/"))
                    throw new ApplicationException("Directory should start with /");

                URI = this.Hostname + directory;

                _lastDirectory = directory;
            }

            return URI;
        }

        // stores last retrieved/set directory
        private string _lastDirectory = "";



        // Obtains a response stream as a string
        // ftp - current FTP request
        // Returns - String containing response
        // FTP servers typically return strings with CR and
        // not CRLF. Use respons.Replace(vbCR, vbCRLF) to convert
        // to an MSDOS string
        private string GetStringResponse(FtpWebRequest ftp)
        {

            // Get the result, streaming to a string
            string result = "";

            using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
            {
                long size = response.ContentLength;

                using (Stream datastream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(datastream))
                    {
                        result = sr.ReadToEnd();

                        sr.Close();
                    }

                    datastream.Close();
                }

                response.Close();
            }

            return result;
        }



        // Gets the size of an FTP request
        private long GetSize(FtpWebRequest ftp)
        {
            long size;
            using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
            {
                size = response.ContentLength;

                response.Close();
            }

            return size;
        }





        private string _hostname;


        public string Hostname
        {
            get
            {
                if (_hostname.StartsWith("ftp://"))
                    return _hostname;
                else
                    return "ftp://" + _hostname;
            }

            set
            {
                _hostname = value;
            }
        }

        private string _username;


        public string Username
        {
            get
            {
                return _username == "" ? "anonymous" : _username;
            }

            set
            {
                _username = value;
            }
        }

        private string _password;

        public string Password
        {
            get
            {
                return _password;
            }

            set
            {
                _password = value;
            }
        }

        private bool _useSSL = false;

        public bool UseSSL
        {
            get
            {
                return _useSSL;
            }
            set
            {
                _useSSL = value;
            }
        }

        private string _currentDirectory = "/";

        public string CurrentDirectory
        {
            get
            {

                // return directory, ensure it ends with /

                return _currentDirectory + (_currentDirectory.EndsWith("/") ? "" : "/");
            }

            set
            {
                if (!value.StartsWith("/"))
                    throw new ApplicationException("Directory should start with /");

                _currentDirectory = value;
            }
        }
    }






    public class FTPfileInfo
    {

        // Stores extended info about FTP file




        public string FullName
        {
            get
            {
                return Path + Filename;
            }
        }

        public string Filename
        {
            get
            {
                return _filename;
            }
        }

        public string Path
        {
            get
            {
                return _path;
            }
        }

        public DirectoryEntryTypes FileType
        {
            get
            {
                return _fileType;
            }
        }

        public long Size
        {
            get
            {
                return _size;
            }
        }

        public DateTime FileDateTime
        {
            get
            {
                return _fileDateTime;
            }
        }

        public string Permission
        {
            get
            {
                return _permission;
            }
        }

        public string Extension
        {
            get
            {
                int i = this.Filename.LastIndexOf(".");

                if (i >= 0 & i < (this.Filename.Length - 1))
                    return this.Filename.Substring(i + 1);
                else
                    return "";
            }
        }

        public string NameOnly
        {
            get
            {
                int i = this.Filename.LastIndexOf(".");

                if (i > 0)
                    return this.Filename.Substring(0, i);
                else
                    return this.Filename;
            }
        }

        private readonly string _filename;

        private readonly string _path;

        private readonly DirectoryEntryTypes _fileType;

        private readonly long _size;

        private readonly DateTime _fileDateTime;

        private readonly string _permission;






        public enum DirectoryEntryTypes
        {
            File,
            Directory
        }



        public FTPfileInfo(string line, string path)
        {

            // parse line

            Match m = GetMatchingRegex(line);

            if (m == null)

                // failed

                throw new ApplicationException("Unable to parse line: " + line);
            else
            {
                _filename = m.Groups["name"].Value;

                _path = path;

                _size = System.Convert.ToInt64(m.Groups["size"].Value);

                _permission = m.Groups["permission"].Value;

                string _dir = m.Groups["dir"].Value;

                if ((_dir != "" & _dir != "-"))
                    _fileType = DirectoryEntryTypes.Directory;
                else
                    _fileType = DirectoryEntryTypes.File;



                try
                {
                    _fileDateTime = DateTime.Parse(m.Groups["timestamp"].Value);
                }
                catch
                {
                    _fileDateTime = default;
                }
            }
        }



        private Match GetMatchingRegex(string line)
        {
            Regex rx;
            Match m;

            for (int i = 0; i <= _ParseFormats.Length - 1; i++)
            {
                rx = new Regex(_ParseFormats[i]);

                m = rx.Match(line);

                if (m.Success)
                    return m;
            }

            return null;
        }
        private static readonly string[] _ParseFormats = new[] { @"(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\w+\s+\w+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{4})\s+(?<name>.+)", @"(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\d+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{4})\s+(?<name>.+)", @"(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\d+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{1,2}:\d{2})\s+(?<name>.+)", @"(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\w+\s+\w+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{1,2}:\d{2})\s+(?<name>.+)", @"(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})(\s+)(?<size>(\d+))(\s+)(?<ctbit>(\w+\s\w+))(\s+)(?<size2>(\d+))\s+(?<timestamp>\w+\s+\d+\s+\d{2}:\d{2})\s+(?<name>.+)", @"(?<timestamp>\d{2}\-\d{2}\-\d{2}\s+\d{2}:\d{2}[Aa|Pp][mM])\s+(?<dir>\<\w+\>){0,1}(?<size>\d+){0,1}\s+(?<name>.+)" };
    }





    /// <summary>
    /// 
    /// </summary>
    public class FTPdirectory : List<FTPfileInfo>
    {
        public FTPdirectory()
        {
        }


        public FTPdirectory(string dir, string path)
        {
            foreach (string line in dir.Replace(((char)10).ToString(), "").Split((char)13))
            {

                // parse

                if (line != "")
                    this.Add(new FTPfileInfo(line, path));
            }
        }



        public FTPdirectory GetFiles(string ext = "")
        {
            return this.GetFileOrDir(FTPfileInfo.DirectoryEntryTypes.File, ext);
        }




        public FTPdirectory GetDirectories()
        {
            return this.GetFileOrDir(FTPfileInfo.DirectoryEntryTypes.Directory);
        }



        // internal: share use function for GetDirectories/Files

        private FTPdirectory GetFileOrDir(FTPfileInfo.DirectoryEntryTypes type, string ext = "")
        {
            FTPdirectory result = new FTPdirectory();

            foreach (FTPfileInfo fi in this)
            {
                if (fi.FileType == type)
                {
                    if (ext == "")
                        result.Add(fi);
                    else if (ext == fi.Extension)
                        result.Add(fi);
                }
            }

            return result;
        }



        public bool FileExists(string filename)
        {
            foreach (FTPfileInfo ftpfile in this)
            {
                if (ftpfile.Filename == filename)
                    return true;
            }

            return false;
        }

        private const char slash = '/';

        public static string GetParentDirectory(string dir)
        {
            string tmp = dir.TrimEnd(slash);

            int i = tmp.LastIndexOf(slash);

            if (i > 0)
                return tmp.Substring(0, i - 1);
            else
                throw new ApplicationException("No parent for root");
        }
    }
}
