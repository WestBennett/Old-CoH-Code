namespace CoH_Modder
{
    partial class CreateEditMod
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateEditMod));
            this.grpModInfo = new System.Windows.Forms.GroupBox();
            this.lblModCategory = new System.Windows.Forms.Label();
            this.cboModCategory = new System.Windows.Forms.ComboBox();
            this.txtModDescription = new System.Windows.Forms.TextBox();
            this.lblModDescription = new System.Windows.Forms.Label();
            this.txtModName = new System.Windows.Forms.TextBox();
            this.lblModName = new System.Windows.Forms.Label();
            this.txtModAuthor = new System.Windows.Forms.TextBox();
            this.lblModAuthor = new System.Windows.Forms.Label();
            this.btnLoadMod = new System.Windows.Forms.Button();
            this.grpFiles = new System.Windows.Forms.GroupBox();
            this.btnChooseDestination = new System.Windows.Forms.Button();
            this.btnChooseSourceFile = new System.Windows.Forms.Button();
            this.btnRemoveFile = new System.Windows.Forms.Button();
            this.btnAddEditFile = new System.Windows.Forms.Button();
            this.txtDestination = new System.Windows.Forms.TextBox();
            this.lblDestination = new System.Windows.Forms.Label();
            this.txtSourceFile = new System.Windows.Forms.TextBox();
            this.lblSourceFile = new System.Windows.Forms.Label();
            this.dgvExistingFiles = new System.Windows.Forms.DataGridView();
            this.btnSaveMod = new System.Windows.Forms.Button();
            this.lblVersion = new System.Windows.Forms.Label();
            this.btnAddDataFolder = new System.Windows.Forms.Button();
            this.btnSaveAndUploadMod = new System.Windows.Forms.Button();
            this.ssMain = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.grpModInfo.SuspendLayout();
            this.grpFiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvExistingFiles)).BeginInit();
            this.ssMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpModInfo
            // 
            this.grpModInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpModInfo.Controls.Add(this.lblModCategory);
            this.grpModInfo.Controls.Add(this.cboModCategory);
            this.grpModInfo.Controls.Add(this.txtModDescription);
            this.grpModInfo.Controls.Add(this.lblModDescription);
            this.grpModInfo.Controls.Add(this.txtModName);
            this.grpModInfo.Controls.Add(this.lblModName);
            this.grpModInfo.Controls.Add(this.txtModAuthor);
            this.grpModInfo.Controls.Add(this.lblModAuthor);
            this.grpModInfo.Location = new System.Drawing.Point(12, 45);
            this.grpModInfo.Name = "grpModInfo";
            this.grpModInfo.Size = new System.Drawing.Size(667, 203);
            this.grpModInfo.TabIndex = 0;
            this.grpModInfo.TabStop = false;
            this.grpModInfo.Text = "Mod Info:";
            // 
            // lblModCategory
            // 
            this.lblModCategory.AutoSize = true;
            this.lblModCategory.Location = new System.Drawing.Point(6, 74);
            this.lblModCategory.Name = "lblModCategory";
            this.lblModCategory.Size = new System.Drawing.Size(76, 13);
            this.lblModCategory.TabIndex = 7;
            this.lblModCategory.Text = "Mod Category:";
            // 
            // cboModCategory
            // 
            this.cboModCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboModCategory.FormattingEnabled = true;
            this.cboModCategory.Items.AddRange(new object[] {
            "",
            "OTHER",
            "AUDIO",
            "GRAPHICS",
            "MAPS",
            "ICONS",
            "CURSORS"});
            this.cboModCategory.Location = new System.Drawing.Point(88, 71);
            this.cboModCategory.Name = "cboModCategory";
            this.cboModCategory.Size = new System.Drawing.Size(568, 21);
            this.cboModCategory.TabIndex = 6;
            this.cboModCategory.SelectedValueChanged += new System.EventHandler(this.CboModCategory_SelectedValueChanged);
            // 
            // txtModDescription
            // 
            this.txtModDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtModDescription.Location = new System.Drawing.Point(9, 111);
            this.txtModDescription.Multiline = true;
            this.txtModDescription.Name = "txtModDescription";
            this.txtModDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtModDescription.Size = new System.Drawing.Size(652, 86);
            this.txtModDescription.TabIndex = 5;
            this.txtModDescription.Validated += new System.EventHandler(this.TxtModDescription_Validated);
            // 
            // lblModDescription
            // 
            this.lblModDescription.AutoSize = true;
            this.lblModDescription.Location = new System.Drawing.Point(6, 95);
            this.lblModDescription.Name = "lblModDescription";
            this.lblModDescription.Size = new System.Drawing.Size(87, 13);
            this.lblModDescription.TabIndex = 4;
            this.lblModDescription.Text = "Mod Description:";
            // 
            // txtModName
            // 
            this.txtModName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtModName.Location = new System.Drawing.Point(88, 45);
            this.txtModName.Name = "txtModName";
            this.txtModName.Size = new System.Drawing.Size(573, 20);
            this.txtModName.TabIndex = 3;
            this.txtModName.Validated += new System.EventHandler(this.TxtModName_Validated);
            // 
            // lblModName
            // 
            this.lblModName.AutoSize = true;
            this.lblModName.Location = new System.Drawing.Point(17, 48);
            this.lblModName.Name = "lblModName";
            this.lblModName.Size = new System.Drawing.Size(62, 13);
            this.lblModName.TabIndex = 2;
            this.lblModName.Text = "Mod Name:";
            // 
            // txtModAuthor
            // 
            this.txtModAuthor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtModAuthor.Location = new System.Drawing.Point(88, 19);
            this.txtModAuthor.Name = "txtModAuthor";
            this.txtModAuthor.Size = new System.Drawing.Size(573, 20);
            this.txtModAuthor.TabIndex = 1;
            this.txtModAuthor.Validated += new System.EventHandler(this.TxtModAuthor_Validated);
            // 
            // lblModAuthor
            // 
            this.lblModAuthor.AutoSize = true;
            this.lblModAuthor.Location = new System.Drawing.Point(17, 22);
            this.lblModAuthor.Name = "lblModAuthor";
            this.lblModAuthor.Size = new System.Drawing.Size(65, 13);
            this.lblModAuthor.TabIndex = 0;
            this.lblModAuthor.Text = "Mod Author:";
            // 
            // btnLoadMod
            // 
            this.btnLoadMod.Location = new System.Drawing.Point(12, 12);
            this.btnLoadMod.Name = "btnLoadMod";
            this.btnLoadMod.Size = new System.Drawing.Size(106, 27);
            this.btnLoadMod.TabIndex = 0;
            this.btnLoadMod.Text = "Load Existing Mod";
            this.btnLoadMod.UseVisualStyleBackColor = true;
            this.btnLoadMod.Click += new System.EventHandler(this.BtnLoadMod_Click);
            // 
            // grpFiles
            // 
            this.grpFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFiles.Controls.Add(this.btnChooseDestination);
            this.grpFiles.Controls.Add(this.btnChooseSourceFile);
            this.grpFiles.Controls.Add(this.btnRemoveFile);
            this.grpFiles.Controls.Add(this.btnAddEditFile);
            this.grpFiles.Controls.Add(this.txtDestination);
            this.grpFiles.Controls.Add(this.lblDestination);
            this.grpFiles.Controls.Add(this.txtSourceFile);
            this.grpFiles.Controls.Add(this.lblSourceFile);
            this.grpFiles.Controls.Add(this.dgvExistingFiles);
            this.grpFiles.Location = new System.Drawing.Point(12, 254);
            this.grpFiles.Name = "grpFiles";
            this.grpFiles.Size = new System.Drawing.Size(667, 262);
            this.grpFiles.TabIndex = 1;
            this.grpFiles.TabStop = false;
            this.grpFiles.Text = "Mod Files:";
            // 
            // btnChooseDestination
            // 
            this.btnChooseDestination.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChooseDestination.Location = new System.Drawing.Point(581, 43);
            this.btnChooseDestination.Name = "btnChooseDestination";
            this.btnChooseDestination.Size = new System.Drawing.Size(75, 23);
            this.btnChooseDestination.TabIndex = 11;
            this.btnChooseDestination.Text = "Browse...";
            this.btnChooseDestination.UseVisualStyleBackColor = true;
            this.btnChooseDestination.Click += new System.EventHandler(this.BtnChooseDestination_Click);
            // 
            // btnChooseSourceFile
            // 
            this.btnChooseSourceFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChooseSourceFile.Location = new System.Drawing.Point(581, 17);
            this.btnChooseSourceFile.Name = "btnChooseSourceFile";
            this.btnChooseSourceFile.Size = new System.Drawing.Size(75, 23);
            this.btnChooseSourceFile.TabIndex = 10;
            this.btnChooseSourceFile.Text = "Browse...";
            this.btnChooseSourceFile.UseVisualStyleBackColor = true;
            this.btnChooseSourceFile.Click += new System.EventHandler(this.BtnChooseSourceFile_Click);
            // 
            // btnRemoveFile
            // 
            this.btnRemoveFile.Enabled = false;
            this.btnRemoveFile.Location = new System.Drawing.Point(162, 71);
            this.btnRemoveFile.Name = "btnRemoveFile";
            this.btnRemoveFile.Size = new System.Drawing.Size(92, 23);
            this.btnRemoveFile.TabIndex = 9;
            this.btnRemoveFile.Text = "Remove File(s)";
            this.btnRemoveFile.UseVisualStyleBackColor = true;
            this.btnRemoveFile.Click += new System.EventHandler(this.BtnRemoveFile_Click);
            // 
            // btnAddEditFile
            // 
            this.btnAddEditFile.Enabled = false;
            this.btnAddEditFile.Location = new System.Drawing.Point(74, 71);
            this.btnAddEditFile.Name = "btnAddEditFile";
            this.btnAddEditFile.Size = new System.Drawing.Size(82, 23);
            this.btnAddEditFile.TabIndex = 8;
            this.btnAddEditFile.Text = "Add/Edit File";
            this.btnAddEditFile.UseVisualStyleBackColor = true;
            this.btnAddEditFile.Click += new System.EventHandler(this.BtnAddFile_Click);
            // 
            // txtDestination
            // 
            this.txtDestination.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDestination.Location = new System.Drawing.Point(74, 45);
            this.txtDestination.Name = "txtDestination";
            this.txtDestination.ReadOnly = true;
            this.txtDestination.Size = new System.Drawing.Size(501, 20);
            this.txtDestination.TabIndex = 7;
            // 
            // lblDestination
            // 
            this.lblDestination.AutoSize = true;
            this.lblDestination.Location = new System.Drawing.Point(6, 48);
            this.lblDestination.Name = "lblDestination";
            this.lblDestination.Size = new System.Drawing.Size(60, 13);
            this.lblDestination.TabIndex = 6;
            this.lblDestination.Text = "Desination:";
            // 
            // txtSourceFile
            // 
            this.txtSourceFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSourceFile.Location = new System.Drawing.Point(74, 19);
            this.txtSourceFile.Name = "txtSourceFile";
            this.txtSourceFile.ReadOnly = true;
            this.txtSourceFile.Size = new System.Drawing.Size(501, 20);
            this.txtSourceFile.TabIndex = 5;
            // 
            // lblSourceFile
            // 
            this.lblSourceFile.AutoSize = true;
            this.lblSourceFile.Location = new System.Drawing.Point(6, 22);
            this.lblSourceFile.Name = "lblSourceFile";
            this.lblSourceFile.Size = new System.Drawing.Size(63, 13);
            this.lblSourceFile.TabIndex = 4;
            this.lblSourceFile.Text = "Source File:";
            // 
            // dgvExistingFiles
            // 
            this.dgvExistingFiles.AllowUserToAddRows = false;
            this.dgvExistingFiles.AllowUserToDeleteRows = false;
            this.dgvExistingFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvExistingFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvExistingFiles.Location = new System.Drawing.Point(6, 106);
            this.dgvExistingFiles.Name = "dgvExistingFiles";
            this.dgvExistingFiles.ReadOnly = true;
            this.dgvExistingFiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvExistingFiles.Size = new System.Drawing.Size(655, 143);
            this.dgvExistingFiles.TabIndex = 0;
            this.dgvExistingFiles.SelectionChanged += new System.EventHandler(this.DgvExistingFiles_SelectionChanged);
            // 
            // btnSaveMod
            // 
            this.btnSaveMod.Enabled = false;
            this.btnSaveMod.Location = new System.Drawing.Point(263, 12);
            this.btnSaveMod.Name = "btnSaveMod";
            this.btnSaveMod.Size = new System.Drawing.Size(106, 27);
            this.btnSaveMod.TabIndex = 2;
            this.btnSaveMod.Text = "Save Mod";
            this.btnSaveMod.UseVisualStyleBackColor = true;
            this.btnSaveMod.Click += new System.EventHandler(this.BtnSaveMod_Click);
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(549, 19);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(0, 13);
            this.lblVersion.TabIndex = 6;
            // 
            // btnAddDataFolder
            // 
            this.btnAddDataFolder.Location = new System.Drawing.Point(124, 12);
            this.btnAddDataFolder.Name = "btnAddDataFolder";
            this.btnAddDataFolder.Size = new System.Drawing.Size(133, 27);
            this.btnAddDataFolder.TabIndex = 7;
            this.btnAddDataFolder.Text = "Add Data Folder to Files";
            this.btnAddDataFolder.UseVisualStyleBackColor = true;
            this.btnAddDataFolder.Click += new System.EventHandler(this.BtnAddDataFolder_Click);
            // 
            // btnSaveAndUploadMod
            // 
            this.btnSaveAndUploadMod.Enabled = false;
            this.btnSaveAndUploadMod.Location = new System.Drawing.Point(375, 12);
            this.btnSaveAndUploadMod.Name = "btnSaveAndUploadMod";
            this.btnSaveAndUploadMod.Size = new System.Drawing.Size(168, 27);
            this.btnSaveAndUploadMod.TabIndex = 8;
            this.btnSaveAndUploadMod.Text = "Save and Upload Mod";
            this.btnSaveAndUploadMod.UseVisualStyleBackColor = true;
            this.btnSaveAndUploadMod.Click += new System.EventHandler(this.BtnSaveAndUploadMod_Click);
            // 
            // ssMain
            // 
            this.ssMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.progressBar});
            this.ssMain.Location = new System.Drawing.Point(0, 506);
            this.ssMain.Name = "ssMain";
            this.ssMain.Size = new System.Drawing.Size(688, 22);
            this.ssMain.TabIndex = 9;
            this.ssMain.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(571, 17);
            this.statusLabel.Spring = true;
            this.statusLabel.Text = "Ready...";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(100, 16);
            // 
            // CreateEditMod
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(688, 528);
            this.Controls.Add(this.ssMain);
            this.Controls.Add(this.btnSaveAndUploadMod);
            this.Controls.Add(this.btnAddDataFolder);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.btnSaveMod);
            this.Controls.Add(this.grpFiles);
            this.Controls.Add(this.btnLoadMod);
            this.Controls.Add(this.grpModInfo);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CreateEditMod";
            this.Text = "Create/Edit Mod";
            this.grpModInfo.ResumeLayout(false);
            this.grpModInfo.PerformLayout();
            this.grpFiles.ResumeLayout(false);
            this.grpFiles.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvExistingFiles)).EndInit();
            this.ssMain.ResumeLayout(false);
            this.ssMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpModInfo;
        private System.Windows.Forms.Button btnLoadMod;
        private System.Windows.Forms.TextBox txtModDescription;
        private System.Windows.Forms.Label lblModDescription;
        private System.Windows.Forms.TextBox txtModName;
        private System.Windows.Forms.Label lblModName;
        private System.Windows.Forms.TextBox txtModAuthor;
        private System.Windows.Forms.Label lblModAuthor;
        private System.Windows.Forms.GroupBox grpFiles;
        private System.Windows.Forms.DataGridView dgvExistingFiles;
        private System.Windows.Forms.Button btnRemoveFile;
        private System.Windows.Forms.Button btnAddEditFile;
        private System.Windows.Forms.TextBox txtDestination;
        private System.Windows.Forms.Label lblDestination;
        private System.Windows.Forms.TextBox txtSourceFile;
        private System.Windows.Forms.Label lblSourceFile;
        private System.Windows.Forms.Button btnSaveMod;
        private System.Windows.Forms.Button btnChooseDestination;
        private System.Windows.Forms.Button btnChooseSourceFile;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Button btnAddDataFolder;
        private System.Windows.Forms.Button btnSaveAndUploadMod;
        private System.Windows.Forms.Label lblModCategory;
        private System.Windows.Forms.ComboBox cboModCategory;
        private System.Windows.Forms.StatusStrip ssMain;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
    }
}