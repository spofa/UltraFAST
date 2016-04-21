namespace ByTeCounter
{
    partial class Receiver
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
            this.lbldatatext = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblrate = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Message: ";
            // 
            // lbldatatext
            // 
            this.lbldatatext.AutoSize = true;
            this.lbldatatext.Location = new System.Drawing.Point(120, 52);
            this.lbldatatext.Name = "lbldatatext";
            this.lbldatatext.Size = new System.Drawing.Size(10, 13);
            this.lbldatatext.TabIndex = 1;
            this.lbldatatext.Text = " ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Frame Rate: ";
            // 
            // lblrate
            // 
            this.lblrate.AutoSize = true;
            this.lblrate.Location = new System.Drawing.Point(123, 101);
            this.lblrate.Name = "lblrate";
            this.lblrate.Size = new System.Drawing.Size(0, 13);
            this.lblrate.TabIndex = 3;
            // 
            // Receiver
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(754, 186);
            this.Controls.Add(this.lblrate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lbldatatext);
            this.Controls.Add(this.label1);
            this.Name = "Receiver";
            this.Text = "Receiver";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Receiver_FormClosing);
            this.Load += new System.EventHandler(this.Receiver_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbldatatext;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblrate;
    }
}