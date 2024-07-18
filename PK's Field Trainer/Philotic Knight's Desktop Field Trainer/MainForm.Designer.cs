
namespace Philotic_Knight
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
            this.lblArchetype = new System.Windows.Forms.Label();
            this.cboArchetype = new System.Windows.Forms.ComboBox();
            this.ssMain = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.grpBasicInfo = new System.Windows.Forms.GroupBox();
            this.lblSecondary = new System.Windows.Forms.Label();
            this.txtCharacterLevel = new System.Windows.Forms.TextBox();
            this.cboSecondary = new System.Windows.Forms.ComboBox();
            this.lblCharacterLevel = new System.Windows.Forms.Label();
            this.lblPrimary = new System.Windows.Forms.Label();
            this.cboPrimary = new System.Windows.Forms.ComboBox();
            this.lblCharacterName = new System.Windows.Forms.Label();
            this.txtCharacterName = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startNewCharacterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.importDefDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scanDefDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportDefDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.importCharacterFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportCharacterFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PowerPanel = new System.Windows.Forms.Panel();
            this.exportForumHTMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.ssMain.SuspendLayout();
            this.grpBasicInfo.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblArchetype
            // 
            this.lblArchetype.AutoSize = true;
            this.lblArchetype.Location = new System.Drawing.Point(35, 48);
            this.lblArchetype.Name = "lblArchetype";
            this.lblArchetype.Size = new System.Drawing.Size(58, 13);
            this.lblArchetype.TabIndex = 0;
            this.lblArchetype.Text = "Archetype:";
            // 
            // cboArchetype
            // 
            this.cboArchetype.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboArchetype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboArchetype.Enabled = false;
            this.cboArchetype.FormattingEnabled = true;
            this.cboArchetype.Location = new System.Drawing.Point(99, 45);
            this.cboArchetype.Name = "cboArchetype";
            this.cboArchetype.Size = new System.Drawing.Size(291, 21);
            this.cboArchetype.TabIndex = 1;
            this.cboArchetype.SelectedIndexChanged += new System.EventHandler(this.ActionHappened);
            // 
            // ssMain
            // 
            this.ssMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
            this.ssMain.Location = new System.Drawing.Point(0, 422);
            this.ssMain.Name = "ssMain";
            this.ssMain.Size = new System.Drawing.Size(404, 22);
            this.ssMain.TabIndex = 3;
            this.ssMain.Text = "statusStrip1";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // grpBasicInfo
            // 
            this.grpBasicInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpBasicInfo.Controls.Add(this.lblSecondary);
            this.grpBasicInfo.Controls.Add(this.txtCharacterLevel);
            this.grpBasicInfo.Controls.Add(this.cboSecondary);
            this.grpBasicInfo.Controls.Add(this.lblCharacterLevel);
            this.grpBasicInfo.Controls.Add(this.lblPrimary);
            this.grpBasicInfo.Controls.Add(this.cboPrimary);
            this.grpBasicInfo.Controls.Add(this.lblCharacterName);
            this.grpBasicInfo.Controls.Add(this.txtCharacterName);
            this.grpBasicInfo.Controls.Add(this.lblArchetype);
            this.grpBasicInfo.Controls.Add(this.cboArchetype);
            this.grpBasicInfo.Location = new System.Drawing.Point(0, 27);
            this.grpBasicInfo.Name = "grpBasicInfo";
            this.grpBasicInfo.Size = new System.Drawing.Size(399, 130);
            this.grpBasicInfo.TabIndex = 6;
            this.grpBasicInfo.TabStop = false;
            this.grpBasicInfo.Text = "Basic Info:";
            // 
            // lblSecondary
            // 
            this.lblSecondary.AutoSize = true;
            this.lblSecondary.Location = new System.Drawing.Point(32, 102);
            this.lblSecondary.Name = "lblSecondary";
            this.lblSecondary.Size = new System.Drawing.Size(61, 13);
            this.lblSecondary.TabIndex = 4;
            this.lblSecondary.Text = "Secondary:";
            // 
            // txtCharacterLevel
            // 
            this.txtCharacterLevel.Location = new System.Drawing.Point(352, 19);
            this.txtCharacterLevel.Name = "txtCharacterLevel";
            this.txtCharacterLevel.ReadOnly = true;
            this.txtCharacterLevel.Size = new System.Drawing.Size(38, 20);
            this.txtCharacterLevel.TabIndex = 63;
            this.txtCharacterLevel.TabStop = false;
            this.txtCharacterLevel.Text = "-3";
            // 
            // cboSecondary
            // 
            this.cboSecondary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSecondary.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSecondary.Enabled = false;
            this.cboSecondary.FormattingEnabled = true;
            this.cboSecondary.Location = new System.Drawing.Point(99, 99);
            this.cboSecondary.Name = "cboSecondary";
            this.cboSecondary.Size = new System.Drawing.Size(291, 21);
            this.cboSecondary.TabIndex = 5;
            this.cboSecondary.SelectedIndexChanged += new System.EventHandler(this.ActionHappened);
            // 
            // lblCharacterLevel
            // 
            this.lblCharacterLevel.AutoSize = true;
            this.lblCharacterLevel.Location = new System.Drawing.Point(261, 22);
            this.lblCharacterLevel.Name = "lblCharacterLevel";
            this.lblCharacterLevel.Size = new System.Drawing.Size(85, 13);
            this.lblCharacterLevel.TabIndex = 62;
            this.lblCharacterLevel.Text = "Character Level:";
            // 
            // lblPrimary
            // 
            this.lblPrimary.AutoSize = true;
            this.lblPrimary.Location = new System.Drawing.Point(49, 75);
            this.lblPrimary.Name = "lblPrimary";
            this.lblPrimary.Size = new System.Drawing.Size(44, 13);
            this.lblPrimary.TabIndex = 2;
            this.lblPrimary.Text = "Primary:";
            // 
            // cboPrimary
            // 
            this.cboPrimary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboPrimary.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPrimary.Enabled = false;
            this.cboPrimary.FormattingEnabled = true;
            this.cboPrimary.Location = new System.Drawing.Point(99, 72);
            this.cboPrimary.Name = "cboPrimary";
            this.cboPrimary.Size = new System.Drawing.Size(291, 21);
            this.cboPrimary.TabIndex = 3;
            this.cboPrimary.SelectedIndexChanged += new System.EventHandler(this.ActionHappened);
            // 
            // lblCharacterName
            // 
            this.lblCharacterName.AutoSize = true;
            this.lblCharacterName.Location = new System.Drawing.Point(6, 22);
            this.lblCharacterName.Name = "lblCharacterName";
            this.lblCharacterName.Size = new System.Drawing.Size(87, 13);
            this.lblCharacterName.TabIndex = 1;
            this.lblCharacterName.Text = "Character Name:";
            // 
            // txtCharacterName
            // 
            this.txtCharacterName.Enabled = false;
            this.txtCharacterName.Location = new System.Drawing.Point(99, 19);
            this.txtCharacterName.Name = "txtCharacterName";
            this.txtCharacterName.Size = new System.Drawing.Size(156, 20);
            this.txtCharacterName.TabIndex = 0;
            this.txtCharacterName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtCharacterName_KeyDown);
            this.txtCharacterName.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.TxtCharacterName_PreviewKeyDown);
            this.txtCharacterName.Validating += new System.ComponentModel.CancelEventHandler(this.TxtCharacterName_Validating);
            this.txtCharacterName.Validated += new System.EventHandler(this.TxtCharacterName_Validated);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuStrip1.Size = new System.Drawing.Size(404, 24);
            this.menuStrip1.TabIndex = 60;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startNewCharacterToolStripMenuItem,
            this.toolStripSeparator2,
            this.importDefDataToolStripMenuItem,
            this.scanDefDataToolStripMenuItem,
            this.exportDefDataToolStripMenuItem,
            this.toolStripSeparator1,
            this.importCharacterFileToolStripMenuItem,
            this.exportCharacterFileToolStripMenuItem,
            this.toolStripSeparator3,
            this.exportForumHTMLToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // startNewCharacterToolStripMenuItem
            // 
            this.startNewCharacterToolStripMenuItem.Name = "startNewCharacterToolStripMenuItem";
            this.startNewCharacterToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.startNewCharacterToolStripMenuItem.Text = "Start New Character";
            this.startNewCharacterToolStripMenuItem.Click += new System.EventHandler(this.StartNewCharacterToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(182, 6);
            // 
            // importDefDataToolStripMenuItem
            // 
            this.importDefDataToolStripMenuItem.Name = "importDefDataToolStripMenuItem";
            this.importDefDataToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.importDefDataToolStripMenuItem.Text = "Import Def Data";
            this.importDefDataToolStripMenuItem.Click += new System.EventHandler(this.ImportDefDataToolStripMenuItem_Click);
            // 
            // scanDefDataToolStripMenuItem
            // 
            this.scanDefDataToolStripMenuItem.Name = "scanDefDataToolStripMenuItem";
            this.scanDefDataToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.scanDefDataToolStripMenuItem.Text = "Scan Def Data";
            this.scanDefDataToolStripMenuItem.Click += new System.EventHandler(this.ScanDefDataToolStripMenuItem_Click);
            // 
            // exportDefDataToolStripMenuItem
            // 
            this.exportDefDataToolStripMenuItem.Name = "exportDefDataToolStripMenuItem";
            this.exportDefDataToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.exportDefDataToolStripMenuItem.Text = "Export Def Data";
            this.exportDefDataToolStripMenuItem.Click += new System.EventHandler(this.ExportDefDataToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(182, 6);
            // 
            // importCharacterFileToolStripMenuItem
            // 
            this.importCharacterFileToolStripMenuItem.Name = "importCharacterFileToolStripMenuItem";
            this.importCharacterFileToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.importCharacterFileToolStripMenuItem.Text = "Import Character File";
            this.importCharacterFileToolStripMenuItem.Click += new System.EventHandler(this.ImportCharacterFileToolStripMenuItem_Click);
            // 
            // exportCharacterFileToolStripMenuItem
            // 
            this.exportCharacterFileToolStripMenuItem.Name = "exportCharacterFileToolStripMenuItem";
            this.exportCharacterFileToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.exportCharacterFileToolStripMenuItem.Text = "Export Character File";
            this.exportCharacterFileToolStripMenuItem.Click += new System.EventHandler(this.ExportCharacterFileToolStripMenuItem_Click);
            // 
            // PowerPanel
            // 
            this.PowerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PowerPanel.AutoScroll = true;
            this.PowerPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PowerPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.PowerPanel.Location = new System.Drawing.Point(6, 163);
            this.PowerPanel.Name = "PowerPanel";
            this.PowerPanel.Size = new System.Drawing.Size(392, 256);
            this.PowerPanel.TabIndex = 64;
            this.PowerPanel.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.TxtCharacterName_PreviewKeyDown);
            // 
            // exportForumHTMLToolStripMenuItem
            // 
            this.exportForumHTMLToolStripMenuItem.Name = "exportForumHTMLToolStripMenuItem";
            this.exportForumHTMLToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.exportForumHTMLToolStripMenuItem.Text = "Export Forum HTML";
            this.exportForumHTMLToolStripMenuItem.Click += new System.EventHandler(this.ExportForumHTMLToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(182, 6);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.CornflowerBlue;
            this.ClientSize = new System.Drawing.Size(404, 444);
            this.Controls.Add(this.PowerPanel);
            this.Controls.Add(this.grpBasicInfo);
            this.Controls.Add(this.ssMain);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Philotic Knight\'s Field Trainer";
            this.ssMain.ResumeLayout(false);
            this.ssMain.PerformLayout();
            this.grpBasicInfo.ResumeLayout(false);
            this.grpBasicInfo.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblArchetype;
        private System.Windows.Forms.ComboBox cboArchetype;
        private System.Windows.Forms.StatusStrip ssMain;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.GroupBox grpBasicInfo;
        private System.Windows.Forms.Label lblSecondary;
        private System.Windows.Forms.ComboBox cboSecondary;
        private System.Windows.Forms.Label lblPrimary;
        private System.Windows.Forms.ComboBox cboPrimary;
        private System.Windows.Forms.Label lblCharacterName;
        private System.Windows.Forms.TextBox txtCharacterName;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importDefDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scanDefDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportDefDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem importCharacterFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportCharacterFileToolStripMenuItem;
        private System.Windows.Forms.TextBox txtCharacterLevel;
        private System.Windows.Forms.Label lblCharacterLevel;
        private System.Windows.Forms.Panel PowerPanel;
        private System.Windows.Forms.ToolStripMenuItem startNewCharacterToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem exportForumHTMLToolStripMenuItem;
    }
}

