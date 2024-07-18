using Philotic_Knight;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Def_File_Translator
{
    public partial class MainForm : Form
    {
#pragma warning disable IDE0069 // Disposable fields should be disposed
        //Disposed at form closing, don't know why that isn't good enough for Visual Studio, so suppressing
        DataSet CurrentDataSet = null;
        DataSet PowerSets = null;
#pragma warning restore IDE0069 // Disposable fields should be disposed

        public MainForm()
        {
            InitializeComponent();
        }

        private void BtnReadDefFile_Click(object sender, EventArgs e)
        {
            try
            {
                ClearForm();
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = "Choose the file to open";
                    if (ofd.ShowDialog() == DialogResult.Cancel || ofd.FileName == "" || !File.Exists(ofd.FileName)) return;

                    Cursor = Cursors.WaitCursor;

                    //Get the DefObjects from the file
                    if (Path.GetExtension(ofd.FileName).ToUpper() == ".POWERS")
                    {
                        //Load all powersets in the same directory, if we're dealing with powers
                        string[] powersets = Directory.GetFiles(Path.GetDirectoryName(ofd.FileName), "*.powersets");
                        List<DefObject> AllPowerSets = new List<DefObject>();
                        foreach (string powerset in powersets)
                        {
                            List<DefObject> ThisPowerset = DefMethods.GetDefObjectsFromDefFile(powerset);
                            AllPowerSets.AddRange(ThisPowerset);
                        }

                        PowerSets = DefMethods.GetDataSetFromDefObjects(AllPowerSets);
                    }
                    List<DefObject> dObjects = DefMethods.GetDefObjectsFromDefFile(ofd.FileName);

                    //Export these objects into a DataSet
                    if (dObjects == null || dObjects.Count == 0)
                    {
                        MessageBox.Show("No Def Objects found in file.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    DataSet ds = DefMethods.GetDataSetFromDefObjects(dObjects);
                    LoadData(ds);
                    Text = "PK's Def File Translator - " + ofd.FileName;
                }
            }
            catch (Exception ex)
            {
                StandardError(ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void BtnReadExcelFile_Click(object sender, EventArgs e)
        {
            try
            {
                ClearForm();
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = "Choose the file to open";
                    ofd.Filter = "Excel Files|*.xls";
                    if (ofd.ShowDialog() == DialogResult.Cancel || ofd.FileName == "" || !File.Exists(ofd.FileName)) return;

                    Cursor = Cursors.WaitCursor;

                    //Get the DefObjects from the file
                    List<DefObject> dObjects = DefMethods.GetDefObjectsFromExcelFile(ofd.FileName);

                    //Export these objects into a DataSet
                    if (dObjects == null || dObjects.Count == 0)
                    {
                        MessageBox.Show("No Def Objects found in file.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    DataSet ds = DefMethods.GetDataSetFromDefObjects(dObjects);
                    LoadData(ds);
                    Text = "PK's Def File Translator - " + ofd.FileName;
                }
            }
            catch (Exception ex)
            {
                StandardError(ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void CboObjects_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                dgvMain.DataSource = null;
                dgvMain.Enabled = false;
                if (cboObjects.SelectedValue == null || cboObjects.SelectedValue.ToString() == "" ||
                    CurrentDataSet == null || CurrentDataSet.Tables.Count == 0) return;

                foreach (DataTable dt in CurrentDataSet.Tables)
                {
                    if (dt.Rows[0]["ObjectName"].ToString() == cboObjects.SelectedValue.ToString())
                    {
                        dgvMain.DataSource = dt;
                        dgvMain.Enabled = true;
                        dgvMain.Columns["ObjectName"].Visible = false;
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                StandardError(ex);
            }
        }

        private void BtnWriteDefFile_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurrentDataSet == null)
                {
                    MessageBox.Show("No Dataset found to write to file.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Title = "Choose where to save the file to";
                    sfd.Filter = "Def Files|*.def|All Files|*.*";
                    if (sfd.ShowDialog() == DialogResult.Cancel || sfd.FileName == "") return;

                    Cursor = Cursors.WaitCursor;
                    DefMethods.WriteDataSetToDefFile(CurrentDataSet, sfd.FileName);
                }
            }
            catch (Exception ex)
            {
                StandardError(ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void BtnWriteExcelFile_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurrentDataSet == null)
                {
                    MessageBox.Show("No Dataset found to write to file.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Title = "Choose where to save the file to";
                    sfd.Filter = "Excel Files|*.xls|All Files|*.*";
                    if (sfd.ShowDialog() == DialogResult.Cancel || sfd.FileName == "") return;

                    Cursor = Cursors.WaitCursor;
                    DefMethods.WriteDataSetToExcelFile(CurrentDataSet, sfd.FileName);
                }
            }
            catch (Exception ex)
            {
                StandardError(ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ClearForm()
        {
            try
            {
                Text = "PK's Def File Translator";
                cboObjects.Enabled = false;
                cboObjects.DataSource = null;
                dgvMain.DataSource = null;
                btnWriteDefFile.Enabled = false;
                btnWriteExcelFile.Enabled = false;
            }
            catch (Exception ex)
            {
                StandardError(ex);
            }
        }

        /// <summary>
        /// Load data into the form
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        private bool LoadData(DataSet ds)
        {
            try
            {
                if (ds.Tables.Count == 0) return false;
                DataTable dt = new DataTable();
                dt.Columns.Add("ObjectName");
                dt.Rows.Add("");
                foreach (DataTable dtSource in ds.Tables)
                {
                    dt.Rows.Add(dtSource.Rows[0]["ObjectName"].ToString());
                }
                cboObjects.ValueMember = "ObjectName";
                cboObjects.DisplayMember = "ObjectName";
                cboObjects.DataSource = dt;
                cboObjects.Enabled = true;
                CurrentDataSet = ds;
                btnWriteDefFile.Enabled = true;
                btnWriteExcelFile.Enabled = true;
                return true;
            }
            catch (Exception ex)
            {
                StandardError(ex);
                return false;
            }
        }

        /// <summary>
        /// Present a standard error message to the user
        /// </summary>
        /// <param name="ex"></param>
        private void StandardError(Exception ex)
        {
            MessageBox.Show("Program Error: " + ex.Message +
                ", please send a screenshot of this error along with the offending file (if applicable) to The Philotic Knight." +
                Environment.NewLine + Environment.NewLine + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CurrentDataSet != null) CurrentDataSet.Dispose();
        }
    }
}