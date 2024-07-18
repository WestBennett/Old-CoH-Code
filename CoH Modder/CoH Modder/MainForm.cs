using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoH_Modder
{
    public partial class MainForm : Form
    {
        public string modDirectory;
        public string AllModsInfo = "AllMods.Info";
        public DataTable ServerMods = null;

        //public static string FTPServerName;
        //public static string FTPUserName;
        //public static string FTPPassword;

        public static string FTPServerName = "";
        public static string FTPUserName = "";
        public static string FTPPassword = "";


        public DataTable GetAllServerMods()
        {
            try
            {
                FTP_Client ftp = new FTP_Client(FTPServerName, FTPUserName, FTPPassword);
                if (!ftp.FileExists(AllModsInfo)) throw new Exception("All Mods Info file does not exist. Please click the " +
                    "'Fresh Start' button to regenerate this file, or report the issue to the program developer.");

                string TempFileName = Path.GetTempFileName();
                if (File.Exists(TempFileName)) File.Delete(TempFileName);

                ftp.GetFile(ProgressBar, AllModsInfo, TempFileName);
                DataSet ds = new DataSet();
                ds.ReadXml(TempFileName);
                if (File.Exists(TempFileName)) File.Delete(TempFileName);
                if (ds.Tables.Count > 0) return ds.Tables[0];
                return null;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return null;
            }
        }

        public static DataColumn[] GetModColumns()
        {
            return new DataColumn[]
            {
                new DataColumn("ModName"),
                new DataColumn("ModCategory"),
                new DataColumn("ModAuthor"),
                new DataColumn("ModDescription"),
                new DataColumn("ModVersion", typeof(int)),
                new DataColumn("ModFileName"),
                new DataColumn("DisplayDescription")
            };
        }

        public static void LogError(Exception ex)
        {
            MessageBox.Show("Error in application:" + Environment.NewLine + ex.ToString(), "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public MainForm()
        {
            InitializeComponent();
            Show();
            ServerMods = GetAllServerMods();
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            try
            {
                txtCoHRootDirectory.Text = Properties.Settings.Default.CoHRootPath;
                RootDirectoryHasCoH();
                RefreshData();
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private void ProcessModUpdate()
        {
            try
            {
                if (MessageBox.Show(@"This option will delete ALL data in the Data\ and Mods\ directories, " +
                    "remove ALL installed and downloaded mods, then download EVERY mod on the server, check their data, " +
                    "and update the server with the accumulated information. While it does download the mods, it does NOT install them. " +
                    "This could take quite a while. Are you SURE that you want to do this?", "Refresh ALL Mod Information???",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
                Enabled = false;

                //Delete everything in the Data\ and Mods\ directories
                string DataDir = txtCoHRootDirectory.Text + @"Data\";
                if (Directory.Exists(modDirectory)) Directory.Delete(modDirectory, true);
                if (Directory.Exists(DataDir)) Directory.Delete(DataDir, true);
                if (!Directory.Exists(DataDir)) Directory.CreateDirectory(DataDir);
                if (!Directory.Exists(modDirectory)) Directory.CreateDirectory(modDirectory);

                //Re-download EVERY mod on the list
                FTP_Client ftp = new FTP_Client(FTPServerName, FTPUserName, FTPPassword);
                DataTable AllMods = new DataTable("AllMods");
                AllMods.Columns.AddRange(GetModColumns());
                foreach (string fileName in ftp.GetDirectoryListing(""))
                {
                    if (Path.GetFileName(fileName) == AllModsInfo) continue;
                    if (Path.GetExtension(fileName).ToUpper() != ".ZIP") continue;
                    if (!ftp.FileExists(fileName)) continue;
                    StatusLabel.Text = "Downloading file '" + fileName + "'";
                    ftp.GetFile(ProgressBar, fileName, modDirectory + Path.GetFileName(fileName));
                    //Seek inside of each of them for their Mod Info, and generate a new ultimate Mod List
                    ModFile mf = new ModFile(modDirectory + Path.GetFileName(fileName));
                    if (mf.ModCategory.Trim() == "") mf.ModCategory = "OTHER";
                    AllMods.Rows.Add(mf.ModName, mf.ModCategory, mf.ModAuthor, mf.ModDescription, mf.ModVersion, mf.ModFileName);
                }

                //Save to a new file
                if (File.Exists(modDirectory + AllModsInfo)) File.Delete(modDirectory + AllModsInfo);
                DataSet ds = new DataSet();
                ds.Tables.Add(AllMods);
                ds.WriteXml(modDirectory + AllModsInfo);

                //Upload that Mod List to the server
                if (ftp.FileExists(AllModsInfo)) ftp.DeleteFile(AllModsInfo);
                ftp.UploadFile(modDirectory + AllModsInfo, AllModsInfo, ProgressBar);

                //Delete the local copy
                File.Delete(modDirectory + AllModsInfo);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            finally
            {
                Enabled = true;
                RefreshData();
            }
        }

        private bool RootDirectoryHasCoH()
        {
            try
            {
                if (!Directory.Exists(txtCoHRootDirectory.Text)) BtnPickRoot_Click(this, new EventArgs());
                List<string> cohClients = new List<string>() { "cityofheroes.exe", "homecoming.exe",
                    "homecoming-beta.exe", "score.exe", "coxg.exe" };
                foreach (string fileName in Directory.GetFiles(txtCoHRootDirectory.Text, "*.exe"))
                {
                    if (cohClients.Contains(Path.GetFileName(fileName).ToLower())) return true;
                }

                //Check for the new Homecoming launcher
                if (Directory.GetFiles(txtCoHRootDirectory.Text + @"bin\win32\live\", "cityofheroes.exe").Length > 0 ||
                    Directory.GetFiles(txtCoHRootDirectory.Text + @"bin\win64\live\", "cityofheroes.exe").Length > 0) return true;

                return false;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;
            }
        }

        private void LoadMods(ListBox lst, string Category)
        {
            try
            {
                txtInstalledModDescription.Text = "";
                txtUninstalledModDescription.Text = "";
                if (!Directory.Exists(txtCoHRootDirectory.Text))
                {
                    MessageBox.Show("Directory '" + txtCoHRootDirectory.Text + "' is not a valid directory. Please click the Browse button to choose " +
                        "the correct CoH installation directory.", "Invalid Directory", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                modDirectory = txtCoHRootDirectory.Text + @"mods\";
                string sourceDirectory;
                if (lst.Name == nameof(lstInstalledMods)) sourceDirectory = modDirectory + @"installed\";
                else sourceDirectory = modDirectory;
                if (!Directory.Exists(sourceDirectory)) Directory.CreateDirectory(sourceDirectory);

                DataTable Mods = new DataTable("ModInfo");
                Mods.Columns.AddRange(GetModColumns());

                foreach (string fileName in System.IO.Directory.GetFiles(sourceDirectory))
                {
                    if (!ModFile.ValidModFile(fileName)) continue;
                    ModFile m = new ModFile(fileName);
                    if (m.ModCategory == "") m.ModCategory = "OTHER";
                    if (Category != "ALL" && m.ModCategory != Category) continue; //Skip if it doesn't match the chosen category
                    Mods.Rows.Add(m.ModName, m.ModCategory, m.ModAuthor, m.ModDescription, m.ModVersion, m.ModFileName,
                        m.ModName + " (v" + m.ModVersion + ")");
                }

                Mods.DefaultView.Sort = "DisplayDescription";
                Mods = Mods.DefaultView.ToTable();
                lst.ValueMember = "ModName";
                lst.DisplayMember = "DisplayDescription";
                lst.DataSource = Mods;
                lst.ClearSelected();
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private void BtnPickRoot_Click(object sender, EventArgs e)
        {
            try
            {
                txtCoHRootDirectory.Text = "";
                FolderBrowserDialog fbd = new FolderBrowserDialog
                {
                    Description = "Choose the City of Heroes Install Directory..."
                };
                if (fbd.ShowDialog() == DialogResult.Cancel || !Directory.Exists(fbd.SelectedPath)) return;
                txtCoHRootDirectory.Text = fbd.SelectedPath + @"\";
                if (!RootDirectoryHasCoH())
                {
                    txtCoHRootDirectory.Text = "";
                    MessageBox.Show("Cannot find City of Heroes in this directory. Please choose a different directory.", "No CoH Found",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Properties.Settings.Default.CoHRootPath = txtCoHRootDirectory.Text;
                Properties.Settings.Default.Save();

                RefreshData();
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

        }

        private void BtnCreateMod_Click(object sender, EventArgs e)
        {
            new CreateEditMod(this).ShowDialog();
            RefreshData();
        }

        private DataRow FixBadDR(DataRow dr)
        {
            try
            {
                if (dr.Table.Rows.Count == 1) return dr;
                DataTable dt = dr.Table.Copy();
                for (int i = 0; i <= dt.Rows.Count - 1; i++) if (dt.Rows[i]["ModName"].ToString().Trim() == "") dt.Rows.Remove(dt.Rows[i]);
                if (dt.Rows.Count == 1) dr = dt.Rows[0];
                return dr;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return null;
            }
        }

        private void LstUninstalledMods_SelectedValueChanged(object sender, EventArgs e)
        {
            txtUninstalledModDescription.Text = "";
            btnInstall.Enabled = false;
            btnDelete.Enabled = false;
            if (lstUninstalledMods.SelectedValue != null && lstUninstalledMods.SelectedValue.ToString().Trim() != "")
            {
                //Find the correct datarow and display the description
                DataRow[] drs = ((DataTable)lstUninstalledMods.DataSource).Select("ModName = '" + lstUninstalledMods.SelectedValue + "'");
                //Find all of the mod info and display it
                ModFile mf = new ModFile(FixBadDR(drs[0])["ModFileName"].ToString());
                txtUninstalledModDescription.Text = "Mod Name: " + mf.ModName + Environment.NewLine +
                    "Mod Author: " + mf.ModAuthor + Environment.NewLine +
                    "Mod Version: " + mf.ModVersion + Environment.NewLine +
                    "Mod Description: " + mf.ModDescription;
                btnInstall.Enabled = true;
                btnDelete.Enabled = true;
            }
        }

        private void LstInstalledMods_SelectedValueChanged(object sender, EventArgs e)
        {
            txtInstalledModDescription.Text = "";
            btnRemove.Enabled = false;
            if (lstInstalledMods.SelectedValue != null)
            {
                //Find the correct datarow and display the description
                DataRow[] drs = ((DataTable)lstInstalledMods.DataSource).Select("ModName = '" + lstInstalledMods.SelectedValue + "'");
                //Find all of the mod info and display it
                ModFile mf = new ModFile(drs[0]["ModFileName"].ToString());
                txtInstalledModDescription.Text = "Mod Name: " + mf.ModName + Environment.NewLine +
                    "Mod Author: " + mf.ModAuthor + Environment.NewLine +
                    "Mod Version: " + mf.ModVersion + Environment.NewLine +
                    "Mod Description: " + mf.ModDescription;
                btnRemove.Enabled = true;
            }
        }

        private void BtnInstall_Click(object sender, EventArgs e)
        {
            try
            {
                if (lstUninstalledMods.SelectedValue == null) return;
                InstallMod(lstUninstalledMods.SelectedValue.ToString());
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private void InstallMod(string ModName)
        {
            try
            {
                ModFile.WaitCursor(this, true);
                StatusLabel.Text = "Installing Mod '" + ModName + "'";
                DataRow[] drs = ((DataTable)lstUninstalledMods.DataSource).Select("ModName = '" + ModName + "'");
                ModFile mf = new ModFile(drs[0]["ModFileName"].ToString());
                string fileName = mf.ModFileName;
                //Get the file and unzip it
                using (ZipArchive archive = ZipFile.OpenRead(fileName))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        //Match on the file name to find the files to install
                        foreach (KeyValuePair<string, string> kvp in mf.InstallFiles)
                        {
                            if (Path.GetFileName(kvp.Key) != entry.FullName) continue;

                            //Backup the existing file if it exists
                            string destinationFile = txtCoHRootDirectory.Text + (kvp.Value.EndsWith(@"\") ? kvp.Value + Path.GetFileName(kvp.Key) : kvp.Value);

                            //For each InstallFile, find the location, and create it if it doesn't exist
                            if (!Directory.Exists(Path.GetDirectoryName(destinationFile))) Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));

                            if (File.Exists(destinationFile))
                            {
                                string backupDir = modDirectory + @"backup\";
                                if (!Directory.Exists(backupDir)) Directory.CreateDirectory(backupDir);
                                string modBackupDir = backupDir + MakeValidFileName(mf.ModName) + @"\";
                                if (!Directory.Exists(modBackupDir)) Directory.CreateDirectory(modBackupDir);
                                if (File.Exists(modBackupDir + Path.GetFileName(destinationFile))) File.Delete(modBackupDir + Path.GetFileName(destinationFile));
                                File.Copy(destinationFile, modBackupDir + Path.GetFileName(destinationFile));
                                if (File.Exists(destinationFile)) File.Delete(destinationFile);
                            }
                            if (!Directory.Exists(Path.GetDirectoryName(destinationFile))) Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));
                            entry.ExtractToFile(destinationFile);
                        }
                    }
                }

                //Move the Installed zip file to the "Installed" directory
                if (File.Exists(modDirectory + @"installed\" + Path.GetFileName(mf.ModFileName))) File.Delete(modDirectory + @"installed\" + Path.GetFileName(mf.ModFileName));
                File.Move(mf.ModFileName, modDirectory + @"installed\" + Path.GetFileName(mf.ModFileName));

                RefreshData();
                MessageBox.Show("Mod '" + mf.ModName + "' installed successfully.", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            finally
            {
                ModFile.WaitCursor(this, false);
                StatusLabel.Text = "Ready...";
            }
        }

        private static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            try
            {
                ModFile.WaitCursor(this, true);
                DataRow[] drs = ((DataTable)lstInstalledMods.DataSource).Select("ModName = '" + lstInstalledMods.SelectedValue + "'");
                ModFile mf = new ModFile(drs[0]["ModFileName"].ToString());
                string fileName = mf.ModFileName;
                //Get the file and unzip it
                using (ZipArchive archive = ZipFile.OpenRead(fileName))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        //Match on the file name to find the files to install
                        foreach (KeyValuePair<string, string> kvp in mf.InstallFiles)
                        {
                            if (Path.GetFileName(kvp.Key) != entry.FullName) continue;

                            //Backup the existing file if it exists
                            string destinationFile = txtCoHRootDirectory.Text +
                                (kvp.Value.EndsWith(@"\") ? kvp.Value + Path.GetFileName(kvp.Key) : kvp.Value);

                            //Uninstall the mod file
                            if (File.Exists(destinationFile)) File.Delete(destinationFile);

                            //If there were any old files that were backed up when the mod was installed, re-install those
                            string backupDir = modDirectory + @"backup\";
                            string modBackupDir = backupDir + MakeValidFileName(mf.ModName) + @"\";
                            if (Directory.Exists(modBackupDir))
                            {
                                if (File.Exists(modBackupDir + Path.GetFileName(destinationFile))) File.Move(
                                    modBackupDir + Path.GetFileName(destinationFile), destinationFile);

                                //If the directory is empty, then delete it
                                if (Directory.Exists(modBackupDir) && Directory.GetFiles(modBackupDir).Length == 0) Directory.Delete(modBackupDir);
                            }
                        }
                    }
                }

                //Move the Installed zip file to the "Installed" directory
                //Delete it if it already exists there first
                if (File.Exists(modDirectory + Path.GetFileName(mf.ModFileName))) File.Delete(modDirectory + Path.GetFileName(mf.ModFileName));
                File.Move(mf.ModFileName, modDirectory + Path.GetFileName(mf.ModFileName));

                RefreshData();
                ModFile.WaitCursor(this, false);
                MessageBox.Show("Mod '" + mf.ModName + "' removed successfully.",
                    "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private void RefreshData()
        {
            string Category = cboCategory.Text.ToString().Trim() == "" ? "ALL" : cboCategory.Text.ToString();
            ModFile.WaitCursor(this, true);

            LoadMods(lstInstalledMods, Category);
            LoadMods(lstUninstalledMods, Category);

            //Get the list of Server Mods
            GetServerMods(Category);

            lstInstalledMods.ClearSelected();
            lstUninstalledMods.ClearSelected();
            btnInstall.Enabled = false;
            btnRemove.Enabled = false;

            StatusLabel.Text = lstServerMods.Items.Count + " Server Mod(s), " + lstUninstalledMods.Items.Count +
                " Local Mods, and " + lstInstalledMods.Items.Count + " Installed Mod(s) Found in the '" + modDirectory + "' directory.";
            ModFile.WaitCursor(this, false);
        }

        private void GetServerMods(string Category)
        {
            try
            {
                StatusLabel.Text = "Getting list of server mods...";
                Cursor = Cursors.WaitCursor;
                Enabled = false;
                FTP_Client ftp = new FTP_Client(FTPServerName, FTPUserName, FTPPassword);
                if (!ftp.FileExists(AllModsInfo)) throw new Exception("All Mods Info file does not exist. Please click the " +
                    "'Fresh Start' button to regenerate this file, or report the issue to the program developer.");

                ftp.GetFile(ProgressBar, AllModsInfo, txtCoHRootDirectory.Text + AllModsInfo);

                DataSet ds = new DataSet();
                ds.ReadXml(txtCoHRootDirectory.Text + AllModsInfo);
                DataTable dt = ds.Tables[0];
                DataTable dtNew = dt.Copy();

                foreach (DataRow dr in dt.Rows)
                {
                    string ModName = dr["ModName"].ToString();
                    DataRow[] drs = dt.Select("ModName = '" + ModName + "'");
                    if (drs.Length > 1)
                    {
                        //Fix the file with only the newest version
                        int HighestVersion = 0;
                        foreach (DataRow drVersion in drs)
                        {
                            if (int.Parse(drVersion["ModVersion"].ToString()) > HighestVersion) HighestVersion = int.Parse(drVersion["ModVersion"].ToString());
                        }

                        List<DataRow> drsToRemove = new List<DataRow>();
                        foreach (DataRow drToCheck in dtNew.Select("ModName = '" + ModName + "'"))
                        {
                            int thisVersion = int.Parse(drToCheck["ModVersion"].ToString());
                            if (thisVersion < HighestVersion) drsToRemove.Add(drToCheck);
                        }

                        foreach (DataRow drToRemove in drsToRemove)
                        {
                            dtNew.Rows.Remove(drToRemove);
                        }
                    }
                }

                if (dtNew.Rows.Count != dt.Rows.Count)
                {
                    //Update the server with the fixed list
                    dt = dtNew;
                    ds.Tables.Clear();
                    ds.Tables.Add(dt);
                    string tempFileName = Path.GetTempPath() + AllModsInfo;
                    ds.WriteXml(tempFileName);
                    ftp.UploadFile(tempFileName, AllModsInfo, ProgressBar);
                    if (File.Exists(tempFileName)) File.Delete(tempFileName);
                }

                dt.PrimaryKey = new DataColumn[] { dt.Columns["ModName"] };

                List<DataRow> rowsToRemove = new List<DataRow>();
                foreach (DataRow dr in dt.Rows)
                {
                    //If the Mod is listed in the list of Installed mods WITH THE SAME VERSION, don't show it here
                    DataRow[] drs = ((DataTable)lstInstalledMods.DataSource).Select("ModName = '" + dr["ModName"] + "' AND ModVersion = '" + dr["ModVersion"] + "'");
                    if (drs.Length > 0 && !rowsToRemove.Contains(dr))
                    {
                        rowsToRemove.Add(dr);
                        continue;
                    }

                    //If the Mod is already downloaded and local WITH THE SAME VERSION, don't show it here
                    drs = ((DataTable)lstUninstalledMods.DataSource).Select("ModName = '" + dr["ModName"] + "' AND ModVersion = '" + dr["ModVersion"] + "'");
                    if (drs.Length > 0 && !rowsToRemove.Contains(dr))
                    {
                        rowsToRemove.Add(dr);
                        continue;
                    }

                    //If the category isn't "All" and the category chosen doesn't match the category in the mod, then skip it
                    string modCategory = dr["ModCategory"].ToString();
                    if (modCategory.Trim() == "") modCategory = "OTHER";
                    if (Category != "ALL" && modCategory != Category) rowsToRemove.Add(dr);

                }

                if (rowsToRemove.Count > 0)
                {
                    foreach (DataRow dr in rowsToRemove)
                    {
                        DataRow[] drs = dt.Select("ModName = '" + dr["ModName"] + "'");
                        if (drs.Length > 0) dt.Rows.Remove(drs[0]);
                    }
                }

                //Create the DisplayDescription
                if (!dt.Columns.Contains("DisplayDescription")) dt.Columns.Add("DisplayDescription");

                foreach (DataRow dr in dt.Rows)
                {
                    dr["DisplayDescription"] = dr["ModName"] + " (v" + dr["ModVersion"] + ")";
                }

                dt.DefaultView.Sort = "DisplayDescription";
                dt = dt.DefaultView.ToTable();
                lstServerMods.ValueMember = "ModName";
                lstServerMods.DisplayMember = "DisplayDescription";
                lstServerMods.DataSource = dt;
                lstServerMods.ClearSelected();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get list of server mods due to the following error: " + ex.ToString(),
                    "Can't Get Server Mods", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                StatusLabel.Text = "Ready...";
                Cursor = Cursors.Default;
                Enabled = true;
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            ModFile.WaitCursor(this, true);
            LoadInitialData();

            lstInstalledMods.ClearSelected();
            lstUninstalledMods.ClearSelected();
            btnInstall.Enabled = false;
            btnRemove.Enabled = false;
            ModFile.WaitCursor(this, false);
        }

        private void BtnBrowseForMod_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Browse for a mod to install into your CoH path...",
                Filter = "Zip Files|*.zip"
            };
            if (ofd.ShowDialog() == DialogResult.Cancel || !System.IO.File.Exists(ofd.FileName)) return;
            if (!ModFile.ValidModFile(ofd.FileName))
            {
                MessageBox.Show("This Zip file is not a valid Mod file. Please choose a valid mod file and try again.", "Invalid File",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string destinationFile = modDirectory + Path.GetFileName(ofd.FileName);
            //Copy the file to the mod directory
            File.Copy(ofd.FileName, destinationFile);

            //Trigger a refresh, and the installation
            BtnRefresh_Click(this, new EventArgs());
            Application.DoEvents();
            ModFile m = new ModFile(ofd.FileName);
            lstUninstalledMods.SelectedValue = m.ModName;
            BtnInstall_Click(this, new EventArgs());
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            DataRow[] drs = ((DataTable)lstUninstalledMods.DataSource).Select("ModName = '" + lstUninstalledMods.SelectedValue + "'");
            ModFile mf = new ModFile(drs[0]["ModFileName"].ToString());
            string fileName = mf.ModFileName;
            if (File.Exists(fileName)) File.Delete(fileName);
            int checkAttempts = 0;
            do
            {
                System.Threading.Thread.Sleep(1000);
                checkAttempts++;
            } while (checkAttempts < 3 && File.Exists(fileName));

            if (File.Exists(fileName))
            {
                MessageBox.Show("Failed to delete local file '" + fileName + "', file may be currently in use. Please try again.",
                    "Failed to Delete Local Mod", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                RefreshData();
                MessageBox.Show("Deleted local file '" + fileName + "' successfully.", "Deleted Local Mod", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LstServerMods_SelectedValueChanged(object sender, EventArgs e)
        {
            txtServerMods.Text = "";
            btnDownloadServerMod.Enabled = false;
            btnInstallServerMod.Enabled = false;
            if (lstServerMods.SelectedValue != null)
            {
                //Find the correct datarow and display the description
                FTP_Client f = new FTP_Client(FTPServerName, FTPUserName, FTPPassword);
                string localFilename = Path.GetTempFileName();
                if (File.Exists(localFilename)) File.Delete(localFilename);
                f.GetFile(ProgressBar, AllModsInfo, localFilename);
                DataSet ds = new DataSet();
                ds.ReadXml(localFilename);
                if (File.Exists(localFilename)) File.Delete(localFilename);

                DataRow[] drs = ds.Tables[0].Select("ModName = '" + lstServerMods.SelectedValue + "'");
                txtServerMods.Text = "Mod Name: " + drs[0][nameof(ModFile.ModName)] + Environment.NewLine +
                    "Mod Category: " + (string.IsNullOrEmpty(drs[0][nameof(ModFile.ModCategory)].ToString()) ? "OTHER" :
                        drs[0][nameof(ModFile.ModCategory)].ToString()) + Environment.NewLine +
                    "Mod Author: " + drs[0][nameof(ModFile.ModAuthor)] + Environment.NewLine +
                    "Mod Version: " + drs[0][nameof(ModFile.ModVersion)] + Environment.NewLine +
                    "Mod Description: " + drs[0][nameof(ModFile.ModDescription)];
                btnDownloadServerMod.Enabled = true;
                btnInstallServerMod.Enabled = true;
            }
        }

        private void BtnInstallServerMod_Click(object sender, EventArgs e)
        {
            if (lstServerMods.SelectedValue == null) return;
            string ModName = lstServerMods.SelectedValue.ToString();
            DownloadMod();
            RefreshData();
            InstallMod(ModName);
        }

        private void BtnDownloadServerMod_Click(object sender, EventArgs e)
        {
            try
            {
                if (lstServerMods.SelectedValue == null) return;
                DownloadMod();
                RefreshData();
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            finally
            {
                Enabled = true;
                StatusLabel.Text = "Ready...";
            }
        }

        private void DownloadMod()
        {
            if (lstServerMods.SelectedValue == null) return;
            StatusLabel.Text = "Downloading Mod Now...";
            Enabled = false;

            DataRow[] drs = ((DataTable)lstServerMods.DataSource).Select("ModName = '" + lstServerMods.SelectedValue + "'");
            FTP_Client f = new FTP_Client(FTPServerName, FTPUserName, FTPPassword);
            string localFilename = modDirectory + Path.GetFileName(drs[0][nameof(ModFile.ModFileName)].ToString());
            if (File.Exists(localFilename)) File.Delete(localFilename);
            string remoteModPath = Path.GetFileName(drs[0][nameof(ModFile.ModFileName)].ToString());
            f.GetFile(ProgressBar, remoteModPath, localFilename);
        }

        private void BtnRegenerateServerModList_Click(object sender, EventArgs e)
        {
            ProcessModUpdate();
        }

        private void CboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void lstBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1) return;
            bool NewVersion = false;
            ListBox lb = (ListBox)sender;
            Brush BackGroundBrush = Brushes.White;
            Brush ForeGroundBrush = Brushes.Black;

            e.DrawBackground();
            Graphics g = e.Graphics;

            DataRowView drv = (DataRowView)lb.Items[e.Index];
            DataRow dr = drv.Row;

            if (!IsLatestVersion(dr))
            {
                BackGroundBrush = Brushes.Red;
                ForeGroundBrush = Brushes.White;
                NewVersion = true;
            }

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) BackGroundBrush = SystemBrushes.Highlight;

            g.FillRectangle(BackGroundBrush, e.Bounds);
            string textToWrite = dr["DisplayDescription"].ToString();
            if (NewVersion) textToWrite += " ***NEW VERSION AVAILABLE***";
            e.Graphics.DrawString(textToWrite, e.Font, ForeGroundBrush, e.Bounds, StringFormat.GenericDefault); ;
            e.DrawFocusRectangle();

        }

        private bool IsLatestVersion(DataRow dr)
        {
            try
            {
                dr = FixBadDR(dr);
                if (ServerMods == null || ServerMods.Rows.Count == 0) ServerMods = GetAllServerMods();

                string ModName = dr["ModName"].ToString();

                DataRow[] drs = ServerMods.Select("ModName = '" + ModName + "'");
                if (drs.Length > 0)
                {
                    //Get the highest version from the server list
                    int HighestVersion = 0;
                    foreach (DataRow drVersion in drs)
                    {
                        if (int.Parse(drVersion["ModVersion"].ToString()) > HighestVersion) HighestVersion = int.Parse(drVersion["ModVersion"].ToString());
                    }
                    int thisVersion = int.Parse(dr["ModVersion"].ToString());

                    //Return whether or not this is the highest version of this mod
                    return thisVersion >= HighestVersion;
                }
                else throw new Exception("Unable to locate mod '" + ModName + "' in Server Mod List!");
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;
            }
        }
    }
}
