using Lz4Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TravelObjects;
using UltraFAST;

namespace RemoteClient
{
    public partial class ScreenCapture : Form
    {

        [DllImport("gdi32.dll", EntryPoint = "BitBlt")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt([In()] System.IntPtr hdc, int x, int y, int cx, int cy, [In()] System.IntPtr hdcSrc, int x1, int y1, uint rop);

        uint SRCCOPY = 13369376;




        System.Drawing.Graphics formGraphics;
        bool locked = false;


        public ScreenCapture()
        {
            InitializeComponent();

            ////SACHIN: 05-Apr-2016 (To Maximize Performance Of Graphics)
            StuffImaging.SetupFastGraphics(this);

            ////SACHIN: 05-Apr-2016 (To Maximize Performance Of Graphics And Reduce Flicker)
            ////(https://msdn.microsoft.com/en-in/library/system.windows.forms.controlstyles%28v=vs.110%29.aspx)
            ////If true, the control ignores the window message WM_ERASEBKGND to reduce flicker. This style should only be applied if the UserPaint bit is set to true.
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            ////If true, the control paints itself rather than the operating system doing so. If false, the Paint event is not raised. This style only applies to classes derived from Control.
            this.SetStyle(ControlStyles.UserPaint, true);
            ////If true flickering is reduced on screen
            this.SetStyle(ControlStyles.DoubleBuffer, true);            
            //Some other improvements (VNCSharp - RemoteDesktop.cs)
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            
            ////Update style of the form
            this.UpdateStyles();
            
            //Original Code
            formGraphics = this.CreateGraphics();

            //A timer to load images at fast rate (10FPS) onto screen
            this.timer1.Interval = (1000 / 10);
            this.timer1.Enabled = false;
        }


        public void ImageChanged(Data data)
        {
            if (data == null || data.Image == null || data.Image.ByteArray == null)
                return;
            MemoryStream ms1 = new MemoryStream(data.Image.ByteArray);



            //  #region BitBlt



          //  Bitmap bmp = new Bitmap(ms1);


            Globals.primaryImage = (Bitmap)Image.FromStream(ms1);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //Graphics g = Graphics.FromImage(bmp);

            //IntPtr srContext = g.GetHdc();

            //IntPtr destContext = formGraphics.GetHdc();

            //////#endregion




            //BitBlt(destContext, data.Image.x, data.Image.y, bmp.Width, bmp.Height, srContext, 0, 0, SRCCOPY);


            //g.ReleaseHdc(srContext);
            //formGraphics.ReleaseHdc(destContext);



           Point Pt = new Point() { X = data.Image.x, Y = (1020 - data.Image.y - Globals.primaryImage.Height) };

           formGraphics.DrawImage(Globals.primaryImage, Pt);






            // Create point for upper-left corner of drawing.
            

            // Draw string to screen.
            //formGraphics.DrawString(drawString, drawFont, drawBrush, drawPoint);



            //Debug.WriteLine(String.Format("{0}-{1}", data.Image.x.ToString(), data.Image.y.ToString()));

            stopWatch.Stop();

            double elapsed = (double)stopWatch.ElapsedMilliseconds / 1000.0;

            Debug.WriteLine(string.Format("{0:0.000}", elapsed.ToString()));

            double fps = (1 / elapsed);




            return;


            ////textBox1.AppendText(data.strMessage + Environment.NewLine); 
            ////return;
            //this.screenBox.Dock = DockStyle.Fill;
            //BinaryFormatter bFormat = new BinaryFormatter();

            //if (data.strName == "pri")
            //{
            //    //  MemoryStream ms1 = new MemoryStream(Decompress(data.ByteArray)); 

            //    MemoryStream ms1 = new MemoryStream(data.Image.ByteArray);


            //    Globals.primaryImage = (Bitmap)Image.FromStream(ms1);

            //    //Globals.primaryImage = bFormat.Deserialize(ms1) as Bitmap;
            //}

            //if (data.strName == "inc")
            //{

            //    UpdatePrimaryImage(data);
            //}
            //this.screenBox.Image = Globals.primaryImage;
        }

        private void UpdatePrimaryImage(Data data)
        {


            if (Globals.primaryImage == null) return;

            if (data == null || data.pointers == null || data.pointers.Length <= 0 || data.Image == null || data.Image.ByteArray.Length <= 0) { return; }

            if (locked == true) { return; }

            var bmpData0 = Globals.primaryImage.LockBits(new Rectangle(0, 0, Globals.primaryImage.Width, Globals.primaryImage.Height), ImageLockMode.ReadOnly, Globals.primaryImage.PixelFormat);
            locked = true;
            int len = bmpData0.Height * bmpData0.Stride;
            byte[] data0 = new byte[len];
            Marshal.Copy(bmpData0.Scan0, data0, 0, len);

            byte[] array = Decompress(data.Image.ByteArray);

            //for (int i = 0; i < len; i++)
            //{
            int index = 0;
            foreach (var itm in data.pointers)
            {
                //    if (itm.pointer == i)
                {
                    data0[itm] = array[index];
                }

                index = index + 1;
            }
            //}
            Marshal.Copy(data0, 0, bmpData0.Scan0, len);
            Globals.primaryImage.UnlockBits(bmpData0);
            locked = false;
        }
        static byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }
        [Serializable]
        public class dojo
        {
            public List<int> pointer;
            public List<byte> data;

        }

