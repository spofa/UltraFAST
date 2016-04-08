using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageGrabber
{
    public partial class Form1 : Form
    {
        public NetClient Client;
        private List<NetIncomingMessage> m_readList;
        public static NetPeer netPeer;
        public Form1()
        {
            InitializeComponent();
            //NetPeerConfiguration m_config = new NetPeerConfiguration("ImageTransfer");
            //NetPeerConfiguration config = m_config.Clone();
            //config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            //config.EnableMessageType(NetIncomingMessageType.DebugMessage);
            //m_readList = new List<NetIncomingMessage>();
            //Client = new NetClient(config);
            //Client.Start();
            //Client.Connect("192.168.12.161", 14242, GetApproveData());
            //try
            //{
            //    Application.Idle += new EventHandler(AppLoop);

            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Ouch: " + ex);
            //}


            NetPeerConfiguration config1 = new NetPeerConfiguration("N/A");
            config1.Port = 14243;
            config1.AcceptIncomingConnections = true;
            netPeer = new NetPeer(config1);
            netPeer.RegisterReceivedCallback(OnMessageReceivedCallback);
             
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


        private NetOutgoingMessage GetApproveData()
        {
            // create approval data
            NetOutgoingMessage approval = Client.CreateMessage();
            approval.Write(42);
            approval.Write("secret");
            return approval;
        }

        void AppLoop(object sender, EventArgs e)
        {
            while (NativeMethods.AppStillIdle)
            {
                Heartbeat();
            }
        }

        public void Heartbeat()
        {
            int numRead = Client.ReadMessages(m_readList);
            if (numRead < 1)
                return;

            foreach (var inc in m_readList)
            {
                switch (inc.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryResponse:
                        // found server! just connect...
                        string serverResponseHello = inc.ReadString();
                        //  Client.Connect(inc.SenderEndPoint, GetApproveData());
                        break;
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        string str = inc.ReadString();

                        //System.IO.File.AppendAllText("C:\\tmp\\clientlog.txt", str + Environment.NewLine);
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)inc.ReadByte();
                        string reason = inc.ReadString();

                        if (status == NetConnectionStatus.Connected)
                        {

                        }
                        break;
                    case NetIncomingMessageType.Data:

                        // image data, whee!
                        // ineffective but simple data model
                        ushort width = inc.ReadUInt16();
                        ushort height = inc.ReadUInt16();
                        int length = inc.ReadInt32();

                        byte[] imageArray = inc.ReadBytes(2833298);


                        Image x = (Bitmap)((new ImageConverter()).ConvertFrom(imageArray));


                        //string filename = "D:\\Temp\\Juggler1-" + length + ".jpg";

                        //if (File.Exists(filename))
                        //{

                        //    File.Delete(filename);
                        //}

                        //x.Save(filename, ImageFormat.Jpeg);

                        
                        pictureBox1.Image = x;

                        System.Threading.Thread.Sleep(0);

                        break;
                }
            }

            // recycle messages to avoid garbage
            Client.Recycle(m_readList);
            m_readList.Clear();
        }


    }
}
