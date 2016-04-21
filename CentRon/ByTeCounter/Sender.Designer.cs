namespace ByTeCounter
{
    partial class Sender
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
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtPartnerIP = new System.Windows.Forms.TextBox();
            this.txtPartnerPort = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnEndless = new System.Windows.Forms.Button();
            this.btnStopEndless = new System.Windows.Forms.Button();
            this.btnSetPort = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(86, 44);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(152, 20);
            this.txtMessage.TabIndex = 0;
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(256, 41);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(86, 23);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtPartnerIP
            // 
            this.txtPartnerIP.Location = new System.Drawing.Point(86, 12);
            this.txtPartnerIP.Name = "txtPartnerIP";
            this.txtPartnerIP.Size = new System.Drawing.Size(100, 20);
            this.txtPartnerIP.TabIndex = 2;
            this.txtPartnerIP.Text = "117.247.227.79";
            // 
            // txtPartnerPort
            // 
            this.txtPartnerPort.Location = new System.Drawing.Point(228, 12);
            this.txtPartnerPort.Name = "txtPartnerPort";
            this.txtPartnerPort.Size = new System.Drawing.Size(53, 20);
            this.txtPartnerPort.TabIndex = 3;
            this.txtPartnerPort.Text = "7891";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Partner IP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(192, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Port";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(25, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Message";
            // 
            // btnEndless
            // 
            this.btnEndless.Location = new System.Drawing.Point(256, 82);
            this.btnEndless.Name = "btnEndless";
            this.btnEndless.Size = new System.Drawing.Size(179, 23);
            this.btnEndless.TabIndex = 7;
            this.btnEndless.Text = "SendScreenShots Contineous";
            this.btnEndless.UseVisualStyleBackColor = true;
            this.btnEndless.Click += new System.EventHandler(this.btnEndless_Click);
            // 
            // btnStopEndless
            // 
            this.btnStopEndless.Location = new System.Drawing.Point(256, 123);
            this.btnStopEndless.Name = "btnStopEndless";
            this.btnStopEndless.Size = new System.Drawing.Size(179, 23);
            this.btnStopEndless.TabIndex = 8;
            this.btnStopEndless.Text = "SendScreenShots Contineous";
            this.btnStopEndless.UseVisualStyleBackColor = true;
            this.btnStopEndless.Click += new System.EventHandler(this.btnStopEndless_Click);
            // 
            // btnSetPort
            // 
            this.btnSetPort.Location = new System.Drawing.Point(296, 10);
            this.btnSetPort.Name = "btnSetPort";
            this.btnSetPort.Size = new System.Drawing.Size(108, 23);
            this.btnSetPort.TabIndex = 9;
            this.btnSetPort.Text = "Set IP Details";
            this.btnSetPort.UseVisualStyleBackColor = true;
            this.btnSetPort.Click += new System.EventHandler(this.btnSetPort_Click);
            // 
            // Sender
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(502, 332);
            this.Controls.Add(this.btnSetPort);
            this.Controls.Add(this.btnStopEndless);
            this.Controls.Add(this.btnEndless);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtPartnerPort);
            this.Controls.Add(this.txtPartnerIP);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.txtMessage);
            this.Name = "Sender";
            this.Text = "SENDER";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Sender_FormClosing);
            this.Load += new System.EventHandler(this.Sender_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox txtPartnerIP;
        private System.Windows.Forms.TextBox txtPartnerPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnEndless;
        private System.Windows.Forms.Button btnStopEndless;
        private System.Windows.Forms.Button btnSetPort;
    }
}

