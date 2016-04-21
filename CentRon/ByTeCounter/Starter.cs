using ImageProcessor.Imaging.Quantizers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ByTeCounter
{
    public partial class Starter : Form
    {
        public Starter()
        {
            InitializeComponent();
        }

        private void Peer1_Click(object sender, EventArgs e)
        {
            Stopwatch S = new Stopwatch();
            Bitmap quantized = null;

            using (Bitmap image = new Bitmap(@"C:\temp\Capture.jpg", true))
            {
                S.Start();
                OctreeQuantizer quantizer = new OctreeQuantizer(255, 8);
                quantized = quantizer.Quantize(image);
                S.Stop();

                quantized.Save(@"C:\temp\quant.gif", ImageFormat.Gif);
            }

            long timesl = S.ElapsedMilliseconds;
            //Image img = Image.FromFile(@"D:\imgGDICapture.bmp", true);

            //string sd = string.Empty;


            //Bitmap pp = Program.ConvertTo8bpp(img);
            //pp.Save(@"C:\temp\imgGDICapture1.png");

            Peer2.Enabled = false;
            Sender s = new Sender();
          
            s.ShowDialog();
           
        }

        private void Peer2_Click(object sender, EventArgs e)
        {
            try {
                Peer1.Enabled = false;
                Receiver r = new Receiver();
                r.ShowDialog();
            }
            catch (Exception ex) { }
           

        }

        private void Starter_Load(object sender, EventArgs e)
        {

        }
    }
}
