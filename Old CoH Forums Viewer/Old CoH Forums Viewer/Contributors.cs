using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Old_CoH_Forums_Viewer
{
    public partial class Contributors : Form
    {
        private MainForm parent;
        public Contributors(MainForm parentForm)
        {
            InitializeComponent();
            parent = parentForm;
        }

        private void Contributors_Load(object sender, EventArgs e)
        {
            dgvContributors.DataSource = GetData("SELECT * FROM ( SELECT f.ProcessedBy, COUNT(FileName) As NumberOfFiles FROM files f GROUP BY f.ProcessedBy) t ORDER BY NumberOfFiles DESC");
            dgvFilesProcessed.DataSource = GetData("SELECT f.DateProcessed, f.ProcessedBy, f.FileName FROM files f ORDER BY f.DateProcessed DESC");
            dgvFilesToBeProcessed.DataSource = GetData("SELECT f.* FROM filestoprocess f LEFT JOIN files f2 ON f.FileName = f2.FileName WHERE f2.FileName IS NULL");
            DataTable TotalFiles = GetData("SELECT * FROM filestoprocess");

            lblNumContributors.Text = ((DataTable)dgvContributors.DataSource).Rows.Count + " Contributor(s)";
            lblProcessedFiles.Text = ((DataTable)dgvFilesProcessed.DataSource).Rows.Count + " Total Files Processed";
            lblFilesLeft.Text = ((DataTable)dgvFilesToBeProcessed.DataSource).Rows.Count + " file(s) left to process out of " +
                TotalFiles.Rows.Count + " Total Files";
        }

        public DataTable GetData(string command)
        {
            Cursor = Cursors.WaitCursor;
            DataTable dtResult = new DataTable();
            try
            {
                string connectionString = MainForm.ConnectionString.Replace("@TimeOut", "30");

                using (MySqlConnection s = new MySqlConnection(connectionString))
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
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void Contributors_VisibleChanged(object sender, EventArgs e)
        {
            Left = parent.Left + 5;
            Top = parent.Top + 5;
        }
    }
}
