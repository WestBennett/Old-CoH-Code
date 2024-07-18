namespace Old_CoH_Forums_Viewer
{
    partial class Contributors
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Contributors));
            this.scMain = new System.Windows.Forms.SplitContainer();
            this.lblNumContributors = new System.Windows.Forms.Label();
            this.dgvContributors = new System.Windows.Forms.DataGridView();
            this.lblContributors = new System.Windows.Forms.Label();
            this.scSecondary = new System.Windows.Forms.SplitContainer();
            this.lblProcessedFiles = new System.Windows.Forms.Label();
            this.dgvFilesProcessed = new System.Windows.Forms.DataGridView();
            this.lblFilesProcessed = new System.Windows.Forms.Label();
            this.lblFilesLeft = new System.Windows.Forms.Label();
            this.dgvFilesToBeProcessed = new System.Windows.Forms.DataGridView();
            this.lblFilesToBeProcessed = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.scMain)).BeginInit();
            this.scMain.Panel1.SuspendLayout();
            this.scMain.Panel2.SuspendLayout();
            this.scMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvContributors)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scSecondary)).BeginInit();
            this.scSecondary.Panel1.SuspendLayout();
            this.scSecondary.Panel2.SuspendLayout();
            this.scSecondary.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFilesProcessed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFilesToBeProcessed)).BeginInit();
            this.SuspendLayout();
            // 
            // scMain
            // 
            this.scMain.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.scMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scMain.Location = new System.Drawing.Point(0, 0);
            this.scMain.Name = "scMain";
            this.scMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scMain.Panel1
            // 
            this.scMain.Panel1.Controls.Add(this.lblNumContributors);
            this.scMain.Panel1.Controls.Add(this.dgvContributors);
            this.scMain.Panel1.Controls.Add(this.lblContributors);
            // 
            // scMain.Panel2
            // 
            this.scMain.Panel2.Controls.Add(this.scSecondary);
            this.scMain.Size = new System.Drawing.Size(593, 583);
            this.scMain.SplitterDistance = 162;
            this.scMain.TabIndex = 0;
            // 
            // lblNumContributors
            // 
            this.lblNumContributors.AutoSize = true;
            this.lblNumContributors.Location = new System.Drawing.Point(82, 7);
            this.lblNumContributors.Name = "lblNumContributors";
            this.lblNumContributors.Size = new System.Drawing.Size(10, 13);
            this.lblNumContributors.TabIndex = 6;
            this.lblNumContributors.Text = ".";
            // 
            // dgvContributors
            // 
            this.dgvContributors.AllowUserToAddRows = false;
            this.dgvContributors.AllowUserToDeleteRows = false;
            this.dgvContributors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvContributors.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvContributors.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvContributors.Location = new System.Drawing.Point(10, 23);
            this.dgvContributors.Name = "dgvContributors";
            this.dgvContributors.ReadOnly = true;
            this.dgvContributors.Size = new System.Drawing.Size(569, 132);
            this.dgvContributors.TabIndex = 1;
            // 
            // lblContributors
            // 
            this.lblContributors.AutoSize = true;
            this.lblContributors.Location = new System.Drawing.Point(10, 7);
            this.lblContributors.Name = "lblContributors";
            this.lblContributors.Size = new System.Drawing.Size(66, 13);
            this.lblContributors.TabIndex = 0;
            this.lblContributors.Text = "Contributors:";
            // 
            // scSecondary
            // 
            this.scSecondary.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.scSecondary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scSecondary.Location = new System.Drawing.Point(0, 0);
            this.scSecondary.Name = "scSecondary";
            this.scSecondary.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scSecondary.Panel1
            // 
            this.scSecondary.Panel1.Controls.Add(this.lblProcessedFiles);
            this.scSecondary.Panel1.Controls.Add(this.dgvFilesProcessed);
            this.scSecondary.Panel1.Controls.Add(this.lblFilesProcessed);
            // 
            // scSecondary.Panel2
            // 
            this.scSecondary.Panel2.Controls.Add(this.lblFilesLeft);
            this.scSecondary.Panel2.Controls.Add(this.dgvFilesToBeProcessed);
            this.scSecondary.Panel2.Controls.Add(this.lblFilesToBeProcessed);
            this.scSecondary.Size = new System.Drawing.Size(593, 417);
            this.scSecondary.SplitterDistance = 214;
            this.scSecondary.TabIndex = 0;
            // 
            // lblProcessedFiles
            // 
            this.lblProcessedFiles.AutoSize = true;
            this.lblProcessedFiles.Location = new System.Drawing.Point(100, 0);
            this.lblProcessedFiles.Name = "lblProcessedFiles";
            this.lblProcessedFiles.Size = new System.Drawing.Size(10, 13);
            this.lblProcessedFiles.TabIndex = 5;
            this.lblProcessedFiles.Text = ".";
            // 
            // dgvFilesProcessed
            // 
            this.dgvFilesProcessed.AllowUserToAddRows = false;
            this.dgvFilesProcessed.AllowUserToDeleteRows = false;
            this.dgvFilesProcessed.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvFilesProcessed.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvFilesProcessed.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFilesProcessed.Location = new System.Drawing.Point(10, 16);
            this.dgvFilesProcessed.Name = "dgvFilesProcessed";
            this.dgvFilesProcessed.ReadOnly = true;
            this.dgvFilesProcessed.Size = new System.Drawing.Size(569, 191);
            this.dgvFilesProcessed.TabIndex = 3;
            // 
            // lblFilesProcessed
            // 
            this.lblFilesProcessed.AutoSize = true;
            this.lblFilesProcessed.Location = new System.Drawing.Point(10, 0);
            this.lblFilesProcessed.Name = "lblFilesProcessed";
            this.lblFilesProcessed.Size = new System.Drawing.Size(84, 13);
            this.lblFilesProcessed.TabIndex = 2;
            this.lblFilesProcessed.Text = "Files Processed:";
            // 
            // lblFilesLeft
            // 
            this.lblFilesLeft.AutoSize = true;
            this.lblFilesLeft.Location = new System.Drawing.Point(133, 0);
            this.lblFilesLeft.Name = "lblFilesLeft";
            this.lblFilesLeft.Size = new System.Drawing.Size(10, 13);
            this.lblFilesLeft.TabIndex = 4;
            this.lblFilesLeft.Text = ".";
            // 
            // dgvFilesToBeProcessed
            // 
            this.dgvFilesToBeProcessed.AllowUserToAddRows = false;
            this.dgvFilesToBeProcessed.AllowUserToDeleteRows = false;
            this.dgvFilesToBeProcessed.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvFilesToBeProcessed.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvFilesToBeProcessed.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFilesToBeProcessed.Location = new System.Drawing.Point(10, 16);
            this.dgvFilesToBeProcessed.Name = "dgvFilesToBeProcessed";
            this.dgvFilesToBeProcessed.ReadOnly = true;
            this.dgvFilesToBeProcessed.Size = new System.Drawing.Size(569, 169);
            this.dgvFilesToBeProcessed.TabIndex = 3;
            // 
            // lblFilesToBeProcessed
            // 
            this.lblFilesToBeProcessed.AutoSize = true;
            this.lblFilesToBeProcessed.Location = new System.Drawing.Point(10, 0);
            this.lblFilesToBeProcessed.Name = "lblFilesToBeProcessed";
            this.lblFilesToBeProcessed.Size = new System.Drawing.Size(116, 13);
            this.lblFilesToBeProcessed.TabIndex = 2;
            this.lblFilesToBeProcessed.Text = "Files To Be Processed:";
            // 
            // Contributors
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(593, 583);
            this.Controls.Add(this.scMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Contributors";
            this.Text = "Project Spelunker - Contributors";
            this.Load += new System.EventHandler(this.Contributors_Load);
            this.VisibleChanged += new System.EventHandler(this.Contributors_VisibleChanged);
            this.scMain.Panel1.ResumeLayout(false);
            this.scMain.Panel1.PerformLayout();
            this.scMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scMain)).EndInit();
            this.scMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvContributors)).EndInit();
            this.scSecondary.Panel1.ResumeLayout(false);
            this.scSecondary.Panel1.PerformLayout();
            this.scSecondary.Panel2.ResumeLayout(false);
            this.scSecondary.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scSecondary)).EndInit();
            this.scSecondary.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvFilesProcessed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFilesToBeProcessed)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer scMain;
        private System.Windows.Forms.DataGridView dgvContributors;
        private System.Windows.Forms.Label lblContributors;
        private System.Windows.Forms.SplitContainer scSecondary;
        private System.Windows.Forms.DataGridView dgvFilesProcessed;
        private System.Windows.Forms.Label lblFilesProcessed;
        private System.Windows.Forms.DataGridView dgvFilesToBeProcessed;
        private System.Windows.Forms.Label lblFilesToBeProcessed;
        private System.Windows.Forms.Label lblNumContributors;
        private System.Windows.Forms.Label lblProcessedFiles;
        private System.Windows.Forms.Label lblFilesLeft;
    }
}