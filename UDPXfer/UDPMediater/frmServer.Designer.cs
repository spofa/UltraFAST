namespace UDPMediater
{
    partial class frmServer
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
            this.lblServerID = new System.Windows.Forms.Label();
            this.lblServerPort = new System.Windows.Forms.Label();
            this.tbServerPort = new System.Windows.Forms.TextBox();
            this.tbServerID = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblServerID
            // 
            this.lblServerID.AutoSize = true;
            this.lblServerID.Location = new System.Drawing.Point(13, 13);
            this.lblServerID.Name = "lblServerID";
            this.lblServerID.Size = new System.Drawing.Size(88, 13);
            this.lblServerID.TabIndex = 0;
            this.lblServerID.Text = "SERVER NAME:";
            // 
            // lblServerPort
            // 
            this.lblServerPort.AutoSize = true;
            this.lblServerPort.Location = new System.Drawing.Point(13, 44);
            this.lblServerPort.Name = "lblServerPort";
            this.lblServerPort.Size = new System.Drawing.Size(87, 13);
            this.lblServerPort.TabIndex = 2;
            this.lblServerPort.Text = "SERVER PORT:";
            // 
            // tbServerPort
            // 
            this.tbServerPort.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::UDPMediater.Properties.Settings.Default, "ListenPort", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.tbServerPort.Location = new System.Drawing.Point(107, 41);
            this.tbServerPort.Name = "tbServerPort";
            this.tbServerPort.Size = new System.Drawing.Size(199, 20);
            this.tbServerPort.TabIndex = 3;
            this.tbServerPort.Text = global::UDPMediater.Properties.Settings.Default.ListenPort;
            // 
            // tbServerID
            // 
            this.tbServerID.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::UDPMediater.Properties.Settings.Default, "ApplicationName", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.tbServerID.Location = new System.Drawing.Point(107, 10);
            this.tbServerID.Name = "tbServerID";
            this.tbServerID.ReadOnly = true;
            this.tbServerID.Size = new System.Drawing.Size(199, 20);
            this.tbServerID.TabIndex = 1;
            this.tbServerID.Text = global::UDPMediater.Properties.Settings.Default.ApplicationName;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(312, 10);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 51);
            this.btnStart.TabIndex = 4;
            this.btnStart.Text = "St&art";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(393, 10);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 51);
            this.btnStop.TabIndex = 5;
            this.btnStop.Text = "St&op";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // frmServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 122);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.tbServerPort);
            this.Controls.Add(this.lblServerPort);
            this.Controls.Add(this.tbServerID);
            this.Controls.Add(this.lblServerID);
            this.Name = "frmServer";
            this.Text = "UDP Mediater";
            this.Load += new System.EventHandler(this.frmServer_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblServerID;
        private System.Windows.Forms.TextBox tbServerID;
        private System.Windows.Forms.TextBox tbServerPort;
        private System.Windows.Forms.Label lblServerPort;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
    }
}

