using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoH_Modder
{
    public class FTP_Client
    {
        public string ServerName { get; set; }
        public int PortNumber { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public FTP_Client(string serverName, string userName, string password) : this(serverName, 21, userName, password) { }
        public FTP_Client(string serverName, int portNumber, string userName, string password)
        {
            ServerName = serverName.EndsWith("/") ? serverName : (serverName + "/");
            PortNumber = portNumber;
            UserName = userName;
            Password = password;
        }

        /// <summary>
        /// Gets the directory listing for the specified directory
        /// </summary>
        /// <param name="DirectoryName"></param>
        /// <returns></returns>
        public List<string> GetDirectoryListing(string DirectoryName)
        {
            List<string> retVal = new List<string>();

            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(ServerName + (DirectoryName.EndsWith("/") ? DirectoryName : (DirectoryName + "/")));
            ftpRequest.Credentials = new NetworkCredential(UserName, Password);
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            ftpRequest.KeepAlive = true;
            ftpRequest.UseBinary = true;
            using (FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse())
            {
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    string line = streamReader.ReadLine();
                    while (!string.IsNullOrEmpty(line))
                    {
                        retVal.Add(line);
                        line = streamReader.ReadLine();
                    }
                }
            }
            return retVal;
        }

        public bool FileExists(string remoteFileName)
        {
            List<string> files = GetDirectoryListing(Path.GetDirectoryName(remoteFileName));
            foreach (string file in files)
            {
                if (file == remoteFileName) return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the named remote file and places it at the named location. Must use the full relative path from the FTP root directory to locate file
        /// </summary>
        /// <param name="remoteFileName"></param>
        /// <param name="localFileName"></param>
        /// <returns></returns>
        public bool GetFile(ToolStripProgressBar ProgressBar, string remoteFileName, string localFileName)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ServerName + remoteFileName);
                request.Credentials = new NetworkCredential(UserName, Password);
                request.Method = WebRequestMethods.Ftp.GetFileSize;
                FtpWebResponse sizeResponse = (FtpWebResponse)request.GetResponse();
                long size = sizeResponse.ContentLength;
                sizeResponse.Close();
                ProgressBar.Maximum = (int)size;
                ProgressBar.Value = 0;

                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(ServerName + remoteFileName);
                ftpRequest.Credentials = new NetworkCredential(UserName, Password);
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                ftpRequest.UseBinary = true;
                ftpRequest.KeepAlive = true;
                using (FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse())
                {
                    using (Stream streamReader = response.GetResponseStream())
                    {
                        using (FileStream writer = new FileStream(localFileName, FileMode.Create))
                        {
                            long length = response.ContentLength;
                            int bufferSize = 2048;
                            ProgressBar.Value = 0;
                            int readCount;
                            byte[] buffer = new byte[2048];

                            readCount = streamReader.Read(buffer, 0, bufferSize);
                            while (readCount > 0)
                            {
                                ProgressBar.Increment(readCount < bufferSize ? readCount : bufferSize);
                                Application.DoEvents();
                                writer.Write(buffer, 0, readCount);
                                readCount = streamReader.Read(buffer, 0, bufferSize);
                            }
                            writer.Flush();
                        }
                    }
                }

                ProgressBar.Value = 0;
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get remote file '" + remoteFileName + "' and place it at location '" + localFileName + "'", ex);
            }
        }

        internal void DeleteFile(string fileName)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ServerName + fileName);
                request.Credentials = new NetworkCredential(UserName, Password);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Console.WriteLine("Delete status: {0}", response.StatusDescription);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to delete file '" + fileName + "' from the server.", ex);
            }
        }

        /// <summary>
        /// Uploads a file from the local directory to the remote FTP site at the full path specified
        /// </summary>
        /// <param name="sourceFileName">Full local path to the file to upload</param>
        /// <param name="destinationFileName">Final path of the file, including filename</param>
        public bool UploadFile(string sourceFileName, string destinationFileName, ToolStripProgressBar progBar)
        {
            try
            {
                destinationFileName = "ftp://ftp.cityofplayers.com/" + destinationFileName;
                FileInfo fileInf = new FileInfo(sourceFileName);
                FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(destinationFileName));
                reqFTP.Credentials = new NetworkCredential(UserName, Password);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
                reqFTP.UseBinary = true;
                reqFTP.ContentLength = fileInf.Length;
                progBar.Maximum = int.Parse(fileInf.Length.ToString());
                int buffLength = 2048;
                byte[] buff = new byte[buffLength];
                int contentLen;
                FileStream fs = fileInf.OpenRead();
                Stream strm = reqFTP.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                progBar.Value = 0;

                while (contentLen != 0)
                {
                    progBar.Increment(contentLen < buffLength ? contentLen : buffLength);
                    Application.DoEvents();
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }

                // Close the file stream and the Request Stream
                strm.Close();
                fs.Close();
                progBar.Value = 0;

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to upload local file '" + sourceFileName + "' to path '" + destinationFileName + "' on the remote FTP server.", ex);
            }
        }
    }
}
