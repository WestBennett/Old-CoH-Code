namespace Def_File_Translator
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
            this.btnReadDefFile = new System.Windows.Forms.Button();
            this.btnReadExcelFile = new System.Windows.Forms.Button();
            this.btnWriteDefFile = new System.Windows.Forms.Button();
            this.btnWriteExcelFile = new System.Windows.Forms.Button();
            this.dgvMain = new System.Windows.Forms.DataGridView();
            this.cboObjects = new System.Windows.Forms.ComboBox();
            this.lblObject = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMain)).BeginInit();
            this.SuspendLayout();
            // 
            // btnReadDefFile
            // 
            this.btnReadDefFile.Location = new System.Drawing.Point(12, 12);
            this.btnReadDefFile.Name = "btnReadDefFile";
            this.btnReadDefFile.Size = new System.Drawing.Size(93, 23);
            this.btnReadDefFile.TabIndex = 1;
            this.btnReadDefFile.Text = "Read Def File";
            this.btnReadDefFile.UseVisualStyleBackColor = true;
            this.btnReadDefFile.Click += new System.EventHandler(this.BtnReadDefFile_Click);
            // 
            // btnReadExcelFile
            // 
            this.btnReadExcelFile.Location = new System.Drawing.Point(111, 12);
            this.btnReadExcelFile.Name = "btnReadExcelFile";
            this.btnReadExcelFile.Size = new System.Drawing.Size(99, 23);
            this.btnReadExcelFile.TabIndex = 2;
            this.btnReadExcelFile.Text = "Read Excel File";
            this.btnReadExcelFile.UseVisualStyleBackColor = true;
            this.btnReadExcelFile.Click += new System.EventHandler(this.BtnReadExcelFile_Click);
            // 
            // btnWriteDefFile
            // 
            this.btnWriteDefFile.Enabled = false;
            this.btnWriteDefFile.Location = new System.Drawing.Point(216, 12);
            this.btnWriteDefFile.Name = "btnWriteDefFile";
            this.btnWriteDefFile.Size = new System.Drawing.Size(105, 23);
            this.btnWriteDefFile.TabIndex = 3;
            this.btnWriteDefFile.Text = "Write Def File";
            this.btnWriteDefFile.UseVisualStyleBackColor = true;
            this.btnWriteDefFile.Click += new System.EventHandler(this.BtnWriteDefFile_Click);
            // 
            // btnWriteExcelFile
            // 
            this.btnWriteExcelFile.Enabled = false;
            this.btnWriteExcelFile.Location = new System.Drawing.Point(327, 12);
            this.btnWriteExcelFile.Name = "btnWriteExcelFile";
            this.btnWriteExcelFile.Size = new System.Drawing.Size(109, 23);
            this.btnWriteExcelFile.TabIndex = 4;
            this.btnWriteExcelFile.Text = "Write Excel File";
            this.btnWriteExcelFile.UseVisualStyleBackColor = true;
            this.btnWriteExcelFile.Click += new System.EventHandler(this.BtnWriteExcelFile_Click);
            // 
            // dgvMain
            // 
            this.dgvMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvMain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMain.Location = new System.Drawing.Point(12, 71);
            this.dgvMain.Name = "dgvMain";
            this.dgvMain.Size = new System.Drawing.Size(879, 218);
            this.dgvMain.TabIndex = 5;
            // 
            // cboObjects
            // 
            this.cboObjects.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboObjects.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboObjects.Enabled = false;
            this.cboObjects.FormattingEnabled = true;
            this.cboObjects.Location = new System.Drawing.Point(59, 41);
            this.cboObjects.Name = "cboObjects";
            this.cboObjects.Size = new System.Drawing.Size(830, 21);
            this.cboObjects.TabIndex = 6;
            this.cboObjects.SelectedValueChanged += new System.EventHandler(this.CboObjects_SelectedValueChanged);
            // 
            // lblObject
            // 
            this.lblObject.AutoSize = true;
            this.lblObject.Location = new System.Drawing.Point(12, 44);
            this.lblObject.Name = "lblObject";
            this.lblObject.Size = new System.Drawing.Size(41, 13);
            this.lblObject.TabIndex = 7;
            this.lblObject.Text = "Object:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.ClientSize = new System.Drawing.Size(903, 301);
            this.Controls.Add(this.lblObject);
            this.Controls.Add(this.cboObjects);
            this.Controls.Add(this.dgvMain);
            this.Controls.Add(this.btnWriteExcelFile);
            this.Controls.Add(this.btnWriteDefFile);
            this.Controls.Add(this.btnReadExcelFile);
            this.Controls.Add(this.btnReadDefFile);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "PK\'s Def File Translator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMain)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnReadDefFile;
        private System.Windows.Forms.Button btnReadExcelFile;
        private System.Windows.Forms.Button btnWriteDefFile;
        private System.Windows.Forms.Button btnWriteExcelFile;
        private System.Windows.Forms.DataGridView dgvMain;
        private System.Windows.Forms.ComboBox cboObjects;
        private System.Windows.Forms.Label lblObject;
    }
}

