namespace CoH_Modder
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.txtCoHRootDirectory = new System.Windows.Forms.TextBox();
            this.lblRootDirectory = new System.Windows.Forms.Label();
            this.btnPickRoot = new System.Windows.Forms.Button();
            this.btnCreateEditMod = new System.Windows.Forms.Button();
            this.lstUninstalledMods = new System.Windows.Forms.ListBox();
            this.grpUninstalledMods = new System.Windows.Forms.GroupBox();
            this.scUninstalledLocalMods = new System.Windows.Forms.SplitContainer();
            this.txtUninstalledModDescription = new System.Windows.Forms.TextBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnInstall = new System.Windows.Forms.Button();
            this.grpInstalledMods = new System.Windows.Forms.GroupBox();
            this.scInstalledMods = new System.Windows.Forms.SplitContainer();
            this.lstInstalledMods = new System.Windows.Forms.ListBox();
            this.txtInstalledModDescription = new System.Windows.Forms.TextBox();
            this.btnRemove = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.Version = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.ProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnBrowseForMod = new System.Windows.Forms.Button();
            this.grpServerMods = new System.Windows.Forms.GroupBox();
            this.scServerMods = new System.Windows.Forms.SplitContainer();
            this.lstServerMods = new System.Windows.Forms.ListBox();
            this.txtServerMods = new System.Windows.Forms.TextBox();
            this.btnDownloadServerMod = new System.Windows.Forms.Button();
            this.btnInstallServerMod = new System.Windows.Forms.Button();
            this.btnFreshStart = new System.Windows.Forms.Button();
            this.scMain = new System.Windows.Forms.SplitContainer();
            this.scBottom = new System.Windows.Forms.SplitContainer();
            this.lblCategoryFilter = new System.Windows.Forms.Label();
            this.cboCategory = new System.Windows.Forms.ComboBox();
            this.grpUninstalledMods.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scUninstalledLocalMods)).BeginInit();
            this.scUninstalledLocalMods.Panel1.SuspendLayout();
            this.scUninstalledLocalMods.Panel2.SuspendLayout();
            this.scUninstalledLocalMods.SuspendLayout();
            this.grpInstalledMods.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scInstalledMods)).BeginInit();
            this.scInstalledMods.Panel1.SuspendLayout();
            this.scInstalledMods.Panel2.SuspendLayout();
            this.scInstalledMods.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.grpServerMods.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scServerMods)).BeginInit();
            this.scServerMods.Panel1.SuspendLayout();
            this.scServerMods.Panel2.SuspendLayout();
            this.scServerMods.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scMain)).BeginInit();
            this.scMain.Panel1.SuspendLayout();
            this.scMain.Panel2.SuspendLayout();
            this.scMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scBottom)).BeginInit();
            this.scBottom.Panel1.SuspendLayout();
            this.scBottom.Panel2.SuspendLayout();
            this.scBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtCoHRootDirectory
            // 
            this.txtCoHRootDirectory.Location = new System.Drawing.Point(120, 12);
            this.txtCoHRootDirectory.Name = "txtCoHRootDirectory";
            this.txtCoHRootDirectory.ReadOnly = true;
            this.txtCoHRootDirectory.Size = new System.Drawing.Size(360, 20);
            this.txtCoHRootDirectory.TabIndex = 1;
            // 
            // lblRootDirectory
            // 
            this.lblRootDirectory.AutoSize = true;
            this.lblRootDirectory.Location = new System.Drawing.Point(12, 15);
            this.lblRootDirectory.Name = "lblRootDirectory";
            this.lblRootDirectory.Size = new System.Drawing.Size(102, 13);
            this.lblRootDirectory.TabIndex = 2;
            this.lblRootDirectory.Text = "CoH Root Directory:";
            // 
            // btnPickRoot
            // 
            this.btnPickRoot.Location = new System.Drawing.Point(486, 10);
            this.btnPickRoot.Name = "btnPickRoot";
            this.btnPickRoot.Size = new System.Drawing.Size(80, 23);
            this.btnPickRoot.TabIndex = 3;
            this.btnPickRoot.Text = "Browse";
            this.btnPickRoot.UseVisualStyleBackColor = true;
            this.btnPickRoot.Click += new System.EventHandler(this.BtnPickRoot_Click);
            // 
            // btnCreateEditMod
            // 
            this.btnCreateEditMod.Location = new System.Drawing.Point(395, 39);
            this.btnCreateEditMod.Name = "btnCreateEditMod";
            this.btnCreateEditMod.Size = new System.Drawing.Size(121, 23);
            this.btnCreateEditMod.TabIndex = 4;
            this.btnCreateEditMod.Text = "Create/Edit Mod";
            this.btnCreateEditMod.UseVisualStyleBackColor = true;
            this.btnCreateEditMod.Click += new System.EventHandler(this.BtnCreateMod_Click);
            // 
            // lstUninstalledMods
            // 
            this.lstUninstalledMods.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstUninstalledMods.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstUninstalledMods.FormattingEnabled = true;
            this.lstUninstalledMods.Location = new System.Drawing.Point(3, 3);
            this.lstUninstalledMods.Name = "lstUninstalledMods";
            this.lstUninstalledMods.ScrollAlwaysVisible = true;
            this.lstUninstalledMods.Size = new System.Drawing.Size(515, 147);
            this.lstUninstalledMods.TabIndex = 5;
            this.lstUninstalledMods.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstBox_DrawItem);
            this.lstUninstalledMods.SelectedValueChanged += new System.EventHandler(this.LstUninstalledMods_SelectedValueChanged);
            // 
            // grpUninstalledMods
            // 
            this.grpUninstalledMods.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpUninstalledMods.Controls.Add(this.scUninstalledLocalMods);
            this.grpUninstalledMods.Location = new System.Drawing.Point(3, 3);
            this.grpUninstalledMods.Name = "grpUninstalledMods";
            this.grpUninstalledMods.Size = new System.Drawing.Size(917, 179);
            this.grpUninstalledMods.TabIndex = 6;
            this.grpUninstalledMods.TabStop = false;
            this.grpUninstalledMods.Text = "Uninstalled Local Mods:";
            // 
            // scUninstalledLocalMods
            // 
            this.scUninstalledLocalMods.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scUninstalledLocalMods.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.scUninstalledLocalMods.Location = new System.Drawing.Point(6, 19);
            this.scUninstalledLocalMods.Name = "scUninstalledLocalMods";
            // 
            // scUninstalledLocalMods.Panel1
            // 
            this.scUninstalledLocalMods.Panel1.Controls.Add(this.lstUninstalledMods);
            // 
            // scUninstalledLocalMods.Panel2
            // 
            this.scUninstalledLocalMods.Panel2.Controls.Add(this.txtUninstalledModDescription);
            this.scUninstalledLocalMods.Panel2.Controls.Add(this.btnDelete);
            this.scUninstalledLocalMods.Panel2.Controls.Add(this.btnInstall);
            this.scUninstalledLocalMods.Size = new System.Drawing.Size(911, 156);
            this.scUninstalledLocalMods.SplitterDistance = 525;
            this.scUninstalledLocalMods.TabIndex = 9;
            // 
            // txtUninstalledModDescription
            // 
            this.txtUninstalledModDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtUninstalledModDescription.Location = new System.Drawing.Point(4, 32);
            this.txtUninstalledModDescription.Multiline = true;
            this.txtUninstalledModDescription.Name = "txtUninstalledModDescription";
            this.txtUninstalledModDescription.ReadOnly = true;
            this.txtUninstalledModDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtUninstalledModDescription.Size = new System.Drawing.Size(372, 117);
            this.txtUninstalledModDescription.TabIndex = 6;
            // 
            // btnDelete
            // 
            this.btnDelete.Enabled = false;
            this.btnDelete.Location = new System.Drawing.Point(189, 3);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(186, 23);
            this.btnDelete.TabIndex = 8;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            // 
            // btnInstall
            // 
            this.btnInstall.Enabled = false;
            this.btnInstall.Location = new System.Drawing.Point(3, 3);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(186, 23);
            this.btnInstall.TabIndex = 6;
            this.btnInstall.Text = "Install >";
            this.btnInstall.UseVisualStyleBackColor = true;
            this.btnInstall.Click += new System.EventHandler(this.BtnInstall_Click);
            // 
            // grpInstalledMods
            // 
            this.grpInstalledMods.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpInstalledMods.Controls.Add(this.scInstalledMods);
            this.grpInstalledMods.Location = new System.Drawing.Point(9, 3);
            this.grpInstalledMods.Name = "grpInstalledMods";
            this.grpInstalledMods.Size = new System.Drawing.Size(911, 178);
            this.grpInstalledMods.TabIndex = 7;
            this.grpInstalledMods.TabStop = false;
            this.grpInstalledMods.Text = "Installed Mods:";
            // 
            // scInstalledMods
            // 
            this.scInstalledMods.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scInstalledMods.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.scInstalledMods.Location = new System.Drawing.Point(6, 19);
            this.scInstalledMods.Name = "scInstalledMods";
            // 
            // scInstalledMods.Panel1
            // 
            this.scInstalledMods.Panel1.Controls.Add(this.lstInstalledMods);
            // 
            // scInstalledMods.Panel2
            // 
            this.scInstalledMods.Panel2.Controls.Add(this.txtInstalledModDescription);
            this.scInstalledMods.Panel2.Controls.Add(this.btnRemove);
            this.scInstalledMods.Size = new System.Drawing.Size(906, 152);
            this.scInstalledMods.SplitterDistance = 520;
            this.scInstalledMods.TabIndex = 9;
            // 
            // lstInstalledMods
            // 
            this.lstInstalledMods.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstInstalledMods.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstInstalledMods.FormattingEnabled = true;
            this.lstInstalledMods.Location = new System.Drawing.Point(3, 3);
            this.lstInstalledMods.Name = "lstInstalledMods";
            this.lstInstalledMods.ScrollAlwaysVisible = true;
            this.lstInstalledMods.Size = new System.Drawing.Size(515, 147);
            this.lstInstalledMods.TabIndex = 5;
            this.lstInstalledMods.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstBox_DrawItem);
            this.lstInstalledMods.SelectedValueChanged += new System.EventHandler(this.LstInstalledMods_SelectedValueChanged);
            // 
            // txtInstalledModDescription
            // 
            this.txtInstalledModDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInstalledModDescription.Location = new System.Drawing.Point(3, 32);
            this.txtInstalledModDescription.Multiline = true;
            this.txtInstalledModDescription.Name = "txtInstalledModDescription";
            this.txtInstalledModDescription.ReadOnly = true;
            this.txtInstalledModDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtInstalledModDescription.Size = new System.Drawing.Size(371, 113);
            this.txtInstalledModDescription.TabIndex = 7;
            // 
            // btnRemove
            // 
            this.btnRemove.Enabled = false;
            this.btnRemove.Location = new System.Drawing.Point(3, 3);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(372, 23);
            this.btnRemove.TabIndex = 8;
            this.btnRemove.Text = "< Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.BtnRemove_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Version,
            this.StatusLabel,
            this.ProgressBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 664);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(954, 22);
            this.statusStrip1.TabIndex = 9;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // Version
            // 
            this.Version.Name = "Version";
            this.Version.Size = new System.Drawing.Size(69, 17);
            this.Version.Text = "Version 1.69";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(118, 17);
            this.StatusLabel.Text = "toolStripStatusLabel1";
            // 
            // ProgressBar
            // 
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(500, 16);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Image = ((System.Drawing.Image)(resources.GetObject("btnRefresh.Image")));
            this.btnRefresh.Location = new System.Drawing.Point(572, 10);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(23, 23);
            this.btnRefresh.TabIndex = 10;
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // btnBrowseForMod
            // 
            this.btnBrowseForMod.Location = new System.Drawing.Point(247, 38);
            this.btnBrowseForMod.Name = "btnBrowseForMod";
            this.btnBrowseForMod.Size = new System.Drawing.Size(142, 23);
            this.btnBrowseForMod.TabIndex = 11;
            this.btnBrowseForMod.Text = "Browse for Mod to Install";
            this.btnBrowseForMod.UseVisualStyleBackColor = true;
            this.btnBrowseForMod.Click += new System.EventHandler(this.BtnBrowseForMod_Click);
            // 
            // grpServerMods
            // 
            this.grpServerMods.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpServerMods.Controls.Add(this.scServerMods);
            this.grpServerMods.Location = new System.Drawing.Point(3, 3);
            this.grpServerMods.Name = "grpServerMods";
            this.grpServerMods.Size = new System.Drawing.Size(927, 183);
            this.grpServerMods.TabIndex = 12;
            this.grpServerMods.TabStop = false;
            this.grpServerMods.Text = "Uninstalled Server Mods:";
            // 
            // scServerMods
            // 
            this.scServerMods.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scServerMods.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.scServerMods.Location = new System.Drawing.Point(6, 19);
            this.scServerMods.Name = "scServerMods";
            // 
            // scServerMods.Panel1
            // 
            this.scServerMods.Panel1.Controls.Add(this.lstServerMods);
            // 
            // scServerMods.Panel2
            // 
            this.scServerMods.Panel2.Controls.Add(this.txtServerMods);
            this.scServerMods.Panel2.Controls.Add(this.btnDownloadServerMod);
            this.scServerMods.Panel2.Controls.Add(this.btnInstallServerMod);
            this.scServerMods.Size = new System.Drawing.Size(915, 158);
            this.scServerMods.SplitterDistance = 527;
            this.scServerMods.TabIndex = 8;
            // 
            // lstServerMods
            // 
            this.lstServerMods.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstServerMods.FormattingEnabled = true;
            this.lstServerMods.Location = new System.Drawing.Point(3, 3);
            this.lstServerMods.Name = "lstServerMods";
            this.lstServerMods.ScrollAlwaysVisible = true;
            this.lstServerMods.Size = new System.Drawing.Size(517, 147);
            this.lstServerMods.TabIndex = 5;
            this.lstServerMods.SelectedValueChanged += new System.EventHandler(this.LstServerMods_SelectedValueChanged);
            // 
            // txtServerMods
            // 
            this.txtServerMods.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtServerMods.Location = new System.Drawing.Point(3, 32);
            this.txtServerMods.Multiline = true;
            this.txtServerMods.Name = "txtServerMods";
            this.txtServerMods.ReadOnly = true;
            this.txtServerMods.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtServerMods.Size = new System.Drawing.Size(374, 119);
            this.txtServerMods.TabIndex = 6;
            // 
            // btnDownloadServerMod
            // 
            this.btnDownloadServerMod.Enabled = false;
            this.btnDownloadServerMod.Location = new System.Drawing.Point(189, 3);
            this.btnDownloadServerMod.Name = "btnDownloadServerMod";
            this.btnDownloadServerMod.Size = new System.Drawing.Size(186, 23);
            this.btnDownloadServerMod.TabIndex = 7;
            this.btnDownloadServerMod.Text = "Download ↓";
            this.btnDownloadServerMod.UseVisualStyleBackColor = true;
            this.btnDownloadServerMod.Click += new System.EventHandler(this.BtnDownloadServerMod_Click);
            // 
            // btnInstallServerMod
            // 
            this.btnInstallServerMod.Enabled = false;
            this.btnInstallServerMod.Location = new System.Drawing.Point(3, 3);
            this.btnInstallServerMod.Name = "btnInstallServerMod";
            this.btnInstallServerMod.Size = new System.Drawing.Size(186, 23);
            this.btnInstallServerMod.TabIndex = 6;
            this.btnInstallServerMod.Text = "Install >";
            this.btnInstallServerMod.UseVisualStyleBackColor = true;
            this.btnInstallServerMod.Click += new System.EventHandler(this.BtnInstallServerMod_Click);
            // 
            // btnFreshStart
            // 
            this.btnFreshStart.Location = new System.Drawing.Point(522, 39);
            this.btnFreshStart.Name = "btnFreshStart";
            this.btnFreshStart.Size = new System.Drawing.Size(73, 23);
            this.btnFreshStart.TabIndex = 13;
            this.btnFreshStart.Text = "Fresh Start";
            this.btnFreshStart.UseVisualStyleBackColor = true;
            this.btnFreshStart.Click += new System.EventHandler(this.BtnRegenerateServerModList_Click);
            // 
            // scMain
            // 
            this.scMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scMain.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.scMain.Location = new System.Drawing.Point(12, 65);
            this.scMain.Name = "scMain";
            this.scMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scMain.Panel1
            // 
            this.scMain.Panel1.Controls.Add(this.grpServerMods);
            // 
            // scMain.Panel2
            // 
            this.scMain.Panel2.Controls.Add(this.scBottom);
            this.scMain.Size = new System.Drawing.Size(938, 595);
            this.scMain.SplitterDistance = 191;
            this.scMain.TabIndex = 14;
            // 
            // scBottom
            // 
            this.scBottom.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scBottom.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.scBottom.Location = new System.Drawing.Point(3, 3);
            this.scBottom.Name = "scBottom";
            this.scBottom.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scBottom.Panel1
            // 
            this.scBottom.Panel1.Controls.Add(this.grpUninstalledMods);
            // 
            // scBottom.Panel2
            // 
            this.scBottom.Panel2.Controls.Add(this.grpInstalledMods);
            this.scBottom.Size = new System.Drawing.Size(928, 390);
            this.scBottom.SplitterDistance = 187;
            this.scBottom.TabIndex = 0;
            // 
            // lblCategoryFilter
            // 
            this.lblCategoryFilter.AutoSize = true;
            this.lblCategoryFilter.Location = new System.Drawing.Point(62, 41);
            this.lblCategoryFilter.Name = "lblCategoryFilter";
            this.lblCategoryFilter.Size = new System.Drawing.Size(52, 13);
            this.lblCategoryFilter.TabIndex = 15;
            this.lblCategoryFilter.Text = "Category:";
            // 
            // cboCategory
            // 
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Items.AddRange(new object[] {
            "",
            "OTHER",
            "AUDIO",
            "GRAPHICS",
            "MAPS",
            "ICONS",
            "CURSORS"});
            this.cboCategory.Location = new System.Drawing.Point(120, 38);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(121, 21);
            this.cboCategory.TabIndex = 16;
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.CboCategory_SelectedIndexChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(954, 686);
            this.Controls.Add(this.cboCategory);
            this.Controls.Add(this.lblCategoryFilter);
            this.Controls.Add(this.scMain);
            this.Controls.Add(this.btnFreshStart);
            this.Controls.Add(this.btnBrowseForMod);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnCreateEditMod);
            this.Controls.Add(this.btnPickRoot);
            this.Controls.Add(this.lblRootDirectory);
            this.Controls.Add(this.txtCoHRootDirectory);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "CoH Modder";
            this.grpUninstalledMods.ResumeLayout(false);
            this.scUninstalledLocalMods.Panel1.ResumeLayout(false);
            this.scUninstalledLocalMods.Panel2.ResumeLayout(false);
            this.scUninstalledLocalMods.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scUninstalledLocalMods)).EndInit();
            this.scUninstalledLocalMods.ResumeLayout(false);
            this.grpInstalledMods.ResumeLayout(false);
            this.scInstalledMods.Panel1.ResumeLayout(false);
            this.scInstalledMods.Panel2.ResumeLayout(false);
            this.scInstalledMods.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scInstalledMods)).EndInit();
            this.scInstalledMods.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.grpServerMods.ResumeLayout(false);
            this.scServerMods.Panel1.ResumeLayout(false);
            this.scServerMods.Panel2.ResumeLayout(false);
            this.scServerMods.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scServerMods)).EndInit();
            this.scServerMods.ResumeLayout(false);
            this.scMain.Panel1.ResumeLayout(false);
            this.scMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scMain)).EndInit();
            this.scMain.ResumeLayout(false);
            this.scBottom.Panel1.ResumeLayout(false);
            this.scBottom.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scBottom)).EndInit();
            this.scBottom.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblRootDirectory;
        private System.Windows.Forms.Button btnPickRoot;
        private System.Windows.Forms.Button btnCreateEditMod;
        private System.Windows.Forms.ListBox lstUninstalledMods;
        private System.Windows.Forms.GroupBox grpUninstalledMods;
        private System.Windows.Forms.GroupBox grpInstalledMods;
        private System.Windows.Forms.ListBox lstInstalledMods;
        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.TextBox txtUninstalledModDescription;
        private System.Windows.Forms.TextBox txtInstalledModDescription;
        public System.Windows.Forms.TextBox txtCoHRootDirectory;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.Button btnBrowseForMod;
        private System.Windows.Forms.GroupBox grpServerMods;
        private System.Windows.Forms.Button btnDownloadServerMod;
        private System.Windows.Forms.TextBox txtServerMods;
        private System.Windows.Forms.ListBox lstServerMods;
        private System.Windows.Forms.Button btnInstallServerMod;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.ToolStripStatusLabel Version;
        private System.Windows.Forms.Button btnFreshStart;
        public System.Windows.Forms.ToolStripProgressBar ProgressBar;
        private System.Windows.Forms.SplitContainer scUninstalledLocalMods;
        private System.Windows.Forms.SplitContainer scInstalledMods;
        private System.Windows.Forms.SplitContainer scServerMods;
        private System.Windows.Forms.SplitContainer scMain;
        private System.Windows.Forms.SplitContainer scBottom;
        private System.Windows.Forms.Label lblCategoryFilter;
        private System.Windows.Forms.ComboBox cboCategory;
    }
}

