namespace RemoteClient
{
    partial class RemoteClientForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtServerIP = new System.Windows.Forms.TextBox();
            this.txtServerPort = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnConnectServer = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMyID = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtPartnerID = new System.Windows.Forms.TextBox();
            this.btnRequestPartnerInfo = new System.Windows.Forms.Button();
            this.txtPartnerDynamicIP = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtpartnerMessage = new System.Windows.Forms.TextBox();
            this.btnSendPartner = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtRelayBox = new System.Windows.Forms.TextBox();
            this.btnConnectServerTCP = new System.Windows.Forms.Button();
            this.ketch = new System.Windows.Forms.Button();
            this.btnSendImage = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server";
            // 
            // txtServerIP
            // 
            this.txtServerIP.Location = new System.Drawing.Point(78, 10);
            this.txtServerIP.Name = "txtServerIP";
            this.txtServerIP.Size = new System.Drawing.Size(100, 20);
            this.txtServerIP.TabIndex = 1;
            this.txtServerIP.Text = "192.168.11.124";
            // 
            // txtServerPort
            // 
            this.txtServerPort.Location = new System.Drawing.Point(258, 13);
            this.txtServerPort.Name = "txtServerPort";
            this.txtServerPort.Size = new System.Drawing.Size(52, 20);
            this.txtServerPort.TabIndex = 3;
            this.txtServerPort.Text = "161";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(192, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Server Port";
            // 
            // btnConnectServer
            // 
            this.btnConnectServer.Location = new System.Drawing.Point(334, 9);
            this.btnConnectServer.Name = "btnConnectServer";
            this.btnConnectServer.Size = new System.Drawing.Size(134, 23);
            this.btnConnectServer.TabIndex = 4;
            this.btnConnectServer.Text = "ConnectServer UDP";
            this.btnConnectServer.UseVisualStyleBackColor = true;
            this.btnConnectServer.Click += new System.EventHandler(this.btnConnectServer_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "My ID";
            // 
            // txtMyID
            // 
            this.txtMyID.Location = new System.Drawing.Point(78, 41);
            this.txtMyID.Name = "txtMyID";
            this.txtMyID.ReadOnly = true;
            this.txtMyID.Size = new System.Drawing.Size(153, 20);
            this.txtMyID.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 95);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Partner ID";
            // 
            // txtPartnerID
            // 
            this.txtPartnerID.Location = new System.Drawing.Point(78, 87);
            this.txtPartnerID.Name = "txtPartnerID";
            this.txtPartnerID.Size = new System.Drawing.Size(153, 20);
            this.txtPartnerID.TabIndex = 8;
            // 
            // btnRequestPartnerInfo
            // 
            this.btnRequestPartnerInfo.Location = new System.Drawing.Point(453, 87);
            this.btnRequestPartnerInfo.Name = "btnRequestPartnerInfo";
            this.btnRequestPartnerInfo.Size = new System.Drawing.Size(86, 23);
            this.btnRequestPartnerInfo.TabIndex = 9;
            this.btnRequestPartnerInfo.Text = "Connect";
            this.btnRequestPartnerInfo.UseVisualStyleBackColor = true;
            this.btnRequestPartnerInfo.Click += new System.EventHandler(this.btnRequestPartnerInfo_Click);
            // 
            // txtPartnerDynamicIP
            // 
            this.txtPartnerDynamicIP.Location = new System.Drawing.Point(238, 87);
            this.txtPartnerDynamicIP.Name = "txtPartnerDynamicIP";
            this.txtPartnerDynamicIP.ReadOnly = true;
            this.txtPartnerDynamicIP.Size = new System.Drawing.Size(209, 20);
            this.txtPartnerDynamicIP.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 140);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Message";
            // 
            // txtpartnerMessage
            // 
            this.txtpartnerMessage.Location = new System.Drawing.Point(78, 137);
            this.txtpartnerMessage.Name = "txtpartnerMessage";
            this.txtpartnerMessage.Size = new System.Drawing.Size(369, 20);
            this.txtpartnerMessage.TabIndex = 12;
            // 
            // btnSendPartner
            // 
            this.btnSendPartner.Location = new System.Drawing.Point(453, 137);
            this.btnSendPartner.Name = "btnSendPartner";
            this.btnSendPartner.Size = new System.Drawing.Size(86, 23);
            this.btnSendPartner.TabIndex = 13;
            this.btnSendPartner.Text = "Send";
            this.btnSendPartner.UseVisualStyleBackColor = true;
            this.btnSendPartner.Click += new System.EventHandler(this.btnSendPartner_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 170);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Relay Box";
            // 
            // txtRelayBox
            // 
            this.txtRelayBox.Location = new System.Drawing.Point(19, 186);
            this.txtRelayBox.Multiline = true;
            this.txtRelayBox.Name = "txtRelayBox";
            this.txtRelayBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtRelayBox.Size = new System.Drawing.Size(612, 240);
            this.txtRelayBox.TabIndex = 15;
            // 
            // btnConnectServerTCP
            // 
            this.btnConnectServerTCP.Location = new System.Drawing.Point(474, 10);
            this.btnConnectServerTCP.Name = "btnConnectServerTCP";
            this.btnConnectServerTCP.Size = new System.Drawing.Size(134, 23);
            this.btnConnectServerTCP.TabIndex = 17;
            this.btnConnectServerTCP.Text = "ConnectServer TCP";
            this.btnConnectServerTCP.UseVisualStyleBackColor = true;
            this.btnConnectServerTCP.Visible = false;
            // 
            // ketch
            // 
            this.ketch.Location = new System.Drawing.Point(371, 44);
            this.ketch.Name = "ketch";
            this.ketch.Size = new System.Drawing.Size(75, 23);
            this.ketch.TabIndex = 19;
            this.ketch.Text = "Save Settings";
            this.ketch.UseVisualStyleBackColor = true;
            this.ketch.Click += new System.EventHandler(this.ketch_Click);
            // 
            // btnSendImage
            // 
            this.btnSendImage.Location = new System.Drawing.Point(557, 87);
            this.btnSendImage.Name = "btnSendImage";
            this.btnSendImage.Size = new System.Drawing.Size(75, 23);
            this.btnSendImage.TabIndex = 20;
            this.btnSendImage.Text = "Send Image";
            this.btnSendImage.UseVisualStyleBackColor = true;
            this.btnSendImage.Click += new System.EventHandler(this.btnSendImage_Click);
            // 
            // RemoteClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(645, 438);
            this.Controls.Add(this.btnSendImage);
            this.Controls.Add(this.ketch);
            this.Controls.Add(this.btnConnectServerTCP);
            this.Controls.Add(this.txtRelayBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnSendPartner);
            this.Controls.Add(this.txtpartnerMessage);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtPartnerDynamicIP);
            this.Controls.Add(this.btnRequestPartnerInfo);
            this.Controls.Add(this.txtPartnerID);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtMyID);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnConnectServer);
            this.Controls.Add(this.txtServerPort);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtServerIP);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(16, 477);
            this.Name = "RemoteClientForm";
            this.Text = "Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtServerIP;
        private System.Windows.Forms.TextBox txtServerPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnConnectServer;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtMyID;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtPartnerID;
        private System.Windows.Forms.Button btnRequestPartnerInfo;
        private System.Windows.Forms.TextBox txtPartnerDynamicIP;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtpartnerMessage;
        private System.Windows.Forms.Button btnSendPartner;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtRelayBox;
        private System.Windows.Forms.Button btnConnectServerTCP;
        private System.Windows.Forms.Button ketch;
        private System.Windows.Forms.Button btnSendImage;
    }
}

