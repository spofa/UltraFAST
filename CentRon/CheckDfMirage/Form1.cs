
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckDfMirage
{
    public partial class Form1 : Form
    {
        Bitmap prev, curr;
        public static byte[] ImageData;
        public static int ImageWidth, ImageHeight;
        public static NetServer Server;
        public static NetPeer netPeer;
        public Form1()
        {
            InitializeComponent();

            NetPeerConfiguration config = new NetPeerConfiguration("ImageTransfer");
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.EnableMessageType(NetIncomingMessageType.DebugMessage);
            config.AutoExpandMTU = true;
            // listen on port 14242
            config.Port = 14242;
            Server = new NetServer(config);

            


            NetPeerConfiguration config1 = new NetPeerConfiguration("N/A");
            config1.Port = 14242;
            config1.AcceptIncomingConnections = true;
            netPeer = new NetPeer(config1);
            netPeer.RegisterReceivedCallback(OnMessageReceivedCallback);


            //Application.Idle += new EventHandler(AppLoop);
        }

        private void OnMessageReceivedCallback(object netPeerObject)
        {
            // It could be a library message
            var incomingMessage = netPeer.ReadMessage();

            switch (incomingMessage.MessageType)
            {
                case NetIncomingMessageType.StatusChanged:

                    break;
                case NetIncomingMessageType.UnconnectedData:

                    break;
                case NetIncomingMessageType.NatIntroductionSuccess:

                    break;
                case NetIncomingMessageType.Data:

                    break;
                case NetIncomingMessageType.DebugMessage:

                    break;
                case NetIncomingMessageType.VerboseDebugMessage:

                    break;
                case NetIncomingMessageType.WarningMessage:

                    break;
                case NetIncomingMessageType.ErrorMessage:

                    break;
            }

            netPeer.Recycle(incomingMessage);
        }

        static void AppLoop(object sender, EventArgs e)
        {
            NetIncomingMessage inc;

            while (NativeMethods.AppStillIdle)
            {
                // read any pending messages
                while ((inc = Server.WaitMessage(100)) != null)
                {
                    switch (inc.MessageType)
                    {
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.VerboseDebugMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.ErrorMessage:
                            // just print any message

                            break;
                        case NetIncomingMessageType.DiscoveryRequest:
                            NetOutgoingMessage dom = Server.CreateMessage();
                            dom.Write("Kokosboll");
                            Server.SendDiscoveryResponse(dom, inc.SenderEndPoint);
                            break;
                        case NetIncomingMessageType.ConnectionApproval:

                            // Here we could check inc.SenderConnection.RemoteEndPoint, deny certain ip

                            // check hail data
                            try
                            {
                                int a = inc.ReadInt32();
                                string s = inc.ReadString();

                                if (a == 42 && s == "secret")
                                    inc.SenderConnection.Approve();
                                else
                                    inc.SenderConnection.Deny("Bad approve data, go away!");
                            }
                            catch (NetException)
                            {
                                inc.SenderConnection.Deny("Bad approve data, go away!");
                            }
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            NetConnectionStatus status = (NetConnectionStatus)inc.ReadByte();

                            if (status == NetConnectionStatus.Connected)
                            {

                                NetOutgoingMessage om = Server.CreateMessage(ImageData.Length + 5);

                                om.Write((ushort)ImageWidth);
                                om.Write((ushort)ImageHeight);
                                om.Write((int)ImageData.Length);
                                om.Write(ImageData);

                                Image x = (Bitmap)((new ImageConverter()).ConvertFrom(ImageData));


                                string filename = "D:\\Temp\\Juggler" + ImageData.Length + ".jpg";

                                if (File.Exists(filename))
                                {

                                    File.Delete(filename);
                                }

                                x.Save(filename, ImageFormat.Jpeg);


                                Server.SendMessage(om, inc.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);

                                // all messages will be sent before disconnect so we can call it here
                                // inc.SenderConnection.Disconnect("Bye bye now");
                            }

                            if (status == NetConnectionStatus.Disconnected)
                            {

                            }

                            break;
                    }

                    // recycle message to avoid garbage
                    Server.Recycle(inc);
                }
            }
        }

        public void Start(Bitmap file)
        {
            if (Server.Status != NetPeerStatus.NotRunning)
            {
                Server.Shutdown("Restarting");
                System.Threading.Thread.Sleep(100);
            }

            Server.Start();


            ImageConverter converter = new ImageConverter();
            ImageData = (byte[])converter.ConvertTo(file, typeof(byte[]));
            return;

            // get image size
            //Bitmap bm = file;// Bitmap.FromFile(filename) as Bitmap;
            //ImageWidth = bm.Width;
            //ImageHeight = bm.Height;

            //// extract color bytes
            //// very slow method, but small code size
            //ImageData = new byte[3 * ImageWidth * ImageHeight];
            //int ptr = 0;
            //for (int y = 0; y < ImageHeight; y++)
            //{
            //    for (int x = 0; x < ImageWidth; x++)
            //    {
            //        Color color = bm.GetPixel(x, y);
            //        ImageData[ptr++] = color.R;
            //        ImageData[ptr++] = color.G;
            //        ImageData[ptr++] = color.B;
            //    }
            //}

            //bm.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // prev = screenshot();



            prev = new Bitmap(Image.FromFile("D:\\sc1.png"));

            this.pictureBox1.Image = prev;
        }

        public Bitmap screenshot()
        {

            Bitmap shot = new Bitmap(SystemInformation.VirtualScreen.Width,
                 SystemInformation.VirtualScreen.Height,
                 PixelFormat.Format24bppRgb);
            Graphics screenGraph = Graphics.FromImage(shot);
            screenGraph.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                       Screen.PrimaryScreen.Bounds.Y,
                                       0,
                                       0,
                                       SystemInformation.VirtualScreen.Size,
                                       CopyPixelOperation.SourceCopy);

            return shot;

        }

        public Bitmap Difference(Bitmap bmp0, Bitmap bmp1)
        {
            Bitmap bmp2;
            int Bpp = bmp0.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 4;

            bmp2 = new Bitmap(bmp0.Width, bmp0.Height, bmp0.PixelFormat);

            var bmpData0 = bmp0.LockBits(
                            new Rectangle(0, 0, bmp0.Width, bmp0.Height),
                            ImageLockMode.ReadOnly, bmp0.PixelFormat);
            var bmpData1 = bmp1.LockBits(
                            new Rectangle(0, 0, bmp1.Width, bmp1.Height),
                            ImageLockMode.ReadOnly, bmp1.PixelFormat);
            var bmpData2 = bmp2.LockBits(
                            new Rectangle(0, 0, bmp2.Width, bmp2.Height),
                            ImageLockMode.ReadWrite, bmp2.PixelFormat);

            // MessageBox.Show(bmpData0.Stride.ToString());
            int len = bmpData0.Height * bmpData0.Stride;

            //   MessageBox.Show(bmpData0.Stride.ToString());
            bool changed = false;

            byte[] data0 = new byte[len];
            byte[] data1 = new byte[len];
            byte[] data2 = new byte[len];

            Marshal.Copy(bmpData0.Scan0, data0, 0, len);
            Marshal.Copy(bmpData1.Scan0, data1, 0, len);
            Marshal.Copy(bmpData2.Scan0, data2, 0, len);

            for (int i = 0; i < len; i += Bpp)
            {
                changed = ((data0[i] != data1[i])
                              || (data0[i + 1] != data1[i + 1])
                              || (data0[i + 2] != data1[i + 2]));

                // this.Invoke(new Action(() => this.Text = changed.ToString()));

                data2[i] = changed ? data1[i] : (byte)2;   // special markers
                data2[i + 1] = changed ? data1[i + 1] : (byte)3;   // special markers
                data2[i + 2] = changed ? data1[i + 2] : (byte)7;   // special markers

                if (Bpp == 4)
                    data2[i + 3] = changed ? (byte)255 : (byte)42;  // special markers
            }

            //  this.Invoke(new Action(() => this.Text = changed.ToString()));
            Marshal.Copy(data2, 0, bmpData2.Scan0, len);
            bmp0.UnlockBits(bmpData0);
            bmp1.UnlockBits(bmpData1);
            bmp2.UnlockBits(bmpData2);

            return bmp2;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // curr = screenshot();
            curr = new Bitmap(Image.FromFile("D:\\sc2.png"));
            this.pictureBox1.Image = curr;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<dojo> fck = Difference1(prev, curr);

            var bmpData0 = prev.LockBits(new Rectangle(0, 0, prev.Width, prev.Height), ImageLockMode.ReadOnly, prev.PixelFormat);
            int len = bmpData0.Height * bmpData0.Stride;
            byte[] data0 = new byte[len];
            Marshal.Copy(bmpData0.Scan0, data0, 0, len);

            //for (int i = 0; i < len; i++)
            //{


            foreach (var itm in fck)
            {
                //    if (itm.pointer == i)
                {
                    data0[itm.pointer] = itm.data;
                }
            }
            //}
            Marshal.Copy(data0, 0, bmpData0.Scan0, len);
            prev.UnlockBits(bmpData0);


            this.pictureBox1.Image = prev;

            Start(prev);
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        public List<dojo> Difference1(Bitmap bmp0, Bitmap bmp1)
        {
            Bitmap bmp2;
            int Bpp = bmp0.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 4;

            bmp2 = new Bitmap(bmp0.Width, bmp0.Height, bmp0.PixelFormat);

            var bmpData0 = bmp0.LockBits(
                            new Rectangle(0, 0, bmp0.Width, bmp0.Height),
                            ImageLockMode.ReadOnly, bmp0.PixelFormat);
            var bmpData1 = bmp1.LockBits(
                            new Rectangle(0, 0, bmp1.Width, bmp1.Height),
                            ImageLockMode.ReadOnly, bmp1.PixelFormat);
            var bmpData2 = bmp2.LockBits(
                            new Rectangle(0, 0, bmp2.Width, bmp2.Height),
                            ImageLockMode.ReadWrite, bmp2.PixelFormat);

            // MessageBox.Show(bmpData0.Stride.ToString());
            int len = bmpData0.Height * bmpData0.Stride;

            int len1 = bmpData1.Height * bmpData1.Stride;


            //   MessageBox.Show(bmpData0.Stride.ToString());
            bool changed = false;

            byte[] data0 = new byte[len];
            byte[] data1 = new byte[len];
            byte[] data2 = new byte[len];

            Marshal.Copy(bmpData0.Scan0, data0, 0, len);
            Marshal.Copy(bmpData1.Scan0, data1, 0, len);
            Marshal.Copy(bmpData2.Scan0, data2, 0, len);

            List<dojo> fck = new List<dojo>();

            for (int i = 0; i < len; i++)
            {
                if (data0[i] != data1[i])
                {
                    fck.Add(new dojo { pointer = i, data = data1[i] });
                }
            }

            //  this.Invoke(new Action(() => this.Text = changed.ToString()));
            Marshal.Copy(data2, 0, bmpData2.Scan0, len);
            bmp0.UnlockBits(bmpData0);
            bmp1.UnlockBits(bmpData1);
            bmp2.UnlockBits(bmpData2);

            return fck;
        }

    }

    public class dojo
    {
        public int pointer;
        public byte data;

    }
}
