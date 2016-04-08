namespace SimplePaletteQuantizer
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
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.panelMain = new System.Windows.Forms.Panel();
            this.splitterMain = new System.Windows.Forms.Splitter();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.panelRight = new System.Windows.Forms.Panel();
            this.dialogOpenFile = new System.Windows.Forms.OpenFileDialog();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.pictureSource = new System.Windows.Forms.PictureBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.pictureTarget = new System.Windows.Forms.PictureBox();
            this.panelMain.SuspendLayout();
            this.panelLeft.SuspendLayout();
            this.panelRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureTarget)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonBrowse.Location = new System.Drawing.Point(5, 409);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(614, 30);
            this.buttonBrowse.TabIndex = 0;
            this.buttonBrowse.Text = "Browse for a file image...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.splitterMain);
            this.panelMain.Controls.Add(this.panelLeft);
            this.panelMain.Controls.Add(this.panelRight);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(5, 5);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.panelMain.Size = new System.Drawing.Size(614, 404);
            this.panelMain.TabIndex = 2;
            // 
            // splitterMain
            // 
            this.splitterMain.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.splitterMain.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitterMain.Location = new System.Drawing.Point(302, 0);
            this.splitterMain.Name = "splitterMain";
            this.splitterMain.Size = new System.Drawing.Size(5, 399);
            this.splitterMain.TabIndex = 2;
            this.splitterMain.TabStop = false;
            // 
            // panelLeft
            // 
            this.panelLeft.Controls.Add(this.pictureSource);
            this.panelLeft.Controls.Add(this.textBox1);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(307, 399);
            this.panelLeft.TabIndex = 0;
            // 
            // panelRight
            // 
            this.panelRight.Controls.Add(this.pictureTarget);
            this.panelRight.Controls.Add(this.textBox2);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelRight.Location = new System.Drawing.Point(307, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(307, 399);
            this.panelRight.TabIndex = 1;
            // 
            // dialogOpenFile
            // 
            this.dialogOpenFile.Filter = "Supported images|*.png;*.jpg;*.gif;*.jpeg;*.bmp;*.tiff";
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(307, 20);
            this.textBox1.TabIndex = 2;
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // pictureSource
            // 
            this.pictureSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureSource.Location = new System.Drawing.Point(0, 20);
            this.pictureSource.Name = "pictureSource";
            this.pictureSource.Size = new System.Drawing.Size(307, 379);
            this.pictureSource.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureSource.TabIndex = 3;
            this.pictureSource.TabStop = false;
            // 
            // textBox2
            // 
            this.textBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox2.Location = new System.Drawing.Point(0, 0);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(307, 20);
            this.textBox2.TabIndex = 3;
            this.textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // pictureTarget
            // 
            this.pictureTarget.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureTarget.Location = new System.Drawing.Point(0, 20);
            this.pictureTarget.Name = "pictureTarget";
            this.pictureTarget.Size = new System.Drawing.Size(307, 379);
            this.pictureTarget.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureTarget.TabIndex = 4;
            this.pictureTarget.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 444);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.buttonBrowse);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Simple palette quantizer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.panelMain.ResumeLayout(false);
            this.panelLeft.ResumeLayout(false);
            this.panelLeft.PerformLayout();
            this.panelRight.ResumeLayout(false);
            this.panelRight.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureTarget)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.OpenFileDialog dialogOpenFile;
        private System.Windows.Forms.Splitter splitterMain;
        private System.Windows.Forms.PictureBox pictureSource;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.PictureBox pictureTarget;
        private System.Windows.Forms.TextBox textBox2;
    }
}

