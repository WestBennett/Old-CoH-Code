namespace DeTexturizer
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
            this.btnConvertFile = new System.Windows.Forms.Button();
            this.btnConvertDirectory = new System.Windows.Forms.Button();
            this.chkExportXML = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnConvertFile
            // 
            this.btnConvertFile.Location = new System.Drawing.Point(12, 35);
            this.btnConvertFile.Name = "btnConvertFile";
            this.btnConvertFile.Size = new System.Drawing.Size(116, 23);
            this.btnConvertFile.TabIndex = 2;
            this.btnConvertFile.Text = "Convert File";
            this.btnConvertFile.UseVisualStyleBackColor = true;
            this.btnConvertFile.Click += new System.EventHandler(this.BtnChooseFile_Click);
            // 
            // btnConvertDirectory
            // 
            this.btnConvertDirectory.Location = new System.Drawing.Point(134, 35);
            this.btnConvertDirectory.Name = "btnConvertDirectory";
            this.btnConvertDirectory.Size = new System.Drawing.Size(130, 23);
            this.btnConvertDirectory.TabIndex = 3;
            this.btnConvertDirectory.Text = "Convert Directory";
            this.btnConvertDirectory.UseVisualStyleBackColor = true;
            this.btnConvertDirectory.Click += new System.EventHandler(this.BtnConvertDirectory_Click);
            // 
            // chkExportXML
            // 
            this.chkExportXML.AutoSize = true;
            this.chkExportXML.Location = new System.Drawing.Point(12, 12);
            this.chkExportXML.Name = "chkExportXML";
            this.chkExportXML.Size = new System.Drawing.Size(81, 17);
            this.chkExportXML.TabIndex = 4;
            this.chkExportXML.Text = "Export XML";
            this.chkExportXML.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(276, 67);
            this.Controls.Add(this.chkExportXML);
            this.Controls.Add(this.btnConvertDirectory);
            this.Controls.Add(this.btnConvertFile);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "DeTexturizer v 1.5";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnConvertFile;
        private System.Windows.Forms.Button btnConvertDirectory;
        private System.Windows.Forms.CheckBox chkExportXML;
    }
}

