namespace UDPClient
{
    partial class frmClient
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
            this.lblServerPort = new System.Windows.Forms.Label();
            this.lblServerIP = new System.Windows.Forms.Label();
            this.btnDisConnect = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.tbServerPort = new System.Windows.Forms.TextBox();
            this.tbServerIP = new System.Windows.Forms.TextBox();
            this.tbServerID = new System.Windows.Forms.TextBox();
            this.lblServerID = new System.Windows.Forms.Label();
            this.btnRegister = new System.Windows.Forms.Button();
            this.btnFetchAll = new System.Windows.Forms.Button();
            this.btnFetchPartner = new System.Windows.Forms.Button();
            this.tbMyName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbMyPartnerName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnIntroduce = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnTransmitMessage = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblServerPort
            // 
            this.lblServerPort.AutoSize = true;
            this.lblServerPort.Location = new System.Drawing.Point(8, 68);
            this.lblServerPort.Name = "lblServerPort";
            this.lblServerPort.Size = new System.Drawing.Size(87, 13);
            this.lblServerPort.TabIndex = 6;
            this.lblServerPort.Text = "SERVER PORT:";
            // 
            // lblServerIP
            // 
            this.lblServerIP.AutoSize = true;
            this.lblServerIP.Location = new System.Drawing.Point(8, 37);
            this.lblServerIP.Name = "lblServerIP";
            this.lblServerIP.Size = new System.Drawing.Size(88, 13);
            this.lblServerIP.TabIndex = 4;
            this.lblServerIP.Text = "SERVER ADDR:";
            // 
            // btnDisConnect
            // 
            this.btnDisConnect.Enabled = false;
            this.btnDisConnect.Location = new System.Drawing.Point(388, 18);
            this.btnDisConnect.Name = "btnDisConnect";
            this.btnDisConnect.Size = new System.Drawing.Size(75, 51);
            this.btnDisConnect.TabIndex = 9;
            this.btnDisConnect.Text = "&DisConnect";
            this.btnDisConnect.UseVisualStyleBackColor = true;
            this.btnDisConnect.Click += new System.EventHandler(this.btnDisConnect_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(307, 18);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 51);
            this.btnConnect.TabIndex = 8;
            this.btnConnect.Text = "&Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // tbServerPort
            // 
            this.tbServerPort.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::UDPClient.Properties.Settings.Default, "ServerPort", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.tbServerPort.Location = new System.Drawing.Point(102, 65);
            this.tbServerPort.Name = "tbServerPort";
            this.tbServerPort.Size = new System.Drawing.Size(199, 20);
            this.tbServerPort.TabIndex = 7;
            this.tbServerPort.Text = global::UDPClient.Properties.Settings.Default.ServerPort;
            // 
            // tbServerIP
            // 
            this.tbServerIP.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::UDPClient.Properties.Settings.Default, "ServerIP", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.tbServerIP.Location = new System.Drawing.Point(102, 34);
            this.tbServerIP.Name = "tbServerIP";
            this.tbServerIP.Size = new System.Drawing.Size(199, 20);
            this.tbServerIP.TabIndex = 5;
            this.tbServerIP.Text = global::UDPClient.Properties.Settings.Default.ServerIP;
            // 
            // tbServerID
            // 
            this.tbServerID.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::UDPClient.Properties.Settings.Default, "ApplicationName", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.tbServerID.Location = new System.Drawing.Point(102, 8);
            this.tbServerID.Name = "tbServerID";
            this.tbServerID.ReadOnly = true;
            this.tbServerID.Size = new System.Drawing.Size(199, 20);
            this.tbServerID.TabIndex = 11;
            this.tbServerID.Text = global::UDPClient.Properties.Settings.Default.ApplicationName;
            // 
            // lblServerID
            // 
            this.lblServerID.AutoSize = true;
            this.lblServerID.Location = new System.Drawing.Point(8, 11);
            this.lblServerID.Name = "lblServerID";
            this.lblServerID.Size = new System.Drawing.Size(88, 13);
            this.lblServerID.TabIndex = 10;
            this.lblServerID.Text = "SERVER NAME:";
            // 
            // btnRegister
            // 
            this.btnRegister.Location = new System.Drawing.Point(15, 179);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(75, 51);
            this.btnRegister.TabIndex = 12;
            this.btnRegister.Text = "&Register";
            this.toolTip1.SetToolTip(this.btnRegister, "Register this client to server");
            this.btnRegister.UseVisualStyleBackColor = true;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            // 
            // btnFetchAll
            // 
            this.btnFetchAll.Location = new System.Drawing.Point(96, 179);
            this.btnFetchAll.Name = "btnFetchAll";
            this.btnFetchAll.Size = new System.Drawing.Size(75, 51);
            this.btnFetchAll.TabIndex = 13;
            this.btnFetchAll.Text = "&FetchAll";
            this.toolTip1.SetToolTip(this.btnFetchAll, "Fetch list of all hosts from server");
            this.btnFetchAll.UseVisualStyleBackColor = true;
            this.btnFetchAll.Click += new System.EventHandler(this.btnFetchAll_Click);
            // 
            // btnFetchPartner
            // 
            this.btnFetchPartner.Location = new System.Drawing.Point(177, 179);
            this.btnFetchPartner.Name = "btnFetchPartner";
            this.btnFetchPartner.Size = new System.Drawing.Size(75, 51);
            this.btnFetchPartner.TabIndex = 14;
            this.btnFetchPartner.Text = "Fetch&Part";
            this.toolTip1.SetToolTip(this.btnFetchPartner, "Fetch partner from server");
            this.btnFetchPartner.UseVisualStyleBackColor = true;
            this.btnFetchPartner.Click += new System.EventHandler(this.btnFetchPartner_Click);
            // 
            // tbMyName
            // 
            this.tbMyName.Location = new System.Drawing.Point(102, 109);
            this.tbMyName.Name = "tbMyName";
            this.tbMyName.Size = new System.Drawing.Size(94, 20);
            this.tbMyName.TabIndex = 16;
            this.tbMyName.Text = "MC-A";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 112);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "MYSELF:";
            // 
            // tbMyPartnerName
            // 
            this.tbMyPartnerName.Location = new System.Drawing.Point(102, 135);
            this.tbMyPartnerName.Name = "tbMyPartnerName";
            this.tbMyPartnerName.Size = new System.Drawing.Size(94, 20);
            this.tbMyPartnerName.TabIndex = 18;
            this.tbMyPartnerName.Text = "MC-B";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 138);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "PARTNER:";
            // 
            // btnIntroduce
            // 
            this.btnIntroduce.Location = new System.Drawing.Point(258, 179);
            this.btnIntroduce.Name = "btnIntroduce";
            this.btnIntroduce.Size = new System.Drawing.Size(75, 51);
            this.btnIntroduce.TabIndex = 19;
            this.btnIntroduce.Text = "&IntroPart";
            this.toolTip1.SetToolTip(this.btnIntroduce, "Introduce to partner");
            this.btnIntroduce.UseVisualStyleBackColor = true;
            this.btnIntroduce.Click += new System.EventHandler(this.btnIntroduce_Click);
            // 
            // btnTransmitMessage
            // 
            this.btnTransmitMessage.Location = new System.Drawing.Point(339, 179);
            this.btnTransmitMessage.Name = "btnTransmitMessage";
            this.btnTransmitMessage.Size = new System.Drawing.Size(75, 51);
            this.btnTransmitMessage.TabIndex = 20;
            this.btnTransmitMessage.Text = "&Transmit";
            this.toolTip1.SetToolTip(this.btnTransmitMessage, "Transmit Message to Partner");
            this.btnTransmitMessage.UseVisualStyleBackColor = true;
            this.btnTransmitMessage.Click += new System.EventHandler(this.btnTransmitMessage_Click);
            // 
            // frmClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(472, 242);
            this.Controls.Add(this.btnTransmitMessage);
            this.Controls.Add(this.btnIntroduce);
            this.Controls.Add(this.tbMyPartnerName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbMyName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnFetchPartner);
            this.Controls.Add(this.btnFetchAll);
            this.Controls.Add(this.btnRegister);
            this.Controls.Add(this.tbServerID);
            this.Controls.Add(this.lblServerID);
            this.Controls.Add(this.btnDisConnect);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.tbServerPort);
            this.Controls.Add(this.lblServerPort);
            this.Controls.Add(this.tbServerIP);
            this.Controls.Add(this.lblServerIP);
            this.Name = "frmClient";
            this.Text = "UDP Client";
            this.Load += new System.EventHandler(this.frmClient_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbServerPort;
        private System.Windows.Forms.Label lblServerPort;
        private System.Windows.Forms.TextBox tbServerIP;
        private System.Windows.Forms.Label lblServerIP;
        private System.Windows.Forms.Button btnDisConnect;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox tbServerID;
        private System.Windows.Forms.Label lblServerID;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Button btnFetchAll;
        private System.Windows.Forms.Button btnFetchPartner;
        private System.Windows.Forms.TextBox tbMyName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbMyPartnerName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnIntroduce;
        private System.Windows.Forms.Button btnTransmitMessage;
    }
}

