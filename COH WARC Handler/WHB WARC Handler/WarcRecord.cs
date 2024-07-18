using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Windows.Forms;

namespace WHBennett
{
    /// <summary>
    /// Represents an extraction record from taking an input WARC filename, and extracting the contents
    /// </summary>
    public class WarcExtractionRecord
    {
        public string SourceFileName { get; set; }
        public string OutputDirectory { get; set; }
        /// <summary>
        /// The list of WARCRecords and FileNames that came out of the file
        /// </summary>
        public Dictionary<WarcRecord, string> ExtractedFiles = new Dictionary<WarcRecord, string>();

        public WarcExtractionRecord(string sourceFileName, string outputDirectory, ref ToolStripStatusLabel tssl)
        {
            SourceFileName = sourceFileName;
            OutputDirectory = outputDirectory;

            StreamWARCFile(sourceFileName, outputDirectory, ref tssl);
        }

        private void StreamWARCFile(string sourceFileName, string outputDirectory, ref ToolStripStatusLabel tssl)
        {
            try
            {
                Encoding standardEncoding = Encoding.GetEncoding("iso-8859-1");
                //Read the file as a running stream, splitting it into smaller WARC files
                int fileNum = 0;
                List<string> WarcFilesCreated = new List<string>();
                WarcRecord wr = null;
                using StreamReader sr = new StreamReader(sourceFileName, standardEncoding);
                int byteLocation = 0;
                List<byte> bytesList = null;

                int numFiles = 0;

                double totNumBytes = double.Parse(new FileInfo(sourceFileName).Length.ToString());
                string previousLine = "";
                string line = "";
                while (true)
                {
                    int ContentLength = 0;

                    previousLine = line;
                    line = sr.ReadLine();
                    if (line == null) break; //all done with the file!
                    byteLocation += standardEncoding.GetByteCount(line + Environment.NewLine);
                    double curPercent = Math.Round(double.Parse((byteLocation / totNumBytes * 100).ToString()), 2);
                    tssl.Text = "On Byte " + byteLocation + " of " + totNumBytes + ", " + (curPercent == 0 ? "" : (curPercent + "% complete."));
                    Application.DoEvents();

                    //Read until we get to the content length. That's the last line before the newline, then we read the lines until we
                    //Reach the correct number of bytes
                    if (line.StartsWith("WARC/"))
                    {
                        //Just increment the file counter and add back to the bytes
                        fileNum++;
                        bytesList = new List<byte>();
                        wr = new WarcRecord();
                        byteLocation = ProcessLine(bytesList, line, byteLocation);
                        WarcRecord.SetHeaderValue(wr, line);
                    }
                    else if (line.ToLower().StartsWith("content-length"))
                    {
                        WarcRecord.SetHeaderValue(wr, line);
                        //Grab the content length, read the next line to skip it, and then start the "byte grabbing" loop
                        ContentLength = int.Parse(line.Split(':')[1].ToString().Trim());
                        byteLocation = ProcessLine(bytesList, line, byteLocation);
                        line = sr.ReadLine();
                        byteLocation = ProcessLine(bytesList, line, byteLocation); //empty line after the header

                        //Check if it's got an HTTP header
                        line = sr.ReadLine();

                        List<byte> contentBytesList = new List<byte>();
                        if (line.StartsWith("HTTP/"))
                        {
                            //If it does, process the header to determine if the rest of the record is defined by content length, or chunk
                            bool chunked = false;
                            bool length = false;
                            int contentLength = 0;
                            do
                            {
                                line = sr.ReadLine();
                                if (line == "Transfer-Encoding: chunked") chunked = true;
                                else if (line.StartsWith("Content-Length"))
                                {
                                    length = true;
                                    contentLength = int.Parse(line.Split(':')[1].ToString().Trim()); //get the updated ContentLength
                                                                                                     //for processing down below
                                }
                                else
                                {
                                    byteLocation = ProcessLine(bytesList, line, byteLocation);
                                }
                            } while (chunked == false && length == false);

                            if (chunked)
                            {
                                //Special handling for chunked data
                                WarcRecord.SetHeaderValue(wr, line);

                                //Skip the next line as it should be blank
                                line = sr.ReadLine();
                                byteLocation += standardEncoding.GetByteCount(line + Environment.NewLine);

                                List<byte> wholeChunk = new List<byte>();
                                int chunkSize = 0;
                                bool validChunkSize = false;
                                do
                                {
                                    line = sr.ReadLine();
                                    byteLocation += standardEncoding.GetByteCount(line + Environment.NewLine);
                                    try
                                    {
                                        chunkSize = int.Parse(line, NumberStyles.HexNumber);
                                        validChunkSize = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine(ex.ToString()); //don't care, just ignore the line
                                    }
                                }
                                while (!validChunkSize);

                                do
                                {
                                    List<byte> tempBytes = new List<byte>();

                                    do
                                    {
                                        //Read by char, rather than by lines now, since lines can be EITHER CRLF or just LF
                                        //and sr.ReadLine() doesn't distinguish between these, so if we use that method,
                                        //we lose or add bytes that weren't there before!
                                        char character = (char)sr.Read();
                                        byte bytes = new byte();
                                        bytes = Convert.ToByte(character);
                                        byteLocation += 1;

                                        tempBytes.Add(bytes);

                                    } while (tempBytes.Count < chunkSize);

                                    foreach (byte b in tempBytes)
                                    {
                                        wholeChunk.Add(b);
                                    }

                                    //look for the next chunkSize, since we're done with this chunk
                                    chunkSize = 0;
                                    validChunkSize = false;
                                    string previousChunk = standardEncoding.GetString(tempBytes.ToArray());
                                    previousLine = previousChunk;
                                    do
                                    {
                                        if (byteLocation >= totNumBytes)
                                        {
                                            chunkSize = 0;
                                            break; //Break if we're at the end of the file
                                        }

                                        line = sr.ReadLine();
                                        byteLocation += standardEncoding.GetByteCount(line + Environment.NewLine);
                                        curPercent = Math.Round(double.Parse((byteLocation / totNumBytes * 100).ToString()), 2);
                                        tssl.Text = "On Byte " + byteLocation + " of " + totNumBytes + ", " + (curPercent == 0 ? "" : (curPercent + "% complete."));
                                        Application.DoEvents();


                                        if (line == null) continue;
                                        try
                                        {
                                            chunkSize = int.Parse(line, NumberStyles.HexNumber);
                                            validChunkSize = true;
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine("");
                                        }
                                        previousLine = line;
                                    } while (!validChunkSize);

                                } while (chunkSize != 0);

                                //Extract the actual file to its final destination and add it to the extracted file list
                                string fileName = GetSanitizedFileName(wr.WARC_Target_URI == null ? wr.WARC_Record_ID.AbsoluteUri :
                                    wr.WARC_Target_URI.AbsoluteUri, outputDirectory);
                                File.WriteAllBytes(fileName, wholeChunk.ToArray());
                                if (!ExtractedFiles.ContainsKey(wr)) ExtractedFiles.Add(wr, fileName);
                                numFiles++;
                                tssl.Text = "Still working as of " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss") + ", found " + numFiles + " file(s) " +
                                    "inside of WARC file so far.";
                                Application.DoEvents();
                                continue;

                            }
                            else
                            {
                                //Get through the rest of the HTTP header first before reading by byte
                                do
                                {
                                    line = sr.ReadLine();
                                } while (line != "");

                                List<byte> tempBytes = new List<byte>();

                                do
                                {
                                    //read by raw int value converted to a byte
                                    int value = sr.Read();
                                    char byteChar = (char)value;
                                    byte b = Convert.ToByte(byteChar);
                                    tempBytes.Add(b);

                                    //curPercent = Math.Round(double.Parse((byteLocation / totNumBytes * 100).ToString()), 2);

                                    //tssl.Text = "Still working as of " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss") + ", adding byte # " + tempBytes.ToArray().Length +
                                    //" up to a max of " + contentLength + " for the current large file processing. " +
                                    //(curPercent == 0 ? "" : (curPercent + "% complete."));
                                    //Application.DoEvents();
                                }
                                while (tempBytes.ToArray().Length < contentLength);
                                //Extract the actual file to its final destination and add it to the extracted file list
                                string fileName = GetSanitizedFileName(wr.WARC_Target_URI == null ? wr.WARC_Record_ID.AbsoluteUri :
                                    wr.WARC_Target_URI.AbsoluteUri, outputDirectory);
                                File.WriteAllBytes(fileName, tempBytes.ToArray());
                                if (!ExtractedFiles.ContainsKey(wr)) ExtractedFiles.Add(wr, fileName);
                                numFiles++;
                                tssl.Text = "Still working as of " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss") + ", found " + numFiles + " file(s) " +
                                    "inside of WARC file so far.";
                                Application.DoEvents();
                                continue;
                            }
                        }
                        else
                        {
                            List<byte> tempBytes = new List<byte>();
                            byte[] oldLineBytes = standardEncoding.GetBytes(line + Environment.NewLine);
                            foreach (byte b in oldLineBytes)
                            {
                                tempBytes.Add(b);
                            }

                            do
                            {
                                //Read by char, rather than by lines now, since lines can be EITHER CRLF or just LF
                                //and sr.ReadLine() doesn't distinguish between these, so if we use that method,
                                //we lose or add bytes that weren't there before!
                                char character = (char)sr.Read();
                                byte bytes = new byte();
                                bytes = Convert.ToByte(character);
                                byteLocation += 1;

                                tempBytes.Add(bytes);
                            }
                            while (tempBytes.ToArray().Length < ContentLength);
                            //Extract the actual file to its final destination and add it to the extracted file list
                            string fileName = GetSanitizedFileName(wr.WARC_Target_URI == null ? wr.WARC_Record_ID.AbsoluteUri :
                                wr.WARC_Target_URI.AbsoluteUri, outputDirectory);
                            File.WriteAllBytes(fileName, tempBytes.ToArray());
                            if (!ExtractedFiles.ContainsKey(wr)) ExtractedFiles.Add(wr, fileName);
                            numFiles++;
                            tssl.Text = "Still working as of " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss") + ", found " + numFiles + " file(s) " +
                                "inside of WARC file so far.";
                            Application.DoEvents();
                            continue;
                        }
                    }
                    else if (line.Contains(":"))
                    {
                        byteLocation = ProcessLine(bytesList, line, byteLocation); //Add back to the bytes for later processing
                        WarcRecord.SetHeaderValue(wr, line);
                    }


                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to handle large WARC file", ex);
            }
        }

        /// <summary>
        /// Adds all of the bytes from the line into the passed in bytesList
        /// </summary>
        /// <param name="bytesList"></param>
        /// <param name="line"></param>
        private int ProcessLine(List<byte> bytesList, string line, int startingPosition)
        {
            try
            {
                //Have to add the newline back in because it was
                //stripped out by the ReadLine method, to get the correct number of bytes
                byte[] tempBytes = Encoding.UTF8.GetBytes(line + "\r\n");
                foreach (byte b in tempBytes)
                {
                    bytesList.Add(b);
                }
                return startingPosition += tempBytes.Length;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return startingPosition;
            }
        }

        public static Byte[] ReadMMFAllBytes(string fileName)
        {
            try
            {
                using MemoryMappedFile mmf = MemoryMappedFile.OpenExisting(fileName);
                using MemoryMappedViewStream stream = mmf.CreateViewStream();
                using BinaryReader binReader = new BinaryReader(stream);
                return binReader.ReadBytes((int)stream.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        ///// <summary>
        ///// Extracts all WARC Records within the file and outputs the files
        ///// </summary>
        ///// <param name="fileName"></param>
        ///// <param name="outputDir"></param>
        //private void GetWARCRecordsByByte(string fileName, string outputDir)
        //{
        //    try
        //    {
        //        byte[] bytes = File.ReadAllBytes(fileName); //Read all of the bytes into memory at once

        //        //Iterate through them, reading them one line at a time. Doing it this way allows us to read the lines and bytes separately when we want to
        //        bool inWARCHeader = true;
        //        WarcRecord wr = new WarcRecord();

        //        int max;
        //        try
        //        {
        //            max = int.Parse(bytes.Length.ToString());
        //        }
        //        catch (Exception ex)
        //        {
        //            max = 2147483647;
        //            Debug.WriteLine(ex.ToString());
        //        }

        //        bool showProgress = false;
        //        System.Windows.Forms.Form progress = null;
        //        System.Windows.Forms.ProgressBar pg = null;
        //        if (showProgress)
        //        {
        //            //Create a temporary windows form to show the progress
        //            progress = new System.Windows.Forms.Form
        //            {
        //                AutoSize = true,
        //                AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink,
        //                TopMost = true
        //            };
        //            pg = new System.Windows.Forms.ProgressBar
        //            {
        //                Maximum = max,
        //                Minimum = 0
        //            };
        //            progress.Controls.Add(pg);
        //            progress.Show();
        //            if (System.Windows.Forms.Application.OpenForms.Count > 0)
        //            {
        //                progress.Top = System.Windows.Forms.Application.OpenForms[0].Top + 5;
        //                progress.Left = System.Windows.Forms.Application.OpenForms[0].Left + 5;
        //            }
        //            progress.TopMost = false;
        //        }


        //        for (int i = 0; i < bytes.Length - 1; i++)
        //        {
        //            int currentPos = i; //Store the current position for later use
        //            int originalPos = i; //Store the original position for later use

        //            try
        //            {
        //                if (showProgress) pg.Value = i;
        //            }
        //            catch (Exception ex)
        //            {
        //                if (showProgress) pg.Value = 0;
        //                Debug.WriteLine(ex.ToString());
        //            }

        //            if (showProgress) progress.Refresh();
        //            System.Windows.Forms.Application.DoEvents();

        //            string line = ReadLine(i, bytes, out currentPos); //Get the line itself
        //            i = currentPos; //Now that we've read it, reset the current position to advance

        //            if (line.Trim() == "")
        //            {
        //                if (inWARCHeader)
        //                {
        //                    //Grab the chunk of all of the rest of the data for this record, then process the HTTP Header out of it
        //                    inWARCHeader = false;
        //                    //Write the file data, but only if the file doesn't already exist
        //                    string fn = GetSanitizedFileName(wr.WARC_Target_URI == null ? wr.WARC_Record_ID.AbsoluteUri :
        //                        wr.WARC_Target_URI.AbsoluteUri, outputDir);

        //                    GetChunkDataAndProcess(bytes, currentPos + 1, wr, outputDir, true); //+1 to get past the \n in \r\n, since we're at the \r right now
        //                    i = currentPos + 1 + wr.Content_Length;
        //                    ExtractedFiles.Add(wr, fn);
        //                    continue;
        //                }
        //                else
        //                {
        //                    //Just ignore, as all response empty lines should be covered in the byte block
        //                    continue;
        //                }
        //            }
        //            //Check for two new lines in a row, followed by the WARC version number. If we find it, then we've found a new record. Store the original and start a new one
        //            else if (ReadLine(originalPos, bytes, out int testEndingPoint2).StartsWith("WARC/") && (originalPos == 0 ||
        //                (IsCRLF(bytes, originalPos - 2) && IsCRLF(bytes, originalPos - 4))))
        //            {
        //                //returnValue.Add(wr);
        //                wr = new WarcRecord();
        //                inWARCHeader = true;
        //            }
        //            else if (inWARCHeader)
        //            {
        //                //If we're in the header, and we're at the start of a line, then record that header to the log file
        //                if (originalPos == 0 || IsCRLF(bytes, originalPos - 2))
        //                {
        //                    WarcRecord.SetHeaderValue(wr, line);
        //                    continue;
        //                }
        //                else
        //                {
        //                    //If we're in the header, yet we're not at the start of a line, then don't do anything but keep moving along
        //                    continue;
        //                }
        //            }
        //            else
        //            {
        //                //WARC header and HTTP Header
        //                if ((line.StartsWith("WARC") && (line.Contains(":"))) || (line.Contains(":") && IsCRLF(bytes, i) && IsCRLF(bytes, i + 2)))
        //                {
        //                    WarcRecord.SetHeaderValue(wr, line);
        //                    continue;
        //                }
        //                else
        //                {
        //                    string fName = GetSanitizedFileName(wr.WARC_Target_URI == null ? wr.WARC_Record_ID.AbsoluteUri :
        //                        wr.WARC_Target_URI.AbsoluteUri, outputDir);
        //                    if (File.Exists(fName)) File.Delete(fName);

        //                    using (StreamWriter sw = new StreamWriter(fName))
        //                    {
        //                        //Must be content, just pull the entire contents by the number of bytes, write it, then increment the loop variable by that number of bytes
        //                        for (int j = originalPos; j < wr.Content_Length; j++)
        //                        {
        //                            sw.Write(bytes[j]);
        //                        }
        //                    }
        //                    i += wr.Content_Length;
        //                    try
        //                    {
        //                        if (!ExtractedFiles.ContainsKey(wr))
        //                        {
        //                            ExtractedFiles.Add(wr, fName);
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Debug.WriteLine(ex.ToString());
        //                    }
        //                }
        //            }
        //        }

        //        if (showProgress)
        //        {
        //            progress.Hide();
        //            progress.Dispose();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.ToString());
        //    }
        //}

        /// <summary>
        /// Reads exactly one line from the byte array, starting from the passed in start point
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="bytes"></param>
        /// <param name="endingPoint"></param>
        /// <returns></returns>
        private static string ReadLine(int startPoint, byte[] bytes, out int endingPoint)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                endingPoint = startPoint;
                for (int j = startPoint; j <= bytes.Length - 1; j++)
                {
                    if (IsCRLF(bytes, j))
                    {
                        endingPoint = j + 1; //Report back the ending point. If this is a CRLF, that will be after the \n in \r\n
                        break; //Break when we've reached a CRLF
                    }
                    sb.Append(Convert.ToChar(bytes[j]));
                }

                string retVal = sb.ToString();
                return retVal;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                endingPoint = startPoint;
                return null;
            }
        }


        /// <summary>
        /// Throw the second character into this method, to look back and see if it's a CLRF combination
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="i">Position of character # 1</param>
        /// <returns></returns>
        private static bool IsCRLF(byte[] bytes, int i)
        {
            try
            {
                if (i < 0) i = 0; //Prevent errors from the start of the document
                if (Convert.ToChar(bytes[i]) == '\r' && Convert.ToChar(bytes[i + 1]) == '\n')
                {
                    return true;
                }
                else return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
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

                output.Replace("/", @"\");
                output = outputDir + (outputDir.EndsWith(@"\") ? "" : @"\") + Path.GetFileName(output);

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

                //Prevent duplicates, and mark the file as a renamed file
                if (File.Exists(output))
                {
                    //Check if the existing file begins with a "GET" statement. If it does, then we don't want it, delete it and replace it with this one
                    if (File.ReadAllText(output).StartsWith("GET "))
                    {
                        File.Delete(output);
                        return output;
                    }

                    //If the file already contains a "_renamed_" section, chop it off and check from there
                    if (output.Contains("_renamed_"))
                    {
                        output = output.Split(new string[] { "_renamed_" }, StringSplitOptions.None)[0] + Path.GetExtension(output);
                    }

                    //Otherwise, it's some sort of duplicate file, so rename this one
                    bool fileExists = true;
                    int iteration = 1;
                    do
                    {
                        iteration++;

                        //If the file already contains a "_renamed_" section, chop it off and check from there
                        if (output.Contains("_renamed_"))
                        {
                            output = output.Split(new string[] { "_renamed_" }, StringSplitOptions.None)[0] + Path.GetExtension(output);
                        }

                        output = Path.GetDirectoryName(output) + @"\" + Path.GetFileNameWithoutExtension(output) + "_renamed_" + iteration + Path.GetExtension(output);

                        // Make sure that adding _renamed_ to the filename doesn't make it too long
                        if (output.Length > 250)
                        {

                            string toReduce = Path.GetDirectoryName(output) + Path.GetFileNameWithoutExtension(output);
                            int lengthWithoutExtension = toReduce.Length - 1;
                            do
                            {
                                toReduce = Path.GetDirectoryName(output) + Path.GetFileNameWithoutExtension(output);
                                output = toReduce.Substring(0, lengthWithoutExtension) +
                                    Path.GetExtension(output);
                                lengthWithoutExtension = (Path.GetDirectoryName(output) + Path.GetFileNameWithoutExtension(output)).Length - 1;
                            } while (output.Length > 250);
                        }

                        fileExists = File.Exists(output);
                    } while (fileExists);
                }

                return output;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get filename from string '" + input + "'.", ex);
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
        /// Extracts the requested contents from the provided bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="currentPos"></param>
        /// <param name="wr"></param>
        /// <param name="outputDir"></param>
        /// <param name="WriteDataToFile"></param>
        private static void GetChunkDataAndProcess(byte[] bytes, int currentPos, WarcRecord wr, string outputDir, bool WriteDataToFile)
        {
            try
            {
                //byte[] chunk = new byte[wr.Content_Length]; //regular files
                byte[] chunk = new byte[bytes.Length - currentPos]; //split files
                Array.Copy(bytes, currentPos, chunk, 0, chunk.Length - 1); //split files
                //Array.Copy(bytes, currentPos, chunk, 0, wr.Content_Length); //Normal file

                //Now, check for an HTTP header, and if there is one, strip it away and log it
                string firstLine = ReadLine(0, chunk, out int endingPoint);
                if (firstLine.StartsWith("HTTP/")) chunk = StripAndReportHTTPHeader(wr, chunk);

                if (WriteDataToFile)
                {
                    string fName = GetSanitizedFileName(wr.WARC_Target_URI == null ? wr.WARC_Record_ID.AbsoluteUri :
                        wr.WARC_Target_URI.AbsoluteUri, outputDir);
                    if (File.Exists(fName))
                    {
                        Debug.WriteLine("");
                        return; //Don't overwrite the existing file
                    }
                    using (var fs = new FileStream(fName, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(chunk, 0, chunk.Length);
                    }

                    string indexDoc = outputDir + "index.html";
                    //Create the index page if it doesn't already exist
                    if (!File.Exists(indexDoc))
                    {
                        //Add the file link to the main URL document
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.DocumentNode.AppendChild(HtmlNode.CreateNode("<html><head></head><body></body></html>"));
                        doc.Save(indexDoc);
                    }

                    fName = fName.Replace(outputDir, "");
                    //Add a relative link to the file into the index document
                    if (!File.ReadAllText(indexDoc).Contains(fName))
                    {
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.Load(indexDoc);
                        HtmlNode body = doc.DocumentNode.SelectSingleNode("//body");
                        body.AppendChild(HtmlNode.CreateNode(@"<br/>"));
                        body.AppendChild(HtmlNode.CreateNode(@"<a href=""" + fName + @""">" + fName + "</a>"));
                        doc.Save(indexDoc);
                    }

                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Strips out the HTTP header, that we don't really care about because we only want the actual file itself
        /// </summary>
        /// <param name="wr"></param>
        /// <param name="chunk"></param>
        /// <param name="outputDir"></param>
        /// <returns></returns>
        private static byte[] StripAndReportHTTPHeader(WarcRecord wr, byte[] chunk)
        {
            try
            {
                byte[] newChunk = null;

                //Measure the chunk until we find a CRLF followed by another CRLF, then remove all bytes up to and including the second CRLF
                List<byte> temp = new List<byte>();
                List<byte> tempHeader = new List<byte>();
                bool foundEndofHeader = false;
                for (int i = 0; i < chunk.Length - 1; i++)
                {
                    if (!foundEndofHeader)
                    {
                        if (IsCRLF(chunk, i) && IsCRLF(chunk, i - 2))
                        {
                            foundEndofHeader = true;
                            i++; //Get past the last \n
                        }
                        else
                        {
                            tempHeader.Add(chunk[i]);
                            continue;
                        }
                    }
                    else
                    {
                        //Add everything else after the header
                        temp.Add(chunk[i]);
                    }
                }

                byte[] headerChunk = tempHeader.ToArray();
                newChunk = temp.ToArray();

                string entireFile = Encoding.UTF8.GetString(chunk);
                //Get the charset for later encoding purposes
                string test = Encoding.UTF8.GetString(headerChunk);
                string encoding = "";
                Encoding e = null;
                if (test.Contains("charset="))
                {
                    encoding = test.Split(new string[] { "charset=" }, StringSplitOptions.None)[1].Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0];
                    try
                    {
                        e = Encoding.GetEncoding(encoding);
                    }
                    catch
                    {
                        //Default to UTF8 encoding
                        e = Encoding.UTF8;
                    }
                }

                //If it's going to be an HTTP page, and the remaining amount contains a two character "header checksum" at the beginning, strip it out
                if (wr.WARC_Target_URI.AbsoluteUri.Contains(".php") || wr.WARC_Target_URI.AbsoluteUri.Contains(".htm"))
                {
                    string remaining = "";

                    if (e == null)
                    {
                        remaining = Encoding.UTF8.GetString(newChunk);
                        Debug.WriteLine(""); //Should never happen
                    }
                    else
                    {
                        remaining = Encoding.GetEncoding(encoding).GetString(newChunk);
                    }

                    if (remaining.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0].Length == 2)
                    {
                        remaining = remaining.Substring(4);

                        if (e == null)
                        {
                            Debug.WriteLine("");
                        }
                        else
                        {
                            newChunk = Encoding.GetEncoding(encoding).GetBytes(remaining);
                        }
                    }
                }

                return newChunk;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        //private static byte[] StripHTTPHeader(byte[] chunk)
        //{
        //    try
        //    {
        //        byte[] newChunk = null;

        //        //Measure the chunk until we find a CRLF followed by another CRLF, then remove all bytes up to and including the second CRLF
        //        List<byte> temp = new List<byte>();
        //        List<byte> tempHeader = new List<byte>();
        //        bool foundEndofHeader = false;
        //        for (int i = 0; i < chunk.Length - 1; i++)
        //        {
        //            if (!foundEndofHeader)
        //            {
        //                if (IsCRLF(chunk, i) && IsCRLF(chunk, i - 2))
        //                {
        //                    foundEndofHeader = true;
        //                    i++; //Get past the last \n
        //                }
        //                else
        //                {
        //                    tempHeader.Add(chunk[i]);
        //                    continue;
        //                }
        //            }
        //            else
        //            {
        //                //Add everything else after the header
        //                temp.Add(chunk[i]);
        //            }
        //        }

        //        byte[] headerChunk = tempHeader.ToArray();
        //        newChunk = temp.ToArray();

        //        string entireFile = Encoding.UTF8.GetString(chunk);
        //        //Get the charset for later encoding purposes
        //        string test = Encoding.UTF8.GetString(headerChunk);
        //        string encoding = "";
        //        Encoding e = null;
        //        if (test.Contains("charset="))
        //        {
        //            encoding = test.Split(new string[] { "charset=" }, StringSplitOptions.None)[1].Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0];
        //            try
        //            {
        //                e = Encoding.GetEncoding(encoding);
        //            }
        //            catch
        //            {
        //                //Default to UTF8 encoding
        //                e = Encoding.UTF8;
        //            }
        //        }


        //        string remaining = "";

        //        if (e == null)
        //        {
        //            //This is a RAW file, not a text file, so do NOT convert it to string, just get the rest of the bytes
        //            return newChunk;
        //        }
        //        else
        //        {
        //            remaining = Encoding.GetEncoding(encoding).GetString(newChunk);
        //        }

        //        if (remaining.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0].Length == 2)
        //        {
        //            remaining = remaining.Substring(4);

        //            if (e == null)
        //            {
        //                Debug.WriteLine("");
        //            }
        //            else
        //            {
        //                newChunk = Encoding.GetEncoding(encoding).GetBytes(remaining);
        //            }
        //        }

        //        return newChunk;
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex);
        //        return null;
        //    }
        //}
    }

    /// <summary>
    /// A class to contain a single WARC format record. Please refer to:
    /// https://iipc.github.io/warc-specifications/specifications/warc-format/warc-1.0/#warc-record-id-mandatory
    /// for details about the ISO standards for this object. The standards from that page have been pasted here for convenience.
    /// </summary>
    public class WarcRecord
    {

        public WarcRecord() { }

        /// <summary>
        /// Minimal constructor to create a "valid" WARC object. You pass in the entire line and the method does the work!
        /// </summary>
        /// <param name="RecordID"></param>
        /// <param name="ContentLength"></param>
        /// <param name="WARCDate"></param>
        /// <param name="WarcType"></param>
        /// <param name="PassedInWholeLines">Whether or not the developer has passed in the whole, unmodified line. If they have,
        /// the method will parse if for them. If they have not, the method will prepend the field name for them.</param>
        /// <example>Pass in "</example>
        public WarcRecord(string RecordID, string ContentLength, string WARCDate, string WarcType, bool PassedInWholeLines = false)
        {
            if (PassedInWholeLines)
            {
                new WarcRecord(RecordID, ContentLength, WARCDate, WarcType);
            }
            else
            {
                new WarcRecord(
                    "WARC-RECORD-ID:" + RecordID,
                    "CONTENT-LENGTH:" + ContentLength,
                    "WARC-DATE:" + WARCDate,
                    "WARC-TYPE:" + WarcType);
            }
        }

        /// <summary>
        /// Protected method that sets the header value based on the entire line
        /// </summary>
        /// <param name="RecordIDLine"></param>
        /// <param name="ContentLengthLine"></param>
        /// <param name="WARCDateLine"></param>
        /// <param name="WarcTypeLine"></param>
        protected WarcRecord(string RecordIDLine, string ContentLengthLine, string WARCDateLine, string WarcTypeLine)
        {
            if (!SetHeaderValue(this, RecordIDLine)) throw new Exception(
                "Failed to create WarcRecord, invalid RecordIDLine of '" + RecordIDLine + "'");
            if (!SetHeaderValue(this, ContentLengthLine)) throw new Exception(
                "Failed to create WarcRecord, invalid ContentLengthLine of '" + ContentLengthLine + "'");
            if (!SetHeaderValue(this, WARCDateLine)) throw new Exception(
                "Failed to create WarcRecord, invalid WARCDateLine of '" + WARCDateLine + "'");
            if (!SetHeaderValue(this, WarcTypeLine)) throw new Exception(
                "Failed to create WarcRecord, invalid WarcTypeLine of '" + WarcTypeLine + "'");
        }

        ///// <summary>
        ///// Takes an input FileName, and returns a HashSet of WarcRecord objects, fully populated
        ///// </summary>
        ///// <param name="FileName"></param>
        ///// <returns></returns>
        //public static HashSet<WarcRecord> GetWarcRecordsFromFile(string FileName, string outputDir)
        //{
        //    try
        //    {
        //        //HashSet<WarcRecord> returnValue = GetWARCRecordsByLine(FileName, outputDir);

        //        HashSet<WarcRecord> returnValue = GetWARCRecordsByByte(FileName, outputDir);

        //        return returnValue;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Failed to obtain a dictionary of WarcRecords from FileName '" + FileName + "'", ex);
        //    }

        //}

        //private static HashSet<WarcRecord> GetWARCRecordsByLine(string FileName, string outputDir)
        //{
        //    try
        //    {
        //        HashSet<WarcRecord> returnValue = new HashSet<WarcRecord>();
        //        string[] lines = File.ReadAllLines(FileName);

        //        int lineNum = 0;
        //        int fileNum = 0;
        //        while (lineNum != -1)
        //        {
        //            fileNum++;
        //            WarcRecord wr = new WarcRecord();
        //            //Fill the Header
        //            Debug.WriteLine("Processing header for file # " + fileNum + " - " + FileName);
        //            lineNum = ProcessHeader(lines, lineNum, wr, outputDir);

        //            //Fill the Content
        //            lineNum = ProcessContent(lines, lineNum, wr, fileNum, outputDir);
        //            returnValue.Add(wr);
        //        }
        //        return returnValue;
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.ToString());
        //        return null;
        //    }
        //}

        ///// <summary>
        ///// Processes the content
        ///// </summary>
        ///// <param name="lines"></param>
        ///// <param name="startingLineNum"></param>
        ///// <param name="wr"></param>
        ///// <returns></returns>
        //private static int ProcessContent(string[] lines, int startingLineNum, WarcRecord wr, int fileNum, string outputDir)
        //{
        //    try
        //    {
        //        bool foundHeader = false;
        //        StringBuilder sb = new StringBuilder();
        //        for (int lineNum = 0; lineNum <= lines.Length - 1; lineNum++)
        //        {
        //            //Skip until we come to the right point in the file
        //            Stopwatch sw = new Stopwatch();
        //            if (lineNum <= startingLineNum) continue;
        //            sw.Start();
        //            string line = lines[lineNum];
        //            if (line.ToUpper().StartsWith("WARC/"))
        //            {
        //                wr.Content = sb.ToString().Trim();
        //                lineNum++;
        //                return lineNum;
        //            }

        //            //Skip until we come to the actual header, just write the line to the header
        //            if (!foundHeader && line.Trim() != "")
        //            {
        //                SetHeaderValue(wr, line, outputDir);
        //                continue;
        //            }
        //            if (!foundHeader && line.Trim() == "")
        //            {
        //                foundHeader = true; //set our foundHeader flag
        //                continue;
        //            }

        //            if (wr.WARC_Type != WarcType.Response) continue; //Skip writing if it's not a response, we don't care
        //            //Write this line to the output file, but only if it's not blank
        //            if (line.Trim() == "") continue;
        //            string fName = GetSanitizedFileName(wr.WARC_Target_URI == null ? wr.WARC_Record_ID.AbsoluteUri :
        //                wr.WARC_Target_URI.AbsoluteUri, outputDir);
        //            if (File.Exists(fName)) File.Delete(fName);
        //            using (StreamWriter swOutput = new StreamWriter(fName, true))
        //            {
        //                swOutput.WriteLine(line);
        //            }

        //            sb.AppendLine(line);
        //            string val = sb.ToString();
        //            Debug.WriteLine("FileNum: " + fileNum + ", Line: " + lineNum + " of " + lines.Length);
        //            sw.Stop();
        //        }

        //        //We're at the end of this file
        //        wr.Content = sb.ToString().Trim();
        //        Debug.WriteLine("End of file reached");
        //        return -1;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Failed to process content.", ex);
        //    }
        //}




        /// <summary>
        /// Processes the Header
        /// </summary>
        /// <returns></returns>
        //private static int ProcessHeader(string[] lines, int startingLineNumber, WarcRecord wr, string outputDir)
        //{
        //    try
        //    {
        //        int LineNum = 0;
        //        foreach (string line in lines)
        //        {
        //            if (LineNum < startingLineNumber)
        //            {
        //                LineNum++;
        //                continue;
        //            }
        //            if (line.Trim() == "")
        //            {
        //                return LineNum;
        //            }
        //            SetHeaderValue(wr, line, outputDir);
        //            LineNum++;
        //        }
        //        throw new Exception("Failed to find header");

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Failed to process header.", ex);
        //    }
        //}

        /// <summary>
        /// An identifier assigned to the current record that is globally unique for its period of intended use. No identifier scheme is mandated by this specification, but each record-id shall be a legal URI and clearly indicate a documented and registered scheme to which it conforms (e.g., via a URI scheme prefix such as “http:” or “urn:”). Care should be taken to ensure that this value is written with no internal whitespace.
        /// WARC-Record-ID = "WARC-Record-ID" ":" uri
        /// All records shall have a WARC-Record-ID field.
        /// </summary>
        public Uri WARC_Record_ID { get; set; }

        /// <summary>
        /// The number of octets in the block, similar to [RFC2616]. If no block is present, a value of ‘0’ (zero) shall be used.
        /// Content-Length = "Content-Length" ":" 1\*DIGIT
        /// All records shall have a Content-Length field.
        /// </summary>
        public int Content_Length { get; set; }

        /// <summary>
        /// A 14-digit UTC timestamp formatted according to YYYY-MM-DDThh:mm:ssZ, described in the W3C profile of ISO8601[W3CDTF]. The timestamp shall represent the instant that data capture for record creation began.Multiple records written as part of a single capture event (see section 5.7) shall use the same WARC-Date, even though the times of their writing will not be exactly synchronized.
        /// WARC-Date = "WARC-Date" ":" w3c-iso8601
        /// w3c-iso8601 = <YYYY-MM-DDThh:mm:ssZ>
        /// All records shall have a WARC-Date field.
        /// </summary>
        public DateTime WARC_Date { get; set; }

        /// <summary>
        /// WARC Type to be referenced by WARC_Type
        /// </summary>
        public enum WarcType
        {
            WarcInfo,
            Response,
            Resource,
            Request,
            MetaData,
            Revisit,
            Conversion,
            Continuation
        }

        /// <summary>
        /// The type of WARC record: one of ‘warcinfo’, ‘response’, ‘resource’, ‘request’, ‘metadata’, ‘revisit’, ‘conversion’, or ‘continuation’. Other types of WARC records may be defined in extensions of the core format. Types are further described in WARC Record Types.
        /// A WARC file needs not contain any particular record types, though starting all WARC files with a “warcinfo” record is recommended.
        /// WARC-Type   = "WARC-Type" ":" record-type
        /// record-type = "warcinfo" | "response" | "resource"
        /// | "request" | "metadata" | "revisit"
        /// | "conversion" | "continuation" | future-type
        /// future-type = token
        /// All records shall have a WARC-Type field.
        /// WARC processing software shall ignore records of unrecognized type.
        /// </summary>
        public WarcType? WARC_Type { get; set; }

        /// <summary>
        /// The MIME type [RFC2045] of the information contained in the record’s block. For example, in HTTP request and response records, this would be ‘application/http’ as per section 19.1 of [RFC2616] (or ‘application/http; msgtype=request’ and ‘application/http; msgtype=response’ respectively). In particular, the content-type is not the value of the HTTP Content-Type header in an HTTP response but a MIME type to describe the full archived HTTP message (hence ‘application/http’ if the block contains request or response headers).
        /// Content-Type = "Content-Type" ":" media-type
        /// media-type   = type "/" subtype* ( ";" parameter )
        /// type         = token
        /// subtype = token
        /// parameter    = attribute "=" value
        /// attribute = token
        /// value        = token | quoted-string
        /// All records with a non-empty block(non-zero Content-Length), except ‘continuation’ records, should have a Content-Type field.Only if the media type is not given by a Content-Type field, a reader may attempt to guess the media type via inspection of its content and/or the name extension(s) of the URI used to identify the resource.If the media type remains unknown, the reader should treat it as type “application/octet-stream”.
        /// </summary>
        public ContentType Content_Type { get; set; }

        /// <summary>
        /// The WARC-Record-IDs of any records created as part of the same capture event as the current record. A capture event comprises the information automatically gathered by a retrieval against a single target-URI; for example, it might be represented by a ‘response’ or ‘revisit’ record plus its associated ‘request’ record.
        /// WARC-Concurrent-To = "WARC-Concurrent-To" ":" uri
        /// This field may be used to associate records of types ‘request’, ‘response’, ‘resource’, ‘metadata’, and ‘revisit’ with one another when they arise from a single capture event (When so used, any WARC-Concurrent-To association shall be considered bidirectional even if the header only appears on one record.) The WARC Concurrent-to field shall not be used in ‘warcinfo’, ‘conversion’, and ‘continuation’ records.
        /// As an exception to the general rule, it is allowed to repeat several WARC-Concurrent-To fields within the same WARC record.
        /// </summary>
        public HashSet<Uri> WARC_Concurrent_To { get; set; } = new HashSet<Uri>();

        /// <summary>
        /// Digest structure to be referenced by WARC_Block_Digest and WARC_Payload_Digest
        /// </summary>
        public class Digest
        {
            public string Algorithm { get; set; }
            public string Value { get; set; }

            public Digest(string algorithm, string value)
            {
                Algorithm = algorithm;
                Value = value;
            }
        }

        /// <summary>
        /// An optional parameter indicating the algorithm name and calculated value of a digest applied to the full block of the record.
        /// WARC-Block-Digest = "WARC-Block-Digest" ":" labelled-digest
        /// labelled-digest   = algorithm ":" digest-value
        /// algorithm = token
        /// digest-value      = token
        /// An example is a SHA-1 labelled Base32([RFC3548]) value:
        /// WARC-Block-Digest: sha1:AB2CD3EF4GH5IJ6KL7MN8OPQ
        /// This document recommends no particular algorithm.
        /// Any record may have a WARC-Block-Digest field.
        /// </summary>
        public Digest WARC_Block_Digest;

        /// <summary>
        /// An optional parameter indicating the algorithm name and calculated value of a digest applied to the payload referred to or contained by the record - which is not necessarily equivalent to the record block.
        /// WARC-Payload-Digest = "WARC-Payload-Digest" ":" labelled-digest
        /// An example is a SHA-1 labelled Base32([RFC3548]) value:
        /// WARC-Payload-Digest: sha1:3EF4GH5IJ6KL7MN8OPQAB2CD
        /// This document recommends no particular algorithm.
        /// The payload of an application/http block is its ‘entity-body’ (per[RFC2616]). In contrast to WARC-Block-Digest, the WARC-Payload-Digest field may also be used for data not actually present in the current record block, for example when a block is left off in accordance with a ‘revisit’ profile(see ‘revisit’), or when a record is segmented(the WARC-Payload-Digest recorded in the first segment of a segmented record shall be the digest of the payload of the logical record).
        /// The WARC-Payload-Digest field may be used on WARC records with a well-defined payload and shall not be used on records without a well-defined payload.
        /// </summary>
        public Digest WARC_Payload_Digest;

        /// <summary>
        /// The numeric Internet address contacted to retrieve any included content. An IPv4 address shall be written as a “dotted quad”; an IPv6 address shall be written as per [RFC1884]. For an HTTP retrieval, this will be the IP address used at retrieval time corresponding to the hostname in the record’s target-Uri.
        /// WARC-IP-Address = "WARC-IP-Address" ":" (ipv4 | ipv6)
        /// ipv4            = <"dotted quad">
        /// ipv6            = <per section 2.2 of RFC1884>
        /// The WARC-IP-Address field may be used on ‘response’, ‘resource’, ‘request’, ‘metadata’, and ‘revisit’ records, but shall not be used on ‘warcinfo’, ‘conversion’ or ‘continuation’ records.
        /// </summary>
        public IPAddress WARC_IP_Address { get; set; }

        /// <summary>
        /// The WARC-Record-ID of a single record for which the present record holds additional content.
        /// WARC-Refers-To = "WARC-Refers-To" ":" uri
        /// The WARC-Refers-To field may be used to associate a ‘metadata’ record to another record it describes.The WARC-Refers-To field may also be used to associate a record of type ‘revisit’ or ‘conversion’ with the preceding record which helped determine the present record content.The WARC-Refers-To field shall not be used in ‘warcinfo’, ‘response’, ‘resource’, ‘request’, and ‘continuation’ records.
        /// </summary>
        public Uri WARC_Refers_To { get; set; }

        /// <summary>
        /// The original URI whose capture gave rise to the information content in this record. In the context of web harvesting, this is the URI that was the target of a crawler’s retrieval request. For a ‘revisit’ record, it is the URI that was the target of a retrieval request. Indirectly, such as for a ‘metadata’, or ‘conversion’ record, it is a copy of the WARC-Target-URI appearing in the original record to which the newer record pertains. The URI in this value shall be properly escaped according to [RFC3986] and written with no internal whitespace.
        /// WARC-Target-URI = "WARC-Target-URI" ":" uri
        /// All ‘response’, ‘resource’, ‘request’, ‘revisit’, ‘conversion’ and ‘continuation’ records shall have a WARC-Target-URI field.A ‘metadata’ record may have a WARC-Target-URI field. A ‘warcinfo’ record shall not have a WARC-Target-URI field.
        /// </summary>
        public Uri WARC_Target_URI { get; set; }

        /// <summary>
        /// WARC Truncated Reason to be referenced by WARC_Truncated
        /// </summary>
        public enum WarcTruncatedReason
        {
            Length,
            Time,
            Disconnect,
            Unspecified
        }

        /// <summary>
        /// For practical reasons, writers of the WARC format may place limits on the time or storage allocated to archiving a single resource. As a result, only a truncated portion of the original resource may be available for saving into a WARC record.
        /// Any record may indicate that truncation of its content block has occurred and give the reason with a ‘WARC-Truncated’ field.
        /// WARC-Truncated = "WARC-Truncated" ":" reason-token
        /// reason-token   = "length"          ; exceeds configured max
        /// ; length
        /// | "time"            ; exceeds configured max time
        /// | "disconnect"      ; network disconnect
        /// | "unspecified"     ; other/unknown reason
        /// future-reason  = token
        /// For example, if the capture of what appeared to be a multi-gigabyte resource was cut short after a transfer time limit was reached, the partial resource could be saved to a WARC record with this field.
        /// The WARC-Truncated field may be used on any WARC record.The WARC field Content-Length shall still report the actual truncated size of the record block.
        /// </summary>
        public WarcTruncatedReason? WARC_Truncated { get; set; }

        /// <summary>
        /// When present, indicates the WARC-Record-ID of the associated ‘warcinfo’ record for this record. Typically, the Warcinfo-ID parameter is used when the context of the applicable ‘warcinfo’ record is unavailable, such as after distributing single records into separate WARC files. WARC writing applications (such web crawlers) may choose to always record this parameter.
        /// WARC-Warcinfo-ID = "WARC-Warcinfo-ID" ":" uri
        /// The WARC-Warcinfo-ID field value overrides any association with a previously occurring(in the WARC) ‘warcinfo’ record, thus providing a way to protect the true association when records are combined from different WARCs.
        /// The WARC-Warcinfo-ID field may be used in any record type except ‘warcinfo’.
        /// </summary>
        public Uri WARC_Warcinfo_ID { get; set; }

        /// <summary>
        /// The filename containing the current ‘warcinfo’ record.
        /// WARC-Filename = "WARC-Filename" ":" (TEXT | quoted-string )
        /// The WARC-Filename field may be used in ‘warcinfo’ type records and shall not be used for other record types.
        /// </summary>
        public string WARC_Filename { get; set; }

        /// <summary>
        /// A URI signifying the kind of analysis and handling applied in a ‘revisit’ record. (Like an XML namespace, the URI may, but need not, return human-readable or machine-readable documentation.) If reading software does not recognize the given URI as a supported kind of handling, it shall not attempt to interpret the associated record block.
        /// WARC-Profile = "WARC-Profile" ":" uri
        /// The section ‘revisit’ defines two initial profile options for the WARC-Profile header for ‘revisit’ records.
        /// The WARC-Profile field is mandatory on ‘revisit’ type records and undefined for other record types.
        /// </summary>
        public Uri WARC_Profile { get; set; }

        /// <summary>
        /// The content-type of the record’s payload as determined by an independent check. This string shall not be arrived at by blindly promoting an HTTP Content-Type value up from a record block into the WARC header without direct analysis of the payload, as such values may often be unreliable.
        /// -Identified-Payload-Type = "WARC-Identified-Payload-Type" ":" 
        /// media-type
        /// The WARC-Identified-Payload-Type field may be used on WARC records with a well-defined payload and shall not be used on records without a well-defined payload.
        /// </summary>
        public string WARC_Identified_Payload_Type { get; set; }

        /// <summary>
        /// Reports the current record’s relative ordering in a sequence of segmented records.
        /// WARC-Segment-Number = "WARC-Segment-Number" ":" 1*DIGIT
        /// In the first segment of any record that is completed in one or more later ‘continuation’ WARC records, this parameter is mandatory.Its value there is “1”. In a ‘continuation’ record, this parameter is also mandatory.Its value is the sequence number of the current segment in the logical whole record, increasing by 1 in each next segment.
        /// See the section below, Record Segmentation, for full details on the use of WARC record segmentation.
        /// </summary>
        public int? WARC_Segment_Number { get; set; }

        /// <summary>
        /// Identifies the starting record in a series of segmented records whose content blocks are reassembled to obtain a logically complete content block.
        /// WARC-Segment-Origin-ID = "WARC-Segment-Origin-ID" ":" uri
        /// This field is mandatory on all ‘continuation’ records, and shall not be used in other records.See the section below, Record segmentation, for full details on the use of WARC record segmentation.
        /// </summary>
        public Uri WARC_Segment_Origin_ID { get; set; }

        /// <summary>
        /// In the final record of a segmented series, reports the total length of all segment content blocks when concatenated together.
        /// WARC-Segment-Total-Length = "WARC-Segment-Total-Length" ":"
        /// 1*DIGIT
        /// This field is mandatory on the last ‘continuation’ record of a series, and shall not be used elsewhere.
        /// See the section below, Record segmentation, for full details on the use of WARC record segmentation.
        /// </summary>
        public int? WARC_Segment_Total_Length { get; set; }

        public static bool FileIsWARC(string FileName)
        {
            try
            {

                if (!File.ReadLines(FileName).First().ToUpper().StartsWith("WARC")) return false;

                //Make sure at least the mandatory fields are populated in the header

                WarcRecord wr = new WarcRecord();

                using (var streamReader = new StreamReader(new FileStream(FileName, FileMode.Open)))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        //Keep checking and filling the header until either we have a full header,
                        //or we don't and reach the end of the header without a the minimum met
                        if (!SetHeaderValue(wr, line)) throw new Exception("Failed to set Header Value of '" + line + "'");
                        if (WARCHeaderMinimumMet(wr)) return true;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to determine WARC status of file.", ex);
            }
        }

        /// <summary>
        /// Sets the header value based on the passed in string/line in a file
        /// </summary>
        /// <param name="wr">WarcRecord object to populate</param>
        /// <param name="value">String value containing the header object name and value 
        /// i.e. WARC-IP-Address: 23.0.160.82 </param>
        /// <returns>True or false depending on whether or not it successfully set the value</returns>
        public static bool SetHeaderValue(WarcRecord wr, string value)
        {
            try
            {
                //Ignore the first line, and don't error on it
                if (!value.ToUpper().StartsWith("WARC/") && !value.Contains(":")) return false;
                if (value.Contains(":"))
                {
                    string[] valueElements = value.Split(':');
                    switch (valueElements[0].Trim().ToUpper())
                    {
                        case "WARC-RECORD-ID":
                            wr.WARC_Record_ID = new Uri(value.Replace(valueElements[0].Trim() + ":", "").Trim().Replace("<", "").Replace(">", ""));
                            break;
                        case "CONTENT-LENGTH":
                            wr.Content_Length = int.Parse(valueElements[1].Trim());
                            break;
                        case "WARC-DATE":
                            wr.WARC_Date = DateTime.Parse(value.Replace(valueElements[0].Trim() + ":", "").Trim());
                            break;
                        case "WARC-TYPE":
                            wr.WARC_Type = ConvertTypeTextToType(valueElements[1].Trim());
                            break;
                        case "CONTENT-TYPE":
                            wr.Content_Type = new ContentType(valueElements[1].Trim());
                            break;
                        case "WARC-CONCURRENT-TO":
                            wr.WARC_Concurrent_To.Add(new Uri(value.Replace(valueElements[0].Trim() + ":", "").Trim().Replace("<", "").Replace(">", "")));
                            break;
                        case "WARC-BLOCK-DIGEST":
                            wr.WARC_Block_Digest = new Digest(valueElements[1].Trim(), valueElements[2].Trim());
                            break;
                        case "WARC-PAYLOAD-DIGEST":
                            wr.WARC_Payload_Digest = new Digest(valueElements[1].Trim(), valueElements[2].Trim());
                            break;
                        case "WARC-IP-ADDRESS":
                            wr.WARC_IP_Address = IPAddress.Parse(valueElements[1].Trim());
                            break;
                        case "WARC-REFERS-TO":
                            wr.WARC_Refers_To = new Uri(value.Replace(valueElements[0].Trim() + ":", "").Trim().Replace("<", "").Replace(">", ""));
                            break;
                        case "WARC-TARGET-URI":
                            wr.WARC_Target_URI = new Uri(value.Replace(valueElements[0].Trim() + ":", "").Trim().Replace("<", "").Replace(">", ""));
                            break;
                        case "WARC-TRUNCATED":
                            wr.WARC_Truncated = ConvertReasonTextToReason(valueElements[1].Trim());
                            break;
                        case "WARC-WARCINFO-ID":
                            wr.WARC_Warcinfo_ID = new Uri(value.Replace(valueElements[0].Trim() + ":", "").Trim().Replace("<", "").Replace(">", ""));
                            break;
                        case "WARC-FILENAME":
                            wr.WARC_Filename = valueElements[1].Trim();
                            break;
                        case "WARC-PROFILE":
                            wr.WARC_Profile = new Uri(value.Replace(valueElements[0].Trim() + ":", "").Trim().Replace("<", "").Replace(">", ""));
                            break;
                        case "WARC-IDENTIFIED-PAYLOAD-TYPE":
                            wr.WARC_Identified_Payload_Type = valueElements[1].Trim();
                            break;
                        case "WARC-SEGMENT-NUMBER":
                            wr.WARC_Segment_Number = int.Parse(valueElements[1].Trim());
                            break;
                        case "WARC-SEGMENT-ORIGIN-ID":
                            wr.WARC_Segment_Origin_ID = new Uri(value.Replace(valueElements[0].Trim() + ":", "").Trim().Replace("<", "").Replace(">", ""));
                            break;
                        case "WARC-SEGMENT-TOTAL-LENGTH":
                            wr.WARC_Segment_Total_Length = int.Parse(valueElements[1].Trim());
                            break;
                        default:
                            break; //Don't set, just write
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Method to convert a string WARCType into a WarcType enumeration value
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static WarcType ConvertTypeTextToType(string type)
        {
            type = type.Trim().ToLower();
            return type switch
            {
                "warcinfo" => WarcType.WarcInfo,
                "response" => WarcType.Response,
                "resource" => WarcType.Resource,
                "request" => WarcType.Request,
                "metadata" => WarcType.MetaData,
                "revisit" => WarcType.Revisit,
                "conversion" => WarcType.Conversion,
                "continuation" => WarcType.Continuation,
                _ => throw new Exception("Invalid WarcType of '" + type + "'"),
            };
        }

        /// <summary>
        /// Method to convert a string WARC-Truncated Reason into a Warc Truncated Reason enumeration value
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        private static WarcTruncatedReason ConvertReasonTextToReason(string reason)
        {
            reason = reason.Trim().ToLower();
            return reason switch
            {
                "length" => WarcTruncatedReason.Length,
                "time" => WarcTruncatedReason.Time,
                "disconnect" => WarcTruncatedReason.Disconnect,
                "unspecified" => WarcTruncatedReason.Unspecified,
                _ => throw new Exception("Invalid WARC Truncated Reason of '" + reason + "'"),
            };
        }

        /// <summary>
        /// Method to determine if a WarcRecord's minimal header requirement has been met as per ISO standards
        /// </summary>
        /// <param name="wr"></param>
        /// <returns></returns>
        public static bool WARCHeaderMinimumMet(WarcRecord wr)
        {
            try
            {
                if (wr.WARC_Record_ID == null) return false;
                if (wr.Content_Length == 0) return false;
                if (wr.WARC_Date == null) return false;
                if (wr.WARC_Type == null) return false;
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to determine if WARC Header's minimum is met.", ex);
            }
        }
    }
}
