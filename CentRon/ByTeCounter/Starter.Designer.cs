namespace ByTeCounter
{
    partial class Starter
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
            this.Peer1 = new System.Windows.Forms.Button();
            this.Peer2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Peer1
            // 
            this.Peer1.Location = new System.Drawing.Point(33, 13);
            this.Peer1.Name = "Peer1";
            this.Peer1.Size = new System.Drawing.Size(75, 23);
            this.Peer1.TabIndex = 0;
            this.Peer1.Text = "Sender";
            this.Peer1.UseVisualStyleBackColor = true;
            this.Peer1.Click += new System.EventHandler(this.Peer1_Click);
            // 
            // Peer2
            // 
            this.Peer2.Location = new System.Drawing.Point(211, 13);
            this.Peer2.Name = "Peer2";
            this.Peer2.Size = new System.Drawing.Size(75, 23);
            this.Peer2.TabIndex = 1;
            this.Peer2.Text = "Receiver";
            this.Peer2.UseVisualStyleBackColor = true;
            this.Peer2.Click += new System.EventHandler(this.Peer2_Click);
            // 
            // Starter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(646, 233);
            this.Controls.Add(this.Peer2);
            this.Controls.Add(this.Peer1);
            this.Name = "Starter";
            this.Text = "Starter";
            this.Load += new System.EventHandler(this.Starter_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Peer1;
        private System.Windows.Forms.Button Peer2;
    }
}