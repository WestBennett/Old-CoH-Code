using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowersChecker
{
    public partial class MainForm : Form
    {

        bool DataLoaded = false;

        DataSet MainData = null;
        public MainForm()
        {
            InitializeComponent();
            Show();
            Cursor = Cursors.WaitCursor;

            string FileName = Path.GetTempFileName();
            File.WriteAllBytes(FileName, PKsPowersChecker.Properties.Resources.PlayerPowers);
            MainData = GetDataSetFromExcel(FileName, true);
            try
            {
                File.Delete(FileName);
            }
            catch
            {
                //Don't really care if we fail to delete
            }

            //MainData = GetDataSetFromExcel(PKsPowersChecker.Properties.Resources.PlayerPowers);

            DataView view = new DataView(MainData.Tables["Powers"]);
            DataTable distinctValues = view.ToTable(true, "Entity");
            distinctValues.DefaultView.Sort = "Entity";
            distinctValues = distinctValues.DefaultView.ToTable();
            DataRow BlankRow = distinctValues.NewRow();
            BlankRow["Entity"] = "";
            distinctValues.Rows.InsertAt(BlankRow, 0);
            cboEntity.DataSource = distinctValues;
            DataLoaded = true;
            Cursor = Cursors.Default;
        }

        public DataSet GetDataSetFromExcel(byte[] file, bool hasHeader = true)
        {
            DataSet ds = new DataSet();
            using (var pck = new OfficeOpenXml.ExcelPackage())
            {

                using (var stream = new MemoryStream(file))
                {
                    pck.Load(stream);
                }

                foreach (ExcelWorksheet ws in pck.Workbook.Worksheets)
                {
                    DataTable tbl = new DataTable(ws.Name);
                    foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                    {
                        tbl.Columns.Add(hasHeader ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));
                    }
                    var startRow = hasHeader ? 2 : 1;

                    ProgBar.Minimum = 1;
                    ProgBar.Maximum = ws.Dimension.End.Row;
                    for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                    {
                        ProgBar.Value = rowNum;
                        StatusLabel.Text = "Adding Row " + rowNum.ToString() + " of " + ws.Dimension.End.Row.ToString() + " to DataSet from Sheet '" + ws.Name + "'";
                        this.Refresh();
                        Application.DoEvents();
                        var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                        DataRow row = tbl.Rows.Add();
                        foreach (var cell in wsRow)
                        {
                            row[cell.Start.Column - 1] = cell.Text;
                        }
                    }
                    ds.Tables.Add(tbl);
                }
            }

            ProgBar.Visible = false;
            StatusLabel.Text = "Ready";
            return ds;
        }

        public DataSet GetDataSetFromExcel(string FileName, bool containsHeaderRow = false)
        {
            string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FileName + ";Extended Properties=\"Excel 12.0;HDR=No;IMEX=1\""; ;
            DataSet dsReturn = new DataSet();

            using (OleDbConnection excelConnection = new OleDbConnection(connectionString))
            {
                excelConnection.Open();

                DataTable tableList = excelConnection.GetSchema("Tables");

                if (tableList.Rows.Count == 0) return null;

                foreach (DataRow dr in tableList.Rows)
                {
                    string sheetName = dr["TABLE_NAME"].ToString();
                    using (OleDbCommand excelCommand = excelConnection.CreateCommand())
                    {
                        excelCommand.CommandText = "Select * From [" + sheetName + "]";
                        excelCommand.CommandType = CommandType.Text;

                        DataTable dt = new DataTable();
                        using (OleDbDataReader oleExcelReader = excelCommand.ExecuteReader())
                        {
                            dt.Load(oleExcelReader);
                            dt.TableName = sheetName.Replace("$", "");

                            //Take the first column as the header column, if applicable
                            if (containsHeaderRow)
                            {
                                foreach (DataColumn dc in dt.Columns)
                                {
                                    dc.ColumnName = dt.Rows[0][dc.ColumnName].ToString();
                                }
                                dt.Rows.Remove(dt.Rows[0]);
                            }

                            dsReturn.Tables.Add(dt);
                        }
                    }

                }
            }
            return dsReturn;
        }


        private void GetAttributes(string EntityName, string PowerSet = null, string PowerName = null, string AttributeModName = null)
        {
            Cursor = Cursors.WaitCursor;
            string Query = "Entity = '" + EntityName + "'";
            if (PowerSet != null  && PowerSet != "") Query += " AND PowerSet = '" + PowerSet + "'";
            if (PowerName != null && PowerName != "") Query += " AND PowerName = '" + PowerName + "'";

            //Populate the Main Attributes and AttributeMod DGVs
            DataTable MainAttributes = MainData.Tables["Powers"].Select(Query).CopyToDataTable<DataRow>();
            DataLoaded = false;
            MainAttributes.TableName = nameof(MainAttributes);
            dgvAttributes.DataSource = MainAttributes;
            dgvAttributes.ClearSelection();

            if (AttributeModName != null) Query += " AND AttribModName = '" + AttributeModName + "'";
            DataTable AttributeMods = MainData.Tables["AttribMods"].Select(Query).CopyToDataTable<DataRow>();
            AttributeMods.TableName = nameof(AttributeMods);
            dgvAttributeMods.DataSource = AttributeMods;
            dgvAttributeMods.ClearSelection();
            DataLoaded = true;
            Cursor = Cursors.Default;
        }

        private void CboEntity_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!DataLoaded) return;
            if (cboEntity.SelectedValue == null || cboEntity.SelectedValue.ToString() == "")
            {
                cboPowerSet.DataSource = null;
                cboPowerSet.Enabled = false;
                cboPowerName.DataSource = null;
                cboPowerName.Enabled = false;
                dgvAttributes.DataSource = null;
                dgvAttributeMods.DataSource = null;

                return;
            }
            Cursor = Cursors.WaitCursor;
            DataLoaded = false;
            DataTable newValues = MainData.Tables["Powers"].Select("Entity = '" + cboEntity.SelectedValue + "'").CopyToDataTable<DataRow>();
            DataView view = new DataView(newValues);
            DataTable distinctValues = view.ToTable(true, "PowerSet");
            distinctValues.DefaultView.Sort = "PowerSet";
            distinctValues = distinctValues.DefaultView.ToTable();
            DataRow BlankRow = distinctValues.NewRow();
            BlankRow["PowerSet"] = "";
            distinctValues.Rows.InsertAt(BlankRow, 0);
            cboPowerSet.DataSource = distinctValues;
            cboPowerSet.Enabled = true;

            DataLoaded = true;
            GetAttributes(cboEntity.SelectedValue.ToString());
            Cursor = Cursors.Default;
        }

        private void CboPowerSet_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!DataLoaded) return;
            if (cboPowerSet.SelectedValue == null || cboPowerSet.SelectedValue.ToString() == "")
            {
                cboPowerName.DataSource = null;
                cboPowerName.Enabled = false;
                dgvAttributes.DataSource = null;
                dgvAttributeMods.DataSource = null;
                return;
            }

            Cursor = Cursors.WaitCursor;
            DataLoaded = false;
            DataTable newValues = MainData.Tables["Powers"].Select("Entity = '" + cboEntity.SelectedValue + "'" +
                " AND PowerSet = '" + cboPowerSet.SelectedValue + "'").CopyToDataTable<DataRow>();
            DataView view = new DataView(newValues);
            DataTable distinctValues = view.ToTable(true, "PowerName");
            distinctValues.DefaultView.Sort = "PowerName";
            distinctValues = distinctValues.DefaultView.ToTable();
            DataRow BlankRow = distinctValues.NewRow();
            BlankRow["PowerName"] = "";
            distinctValues.Rows.InsertAt(BlankRow, 0);
            cboPowerName.DataSource = distinctValues;
            cboPowerName.Enabled = true;

            DataLoaded = true;
            GetAttributes(cboEntity.SelectedValue.ToString(), cboPowerSet.SelectedValue.ToString());
            Cursor = Cursors.Default;
        }

        private void CboPowerName_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!DataLoaded) return;
            if (cboPowerSet.SelectedValue == null || cboPowerSet.SelectedValue.ToString() == "")
            {
                dgvAttributes.DataSource = null;
                dgvAttributeMods.DataSource = null;
                return;
            }

            GetAttributes(cboEntity.SelectedValue.ToString(), cboPowerSet.SelectedValue.ToString(), cboPowerName.SelectedValue.ToString());
        }

        private void ExportDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridView dgv = (DataGridView)((ContextMenuStrip)(((ToolStripMenuItem)sender).Owner)).SourceControl;
            if (dgv.DataSource == null || ((DataTable)dgv.DataSource).Rows.Count == 0)
            {
                MessageBox.Show("No data found to export!", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Where to save the file to?";
            sfd.Filter = "Excel Files|*.xlsx";
            if (sfd.ShowDialog() != DialogResult.OK) return;

            if (File.Exists(sfd.FileName)) File.Delete(sfd.FileName);

            using (ExcelPackage pck = new ExcelPackage(new FileInfo(sfd.FileName)))
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add(((DataTable)dgv.DataSource).TableName);
                ws.Cells["A1"].LoadFromDataTable((DataTable)dgv.DataSource, true);
                pck.Save();
            }
            MessageBox.Show("File '" + sfd.FileName + "' exported successfully!", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
