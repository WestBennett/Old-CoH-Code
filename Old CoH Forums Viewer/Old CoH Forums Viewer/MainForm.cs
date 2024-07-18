using ClosedXML.Excel;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Old_CoH_Forums_Viewer
{
    public partial class MainForm : Form
    {

        public const string ConnectionString = @"";

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DataTable SearchOptions = new DataTable();
            SearchOptions.Columns.AddRange(new DataColumn[] {
                new DataColumn("SearchOptionValue"),
                new DataColumn("SearchOptionDescription"),
                new DataColumn("SQLToRun"),
                new DataColumn("VariableName")
            });
            SearchOptions.Rows.Add("", "");

            //Page Search Options
            SearchOptions.Rows.Add("- Page Search Options -", "- Page Search Options -");
            SearchOptions.Rows.Add("PageByURL", "Page By URL", "SELECT * FROM Pages WHERE URL = '@URL'", "@URL");

            //Post Search Options
            SearchOptions.Rows.Add("- Post Search Options -", "- Post Search Options -");
            SearchOptions.Rows.Add("PostsByID", "Posts By ID",
                @"SELECT u.UserName, p.*
                FROM Posts p
                JOIN Users u ON u.UserID = p.UserID
                WHERE p.PostID = '@PostID'",
                "@PostID");
            SearchOptions.Rows.Add("PostsByUserName", "Posts By UserName",
                @"SELECT u.UserName, p.*
                FROM Posts p
                JOIN Users u ON u.UserID = p.UserID
                AND UPPER(u.UserName) LIKE '%@UserName%'",
                "@UserName");

            //Thread Search Options
            SearchOptions.Rows.Add("- Thread Search Options -", "- Thread Search Options -");
            SearchOptions.Rows.Add("ThreadByID", "Thread By ID", "SELECT * FROM Threads WHERE ThreadID = '@ThreadID'", "@ThreadID");
            SearchOptions.Rows.Add("ThreadByExactTitle", "Thread By EXACT Title", "SELECT * FROM Threads t WHERE t.Title = '@Title';", "@Title");
            SearchOptions.Rows.Add("ThreadByPartialTitle", "Thread By Partial Title", "SELECT * FROM Threads t WHERE UPPER(t.Title) LIKE UPPER('%@Title%');", "@Title");

            //User Search Options
            SearchOptions.Rows.Add("- User Search Options -", "- User Search Options -");
            SearchOptions.Rows.Add("UserByID", "User By ID", "SELECT * FROM Users WHERE UserID = '@UserID'", "@UserID");
            SearchOptions.Rows.Add("UserByName", "User By Name", "SELECT * FROM Users WHERE UPPER(UserName) LIKE CONCAT('%', UPPER('@UserName'), '%')", "@UserName");

            //Beta search options
            SearchOptions.Rows.Add("- BETA Search Options -", "- BETA Search Options -");
            SearchOptions.Rows.Add("BETAThreadsByUserID", "ThreadsByUserID",
            @"SELECT u.UserName, u.UserID, t.ThreadID, t.Title, t.ContentHTML
                FROM users u
                INNER JOIN userinthread ut ON ut.UserID = u.UserID
                INNER JOIN threads t ON t.ThreadID = ut.ThreadID
                WHERE u.UserID = @UserID",
            "@UserID");
            SearchOptions.Rows.Add("BETAThreadsWithExactUserName", "Threads With Exact UserName",
            @"SELECT u.UserName, u.UserID, t.Title, t.ContentHTML
                FROM users u
                INNER JOIN userinthread ut ON ut.UserID = u.UserID
                INNER JOIN threads t ON t.ThreadID = ut.ThreadID
                WHERE UPPER(u.UserName) = '@UserName'",
            "@UserName");
            SearchOptions.Rows.Add("BETAThreadsWithPartialUserName", "Threads With Partial UserName",
            @"SELECT u.UserName, u.UserID, t.ThreadID, t.Title, t.ContentHTML
                FROM users u
                INNER JOIN userinthread ut ON ut.UserID = u.UserID
                INNER JOIN threads t ON t.ThreadID = ut.ThreadID
                WHERE UPPER(u.UserName) LIKE '%@UserName%'", 
            "@UserName");

            cboSearchType.ValueMember = "SearchOptionValue";
            cboSearchType.DisplayMember = "SearchOptionDescription";
            cboSearchType.DataSource = SearchOptions;
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            int test = 0;
            if (cboSearchType.Text.EndsWith("ID") && !int.TryParse(txtSearchValue.Text, out test))
            {
                MessageBox.Show("Can only use integers with an ID search! Please try again.", "Bad Input", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            GetData();
        }

        private void CboSearchType_SelectedValueChanged(object sender, EventArgs e)
        {
            lblResults.Visible = false;
            btnSearch.Enabled = (cboSearchType.Text == "") ? false : (cboSearchType.Text.StartsWith("-")) ? false : true;
            txtSearchValue.Enabled = (cboSearchType.Text.StartsWith("All") ? false : true);
            dgvMain.DataSource = null;
        }

        private void DgvMain_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string ColName = dgvMain.Columns[e.ColumnIndex].Name;
            if (!ColName.StartsWith("View")) return;

            switch (ColName)
            {
                case "View HTML":
                    string Content = dgvMain["ContentHTML", e.RowIndex].Value.ToString();
                    string FileName = Path.GetTempFileName().Replace(".tmp", ".html");
                    File.WriteAllText(FileName, Content);
                    do
                    {
                        Thread.Sleep(100);
                    } while (!File.Exists(FileName));
                    Process prc = Process.Start(FileName);
                    break;
                case "View Wayback":
                    Process.Start("https://web.archive.org/web/*/" + dgvMain["URL", e.RowIndex].Value.ToString());
                    break;
                default:
                    return;
            }
        }

        public void GetData()
        {
            Cursor = Cursors.WaitCursor;
            try
            {

                using (MySqlConnection s = new MySqlConnection(ConnectionString))
                {
                    lblResults.Text = "";
                    DataRow dr = ((DataTable)cboSearchType.DataSource).Select("SearchOptionValue = '" + cboSearchType.SelectedValue + "'")[0];
                    string Command = "";
                    if (dr["VariableName"].ToString() == "")
                    {
                        Command = dr["SQLToRun"].ToString();
                    }
                    else
                    {
                        Command = dr["SQLToRun"].ToString().Replace(dr["VariableName"].ToString(), txtSearchValue.Text);
                    }

                    using (MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(Command, s))
                    {
                        DataTable dtResult = new DataTable();
                        try
                        {
                            myDataAdapter.Fill(dtResult);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.ToUpper().Contains("TIMEOUT EXPIRED"))
                            {
                                MessageBox.Show("Maximium timeout reached. This is a very long query with massive amounts of data!" +
                                    "This query will need to be looked at, and either made more efficient, " +
                                    "or made more specific to not timeout, or removed from the system entirely. " +
                                    "Please report this error message with the name of the query you chose, so it can be investigated.",
                                    "Timed Out", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                return;
                            }
                            else
                            {
                                throw new Exception(ex.Message, ex);
                            }
                        }

                        lblResults.Text = dtResult.Rows.Count + " result(s)";
                        lblResults.Visible = true;

                        if (dgvMain.Columns.Contains("View HTML")) dgvMain.Columns.Remove("View HTML");

                        if (dtResult.Columns.Contains("ContentHTML") && !dgvMain.Columns.Contains("View HTML"))
                        {
                            DataGridViewButtonColumn dgvbc = new DataGridViewButtonColumn();
                            dgvbc.Text = "HTML";
                            dgvbc.Name = "View HTML";
                            dgvbc.UseColumnTextForButtonValue = true;
                            dgvMain.Columns.Add(dgvbc);
                        }

                        if (dgvMain.Columns.Contains("View Wayback")) dgvMain.Columns.Remove("View Wayback");
                        if (dtResult.Columns.Contains("URL"))
                        {
                            DataGridViewButtonColumn dgvbc = new DataGridViewButtonColumn();
                            dgvbc.Text = "Wayback";
                            dgvbc.Name = "View Wayback";
                            dgvbc.UseColumnTextForButtonValue = true;
                            dgvMain.Columns.Add(dgvbc);
                            //https://web.archive.org/web/*/http://boards.cityofheroes.com
                        }

                        dgvMain.DataSource = dtResult;
                        if (dgvMain.Columns.Contains("ContentHTML")) dgvMain.Columns["ContentHTML"].Visible = false;
                        if (dgvMain.Columns.Contains("URL") && !cboSearchType.SelectedValue.ToString().StartsWith("All")) dgvMain.Columns["URL"].Visible = false;
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error obtaining data.", ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ContributorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Contributors c = new Contributors(this);
            c.ShowDialog();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Choose the folder where you have the uncompressed .warc files at";
            if (fbd.ShowDialog() == DialogResult.Cancel) return;
            if (!Directory.Exists(fbd.SelectedPath)) return;
            List<string> filesToProcess = Directory.GetFiles(fbd.SelectedPath, "*.warc").ToList();
            string connectionString = ";";

            lblProcessing.Visible = true;
            int iIFileProcessed = 0;
            foreach (string file in filesToProcess)
            {
                iIFileProcessed++;
                lblProcessing.Text = "Now processing file # " + iIFileProcessed + " of " + filesToProcess.Count;
                Application.DoEvents();
                using (MySqlConnection s = new MySqlConnection(connectionString))
                {
                    string cmd = "INSERT INTO filestoprocess (FileName) VALUES('@FILE')".Replace("@FILE", Path.GetFileNameWithoutExtension(file));
                    using (MySqlCommand c = new MySqlCommand(cmd, s))
                    {
                        s.Open();
                        try
                        {
                            c.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            //Dont' care
                        }
                    }
                }
            }
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(dgvMain.SelectedCells[0].Value.ToString());
        }

        private void CmsGrid_Opening(object sender, CancelEventArgs e)
        {
            copyToolStripMenuItem.Enabled = dgvMain.SelectedCells.Count == 1 ? true : false;
            exportToolStripMenuItem.Enabled = dgvMain.Rows.Count > 0 ? true : false;
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save Excel file to where?";
            sfd.Filter = "Excel Files|*.xlsx";
            if (sfd.ShowDialog() == DialogResult.Cancel) return;
            if (sfd.FileName.Trim() == "") return;
            XLWorkbook wb = new XLWorkbook();
            DataTable dt = (DataTable)dgvMain.DataSource;
            if (dt.Columns.Contains("ContentHTML")) dt.Columns.Remove("ContentHTML");
            wb.Worksheets.Add(dt, cboSearchType.Text);
            wb.SaveAs(sfd.FileName);
            if (MessageBox.Show("File is successfully saved at '" + sfd.FileName +
                "'. Do you wish to open it now?", "File Saved",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Process.Start(sfd.FileName);
            }
        }
    }
}