        private void screenBox_MouseMove(object sender, MouseEventArgs e)
        {

            // SafeSendValue("M" + e.X + " " + e.Y);
            int[] arr = new int[2];
            arr[0] = e.X;
            arr[1] = e.Y;

            ListenService.SendLidgrenMessage(new Data() { cmdCommand = Command.Move, Image = new TravelImage() { ByteArray = ObjectToByteArray(arr) } }, ListenService.GetPartnerEndPoint());
        }


        byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        private void screenBox_MouseClick(object sender, MouseEventArgs e)
        {

            Data d = new Data() { cmdCommand = Command.LClick };
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                d = new Data() { cmdCommand = Command.LClick };
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                d = new Data() { cmdCommand = Command.RClick };

            ListenService.SendLidgrenMessage(d, ListenService.GetPartnerEndPoint());
        }

        private void screenBox_MouseDown(object sender, MouseEventArgs e)
        {

            Data d = new Data() { cmdCommand = Command.LDown };
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                d = new Data() { cmdCommand = Command.LDown };
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                d = new Data() { cmdCommand = Command.RDown };

            ListenService.SendLidgrenMessage(d, ListenService.GetPartnerEndPoint());
        }

        private void screenBox_MouseUp(object sender, MouseEventArgs e)
        {
            Data d = new Data() { cmdCommand = Command.LUp };
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                d = new Data() { cmdCommand = Command.LUp };
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                d = new Data() { cmdCommand = Command.RUp };

            ListenService.SendLidgrenMessage(d, ListenService.GetPartnerEndPoint());

        }

        private void ScreenCapture_FormClosing(object sender, FormClosingEventArgs e)
        {
            Globals.service.stop();
            e.Cancel = true;
            this.Hide();
        }

        private void ScreenCapture_Paint(object sender, PaintEventArgs e)
        {
            //// Create font and brush.
            //Font drawFont = new Font("Arial", 16);
            //SolidBrush drawBrush = new SolidBrush(Color.Black);
            //PointF drawPoint = new PointF(0.0F, 0.0F);
            //formGraphics.DrawString("A(0,0)", drawFont, drawBrush, drawPoint);

            //drawPoint = new PointF(0.0F, 970.0F);
            //formGraphics.DrawString("B(0,970.0F)", drawFont, drawBrush, drawPoint);

            //drawPoint = new PointF(1800.0F, 0.0F);
            //formGraphics.DrawString("C(1900,0)", drawFont, drawBrush, drawPoint);

            //drawPoint = new PointF(1900.0F, 970.0F);
            //formGraphics.DrawString("D(1900,970.0F)", drawFont, drawBrush, drawPoint);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Data d = new Data();
            d.Image = new TravelImage();
            Bitmap B = new Bitmap(@"C:\temp\imgFHDLimited.bmp");
            byte[] byt;
            using (MemoryStream ms = new MemoryStream())
            {
                B.Save(ms, ImageFormat.Bmp);
                byt = ms.ToArray();
            }

            d.Image.ByteArray = byt;

            this.ImageChanged(d);            
        }
    }
}
