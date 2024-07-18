namespace PowersChecker
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.cboEntity = new System.Windows.Forms.ComboBox();
            this.lblEntity = new System.Windows.Forms.Label();
            this.lblPowerSet = new System.Windows.Forms.Label();
            this.cboPowerSet = new System.Windows.Forms.ComboBox();
            this.lblPowerName = new System.Windows.Forms.Label();
            this.cboPowerName = new System.Windows.Forms.ComboBox();
            this.dgvAttributes = new System.Windows.Forms.DataGridView();
            this.lblPrimaryAttributes = new System.Windows.Forms.Label();
            this.lblAttributeMods = new System.Windows.Forms.Label();
            this.dgvAttributeMods = new System.Windows.Forms.DataGridView();
            this.ssMain = new System.Windows.Forms.StatusStrip();
            this.ProgBar = new System.Windows.Forms.ToolStripProgressBar();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.cmsMain = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAttributes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAttributeMods)).BeginInit();
            this.ssMain.SuspendLayout();
            this.cmsMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // cboEntity
            // 
            this.cboEntity.DisplayMember = "Entity";
            this.cboEntity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboEntity.FormattingEnabled = true;
            this.cboEntity.Location = new System.Drawing.Point(54, 12);
            this.cboEntity.Name = "cboEntity";
            this.cboEntity.Size = new System.Drawing.Size(121, 21);
            this.cboEntity.TabIndex = 0;
            this.cboEntity.ValueMember = "Entity";
            this.cboEntity.SelectedValueChanged += new System.EventHandler(this.CboEntity_SelectedValueChanged);
            // 
            // lblEntity
            // 
            this.lblEntity.AutoSize = true;
            this.lblEntity.Location = new System.Drawing.Point(12, 15);
            this.lblEntity.Name = "lblEntity";
            this.lblEntity.Size = new System.Drawing.Size(36, 13);
            this.lblEntity.TabIndex = 1;
            this.lblEntity.Text = "Entity:";
            // 
            // lblPowerSet
            // 
            this.lblPowerSet.AutoSize = true;
            this.lblPowerSet.Location = new System.Drawing.Point(181, 15);
            this.lblPowerSet.Name = "lblPowerSet";
            this.lblPowerSet.Size = new System.Drawing.Size(59, 13);
            this.lblPowerSet.TabIndex = 3;
            this.lblPowerSet.Text = "Power Set:";
            // 
            // cboPowerSet
            // 
            this.cboPowerSet.DisplayMember = "PowerSet";
            this.cboPowerSet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPowerSet.Enabled = false;
            this.cboPowerSet.FormattingEnabled = true;
            this.cboPowerSet.Location = new System.Drawing.Point(246, 12);
            this.cboPowerSet.Name = "cboPowerSet";
            this.cboPowerSet.Size = new System.Drawing.Size(121, 21);
            this.cboPowerSet.TabIndex = 2;
            this.cboPowerSet.ValueMember = "PowerSet";
            this.cboPowerSet.SelectedValueChanged += new System.EventHandler(this.CboPowerSet_SelectedValueChanged);
            // 
            // lblPowerName
            // 
            this.lblPowerName.AutoSize = true;
            this.lblPowerName.Location = new System.Drawing.Point(373, 12);
            this.lblPowerName.Name = "lblPowerName";
            this.lblPowerName.Size = new System.Drawing.Size(71, 13);
            this.lblPowerName.TabIndex = 5;
            this.lblPowerName.Text = "Power Name:";
            // 
            // cboPowerName
            // 
            this.cboPowerName.DisplayMember = "PowerName";
            this.cboPowerName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPowerName.Enabled = false;
            this.cboPowerName.FormattingEnabled = true;
            this.cboPowerName.Location = new System.Drawing.Point(450, 12);
            this.cboPowerName.Name = "cboPowerName";
            this.cboPowerName.Size = new System.Drawing.Size(121, 21);
            this.cboPowerName.TabIndex = 4;
            this.cboPowerName.ValueMember = "PowerName";
            this.cboPowerName.SelectedValueChanged += new System.EventHandler(this.CboPowerName_SelectedValueChanged);
            // 
            // dgvAttributes
            // 
            this.dgvAttributes.AllowUserToAddRows = false;
            this.dgvAttributes.AllowUserToDeleteRows = false;
            this.dgvAttributes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvAttributes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAttributes.ContextMenuStrip = this.cmsMain;
            this.dgvAttributes.Location = new System.Drawing.Point(15, 61);
            this.dgvAttributes.MultiSelect = false;
            this.dgvAttributes.Name = "dgvAttributes";
            this.dgvAttributes.ReadOnly = true;
            this.dgvAttributes.RowHeadersVisible = false;
            this.dgvAttributes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAttributes.Size = new System.Drawing.Size(556, 150);
            this.dgvAttributes.TabIndex = 6;
            // 
            // lblPrimaryAttributes
            // 
            this.lblPrimaryAttributes.AutoSize = true;
            this.lblPrimaryAttributes.Location = new System.Drawing.Point(12, 45);
            this.lblPrimaryAttributes.Name = "lblPrimaryAttributes";
            this.lblPrimaryAttributes.Size = new System.Drawing.Size(80, 13);
            this.lblPrimaryAttributes.TabIndex = 7;
            this.lblPrimaryAttributes.Text = "Main Attributes:";
            // 
            // lblAttributeMods
            // 
            this.lblAttributeMods.AutoSize = true;
            this.lblAttributeMods.Location = new System.Drawing.Point(12, 214);
            this.lblAttributeMods.Name = "lblAttributeMods";
            this.lblAttributeMods.Size = new System.Drawing.Size(94, 13);
            this.lblAttributeMods.TabIndex = 8;
            this.lblAttributeMods.Text = "Attribute Modifiers:";
            // 
            // dgvAttributeMods
            // 
            this.dgvAttributeMods.AllowUserToAddRows = false;
            this.dgvAttributeMods.AllowUserToDeleteRows = false;
            this.dgvAttributeMods.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvAttributeMods.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAttributeMods.ContextMenuStrip = this.cmsMain;
            this.dgvAttributeMods.Location = new System.Drawing.Point(15, 230);
            this.dgvAttributeMods.MultiSelect = false;
            this.dgvAttributeMods.Name = "dgvAttributeMods";
            this.dgvAttributeMods.ReadOnly = true;
            this.dgvAttributeMods.RowHeadersVisible = false;
            this.dgvAttributeMods.Size = new System.Drawing.Size(556, 150);
            this.dgvAttributeMods.TabIndex = 9;
            // 
            // ssMain
            // 
            this.ssMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ProgBar,
            this.StatusLabel});
            this.ssMain.Location = new System.Drawing.Point(0, 396);
            this.ssMain.Name = "ssMain";
            this.ssMain.Size = new System.Drawing.Size(583, 22);
            this.ssMain.TabIndex = 10;
            this.ssMain.Text = "statusStrip1";
            // 
            // ProgBar
            // 
            this.ProgBar.Name = "ProgBar";
            this.ProgBar.Size = new System.Drawing.Size(100, 16);
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(39, 17);
            this.StatusLabel.Text = "Ready";
            // 
            // cmsMain
            // 
            this.cmsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportDataToolStripMenuItem});
            this.cmsMain.Name = "cmsMain";
            this.cmsMain.Size = new System.Drawing.Size(135, 26);
            // 
            // exportDataToolStripMenuItem
            // 
            this.exportDataToolStripMenuItem.Name = "exportDataToolStripMenuItem";
            this.exportDataToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            this.exportDataToolStripMenuItem.Text = "Export Data";
            this.exportDataToolStripMenuItem.Click += new System.EventHandler(this.ExportDataToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(583, 418);
            this.Controls.Add(this.ssMain);
            this.Controls.Add(this.dgvAttributeMods);
            this.Controls.Add(this.lblAttributeMods);
            this.Controls.Add(this.lblPrimaryAttributes);
            this.Controls.Add(this.dgvAttributes);
            this.Controls.Add(this.lblPowerName);
            this.Controls.Add(this.cboPowerName);
            this.Controls.Add(this.lblPowerSet);
            this.Controls.Add(this.cboPowerSet);
            this.Controls.Add(this.lblEntity);
            this.Controls.Add(this.cboEntity);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Powers Checker";
            ((System.ComponentModel.ISupportInitialize)(this.dgvAttributes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAttributeMods)).EndInit();
            this.ssMain.ResumeLayout(false);
            this.ssMain.PerformLayout();
            this.cmsMain.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboEntity;
        private System.Windows.Forms.Label lblEntity;
        private System.Windows.Forms.Label lblPowerSet;
        private System.Windows.Forms.ComboBox cboPowerSet;
        private System.Windows.Forms.Label lblPowerName;
        private System.Windows.Forms.ComboBox cboPowerName;
        private System.Windows.Forms.DataGridView dgvAttributes;
        private System.Windows.Forms.Label lblPrimaryAttributes;
        private System.Windows.Forms.Label lblAttributeMods;
        private System.Windows.Forms.DataGridView dgvAttributeMods;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        public System.Windows.Forms.ToolStripProgressBar ProgBar;
        public System.Windows.Forms.StatusStrip ssMain;
        private System.Windows.Forms.ContextMenuStrip cmsMain;
        private System.Windows.Forms.ToolStripMenuItem exportDataToolStripMenuItem;
    }
}

