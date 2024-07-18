using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoH_Modder
{
    public partial class CreateEditMod : Form
    {
        private readonly MainForm parent = null;
        private ModFile CurrentFile = null;

        private DataTable CreateFilesTable()
        {
            DataTable AddedFiles = new DataTable();
            AddedFiles.Columns.AddRange(new DataColumn[] {
                    new DataColumn("SourceFile"),
                    new DataColumn("Destination")
                });
            AddedFiles.PrimaryKey = new DataColumn[] { AddedFiles.Columns["SourceFile"], AddedFiles.Columns["Destination"] };
            return AddedFiles;
        }

        public CreateEditMod(MainForm Parent)
        {
            try
            {
                InitializeComponent();
                parent = Parent;
                dgvExistingFiles.DataSource = CreateFilesTable();
                Show();
                Top = parent.Top + 20;
                Left = parent.Left + 20;
                Hide();
            }
            catch (Exception ex)
            {
                MainForm.LogError(ex);
            }
        }

        public bool ModIsValid(bool ShowError)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                if (txtModAuthor.Text.Trim() == "") sb.AppendLine("Mod Author is blank.");
                if (cboModCategory.SelectedItem == null || 
                    cboModCategory.SelectedItem.ToString().Trim() == "") sb.AppendLine("Category is blank.");
                if (txtModName.Text.Trim() == "") sb.AppendLine("Mod Name is blank.");
                if (txtModDescription.Text.Trim() == "") sb.AppendLine("Mod Description is blank.");
                if (dgvExistingFiles.DataSource == null ||
                    ((DataTable)dgvExistingFiles.DataSource).Rows.Count == 0) sb.AppendLine("No Files Added to Mod.");

                if (sb.ToString().Trim().Length > 0)
                {
                    if (ShowError) MessageBox.Show("Invalid Mod. Please correct the errors shown below to proceed:" + Environment.NewLine + sb.ToString(),
                        "Invalid Mod", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                MainForm.LogError(ex);
                return false;
            }
        }

        private void BtnSaveMod_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ModIsValid(true)) return;
                SaveMod();
            }
            catch (Exception ex)
            {
                MainForm.LogError(ex);
            }
        }

        private void SaveMod(bool Upload = false)
        {
            try
            {
                if (CurrentFile == null)
                {
                    //Ask the user where to save the mod file at
                    SaveFileDialog sfd = new SaveFileDialog
                    {
                        Title = "Where do you wish to save this Mod to?",
                        Filter = "Zip Files|*.zip"
                    };
                    if (sfd.ShowDialog() == DialogResult.Cancel) return;
                    ModFile m = new ModFile
                    {
                        ModFileName = sfd.FileName,
                        ModAuthor = txtModAuthor.Text,
                        ModName = txtModName.Text,
                        ModCategory = cboModCategory.SelectedItem.ToString(),
                        ModDescription = txtModDescription.Text
                    };

                    foreach (DataRow dr in ((DataTable)dgvExistingFiles.DataSource).Rows)
                    {
                        m.InstallFiles.Add(dr["SourceFile"].ToString(), dr["Destination"].ToString());
                    }
                    CurrentFile = m;
                }
                else
                {
                    //Make sure to wipe the old data and update it
                    CurrentFile.ModName = txtModName.Text;
                    CurrentFile.ModCategory = cboModCategory.SelectedItem.ToString();
                    CurrentFile.ModAuthor = txtModAuthor.Text;
                    CurrentFile.ModDescription = txtModDescription.Text;
                    CurrentFile.InstallFiles = new Dictionary<string, string>();
                    foreach (DataRow dr in ((DataTable)dgvExistingFiles.DataSource).Rows)
                    {
                        CurrentFile.InstallFiles.Add(dr["SourceFile"].ToString(), dr["Destination"].ToString());
                    }
                }
                statusLabel.Text = "Zipping file, please wait...";
                Application.DoEvents();
                CurrentFile.Save();

                if (Upload)
                {
                    statusLabel.Text = "Uploading file...";
                    Application.DoEvents();
                    //Upload the actual mod file
                    FTP_Client f = new FTP_Client(MainForm.FTPServerName, MainForm.FTPUserName, MainForm.FTPPassword);
                    if (f.FileExists(Path.GetFileName(CurrentFile.ModFileName))) f.DeleteFile(Path.GetFileName(CurrentFile.ModFileName));
                    f.UploadFile(CurrentFile.ModFileName, Path.GetFileName(CurrentFile.ModFileName), progressBar);

                    //Update the master mod list with this new mod
                    if (!f.FileExists(parent.AllModsInfo)) throw new Exception("All Mods Info file appears to be missing!" +
                        "Please either click the 'Fresh Start' button on the main form, or report this to the developer.");

                    f.GetFile(parent.ProgressBar, parent.AllModsInfo, parent.modDirectory + parent.AllModsInfo);
                    DataSet ds = new DataSet();
                    ds.ReadXml(parent.modDirectory + parent.AllModsInfo);
                    DataTable dt = ds.Tables[0];
                    DataRow[] drs = dt.Select("ModName = '" + CurrentFile.ModName + "'");
                    if (drs.Length > 0)
                    {
                        //Update the existing record
                        drs[0]["ModName"] = CurrentFile.ModName;
                        drs[0]["ModCategory"] = CurrentFile.ModCategory;
                        drs[0]["ModAuthor"] = CurrentFile.ModAuthor;
                        drs[0]["ModDescription"] = CurrentFile.ModDescription;
                        drs[0]["ModVersion"] = CurrentFile.ModVersion;
                        drs[0]["ModFileName"] = CurrentFile.ModFileName;
                    }
                    else
                    {
                        //Add the new record
                        dt.Rows.Add(CurrentFile.ModName, CurrentFile.ModCategory, CurrentFile.ModAuthor, CurrentFile.ModDescription,
                            CurrentFile.ModVersion, CurrentFile.ModFileName);
                    }
                    File.Delete(parent.modDirectory + parent.AllModsInfo);
                    ds.WriteXml(parent.modDirectory + parent.AllModsInfo);
                    f.UploadFile(parent.modDirectory + parent.AllModsInfo, parent.AllModsInfo, progressBar);
                    File.Delete(parent.modDirectory + parent.AllModsInfo);
                }

                parent.ServerMods = parent.GetAllServerMods();

                lblVersion.Text = "Version " + CurrentFile.ModVersion;
                MessageBox.Show("Mod '" + CurrentFile.ModName + "' version " + CurrentFile.ModVersion + " has been saved!",
                    "Mod Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CurrentFile = null; //Prevent accidental saves over different mods
            }
            catch (Exception ex)
            {
                MainForm.LogError(ex);
            }
            finally
            {
                statusLabel.Text = "Ready...";
            }
        }

        private void TxtModAuthor_Validated(object sender, EventArgs e)
        {
            btnSaveMod.Enabled = ModIsValid(false);
            btnSaveAndUploadMod.Enabled = ModIsValid(false);
        }

        private void TxtModName_Validated(object sender, EventArgs e)
        {
            txtModName.Text = txtModName.Text.Replace("'", ""); //Don't allow single quotes as that messes with querying
            btnSaveMod.Enabled = ModIsValid(false);
            btnSaveAndUploadMod.Enabled = ModIsValid(false);
        }

        private void TxtModDescription_Validated(object sender, EventArgs e)
        {
            btnSaveMod.Enabled = ModIsValid(false);
            btnSaveAndUploadMod.Enabled = ModIsValid(false);
        }

        private void BtnAddFile_Click(object sender, EventArgs e)
        {
            try
            {
                //Check if file already exists, if so, edit the existing record
                DataRow[] drs = ((DataTable)dgvExistingFiles.DataSource).Select("SourceFile = '" + txtSourceFile.Text + "'");
                if (drs.Length > 0)
                {
                    drs[0]["Destination"] = txtDestination.Text;
                    drs[0].Table.AcceptChanges();
                }
                else
                {
                    //Otherwise, add the new record
                    ((DataTable)dgvExistingFiles.DataSource).Rows.Add(txtSourceFile.Text, txtDestination.Text);
                }

                btnSaveMod.Enabled = ModIsValid(false);
                btnSaveAndUploadMod.Enabled = ModIsValid(false);
            }
            catch (Exception ex)
            {
                MainForm.LogError(ex);
            }
        }

        private void BtnRemoveFile_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (DataGridViewRow dgvr in dgvExistingFiles.SelectedRows)
                {
                    DataRow dr = ((DataRowView)dgvr.DataBoundItem).Row;
                    DataRow[] drs = ((DataTable)dgvExistingFiles.DataSource).Select("SourceFile = '" + dr["SourceFile"] +
                        "' AND Destination = '" + dr["Destination"] + "'");

                    foreach (DataRow drToRemove in drs)
                    {
                        drToRemove.Delete();
                    }
                }
                btnSaveMod.Enabled = ModIsValid(false);
                btnSaveAndUploadMod.Enabled = ModIsValid(false);
            }
            catch (Exception ex)
            {
                MainForm.LogError(ex);
            }
        }

        private void BtnChooseSourceFile_Click(object sender, EventArgs e)
        {
            try
            {
                txtSourceFile.Text = "";
                OpenFileDialog ofd = new OpenFileDialog
                {
                    Title = "Choose a file to add to the mod...",
                    Filter = "All Files|*.*"
                };
                if (ofd.ShowDialog() == DialogResult.Cancel || !System.IO.File.Exists(ofd.FileName)) return;
                txtSourceFile.Text = ofd.FileName;

                btnAddEditFile.Enabled = txtSourceFile.Text.Trim() != "" && txtDestination.Text.Trim() != "";
            }
            catch (Exception ex)
            {
                MainForm.LogError(ex);
            }
        }

        private void BtnChooseDestination_Click(object sender, EventArgs e)
        {
            try
            {
                txtDestination.Text = "";
                FolderBrowserDialog fbd = new FolderBrowserDialog
                {
                    SelectedPath = parent.txtCoHRootDirectory.Text
                };
                if (fbd.ShowDialog() == DialogResult.Cancel || !System.IO.Directory.Exists(fbd.SelectedPath)) return;
                if (!fbd.SelectedPath.StartsWith(parent.txtCoHRootDirectory.Text))
                {
                    MessageBox.Show("Invalid location. Location must exist within the City of Heroes root directory chosen on the main form.",
                        "Invalid Location", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string FinalDestination = fbd.SelectedPath.Replace(parent.txtCoHRootDirectory.Text, "");
                txtDestination.Text = FinalDestination.EndsWith(@"\") ? FinalDestination : FinalDestination + @"\";

                btnAddEditFile.Enabled = txtSourceFile.Text.Trim() != "" && txtDestination.Text.Trim() != "";
            }
            catch (Exception ex)
            {
                MainForm.LogError(ex);
            }
        }

        private void BtnLoadMod_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog
                {
                    Title = "Please choose the mod file to open..."
                };
                if (ofd.ShowDialog() == DialogResult.Cancel || !System.IO.File.Exists(ofd.FileName)) return;
                if (!ModFile.ValidModFile(ofd.FileName))
                {
                    MessageBox.Show("This Zip file is not a valid Mod file. Please choose a valid mod file and try again.", "Invalid File",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                CurrentFile = new ModFile(ofd.FileName);
                txtModAuthor.Text = CurrentFile.ModAuthor;
                txtModName.Text = CurrentFile.ModName;
                txtModDescription.Text = CurrentFile.ModDescription;

                DataTable AddedFiles = CreateFilesTable();
                foreach (KeyValuePair<string, string> kvp in CurrentFile.InstallFiles)
                {
                    AddedFiles.Rows.Add(kvp.Key, kvp.Value);
                }
                dgvExistingFiles.DataSource = AddedFiles;
                lblVersion.Text = "Version " + CurrentFile.ModVersion;
                btnSaveMod.Enabled = ModIsValid(false);
                btnSaveAndUploadMod.Enabled = ModIsValid(false);
            }
            catch (Exception ex)
            {
                MainForm.LogError(ex);
            }
        }

        private void DgvExistingFiles_SelectionChanged(object sender, EventArgs e)
        {
            txtSourceFile.Text = "";
            txtDestination.Text = "";
            btnRemoveFile.Enabled = false;
            btnAddEditFile.Enabled = false;
            if (dgvExistingFiles.SelectedRows.Count == 1)
            {
                txtSourceFile.Text = dgvExistingFiles.SelectedRows[0].Cells["SourceFile"].Value.ToString();
                txtDestination.Text = dgvExistingFiles.SelectedRows[0].Cells["Destination"].Value.ToString();
                btnRemoveFile.Enabled = true;
                btnAddEditFile.Enabled = true;
            }
            else if (dgvExistingFiles.SelectedRows.Count > 0)
            {
                btnRemoveFile.Enabled = true;
            }
        }

        private void BtnAddDataFolder_Click(object sender, EventArgs e)
        {
            try
            {
                string dataDirectory = parent.txtCoHRootDirectory.Text + @"data\";
                //Get the list of all files in the data directory
                if (!Directory.Exists(dataDirectory))
                {
                    MessageBox.Show("No Data Directory found in location '" + dataDirectory + "'", "No Data Directory",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string[] allFiles = Directory.GetFiles(dataDirectory, "*", SearchOption.AllDirectories);
                if (allFiles.Length == 0)
                {
                    MessageBox.Show("No files found to add in the data directory at '" + dataDirectory + "'", "No Files Found",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                ModFile.WaitCursor(this, true);
                DataTable AddedFiles = (DataTable)dgvExistingFiles.DataSource;
                if (AddedFiles == null) AddedFiles = CreateFilesTable();

                foreach (string sourceFile in allFiles)
                {
                    string destination = sourceFile.Replace(parent.txtCoHRootDirectory.Text, "");
                    //Check if file already exists, if so, edit the existing record
                    DataRow[] drs = ((DataTable)dgvExistingFiles.DataSource).Select("SourceFile = '" + sourceFile + "'");
                    if (drs.Length > 0)
                    {
                        drs[0]["Destination"] = destination;
                        drs[0].Table.AcceptChanges();
                    }
                    else
                    {
                        //Otherwise, add the new record
                        ((DataTable)dgvExistingFiles.DataSource).Rows.Add(sourceFile, destination);
                    }

                }
                dgvExistingFiles.DataSource = AddedFiles;
            }
            catch (Exception ex)
            {
                MainForm.LogError(ex);
            }
            finally
            {
                btnSaveMod.Enabled = ModIsValid(false);
                btnSaveAndUploadMod.Enabled = ModIsValid(false);
                ModFile.WaitCursor(this, false);
            }
        }

        private void BtnSaveAndUploadMod_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ModIsValid(true)) return;
                SaveMod(true);
            }
            catch (Exception ex)
            {
                MainForm.LogError(ex);
            }
        }

        private void CboModCategory_SelectedValueChanged(object sender, EventArgs e)
        {
            btnSaveMod.Enabled = ModIsValid(false);
            btnSaveAndUploadMod.Enabled = ModIsValid(false);
        }
    }
}
