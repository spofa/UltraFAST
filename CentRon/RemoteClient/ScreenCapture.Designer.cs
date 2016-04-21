namespace RemoteClient
{
    partial class ScreenCapture
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
            this.tmrRateMeasure = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // tmrRateMeasure
            // 
            this.tmrRateMeasure.Enabled = true;
            this.tmrRateMeasure.Interval = 1000;
            this.tmrRateMeasure.Tick += new System.EventHandler(this.tmrRateMeasure_Tick);
            // 
            // ScreenCapture
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1062, 725);
            this.Name = "ScreenCapture";
            this.Text = "ScreenCapture";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ScreenCapture_FormClosing);
            this.Load += new System.EventHandler(this.ScreenCapture_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ScreenCapture_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.screenBox_MouseClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.screenBox_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.screenBox_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.screenBox_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer tmrRateMeasure;
    }
}