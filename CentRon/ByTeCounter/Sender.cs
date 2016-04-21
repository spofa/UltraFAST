using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TravelObjects;


namespace ByTeCounter
{
    public partial class Sender : Form
    {
        private Thread theThread;
        public static NetPeer LClient;
        private bool contineous = false;
        public int listenport = 7890;
        public IPEndPoint partnerEndPoint;

        public Sender()
        {
            InitializeComponent();
        }

        public static void startListen(int listenport)
        {

            NetPeerConfiguration config = GetStandardConfiguration();

            config.Port = listenport;// 7890;

            LClient = new NetClient(config);
            LClient.Configuration.AcceptIncomingConnections = true;

            LClient.RegisterReceivedCallback(new SendOrPostCallback(OnMessageReceivedCallback));
            LClient.Start();

            //LClient.Connect("192.168.12.161", 14242, GetApproveData());
        }

        public static NetPeerConfiguration GetStandardConfiguration()
        {
            var config = new NetPeerConfiguration("ImageTransfer");
            // Disable all message types
            config.DisableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.DisableMessageType(NetIncomingMessageType.ConnectionLatencyUpdated);
            config.DisableMessageType(NetIncomingMessageType.Data);
            config.DisableMessageType(NetIncomingMessageType.DebugMessage);
            config.DisableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.DisableMessageType(NetIncomingMessageType.DiscoveryResponse);
            config.DisableMessageType(NetIncomingMessageType.Error);
            config.DisableMessageType(NetIncomingMessageType.ErrorMessage);
            config.DisableMessageType(NetIncomingMessageType.NatIntroductionSuccess);
            config.DisableMessageType(NetIncomingMessageType.Receipt);
            config.DisableMessageType(NetIncomingMessageType.UnconnectedData);
            config.DisableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.DisableMessageType(NetIncomingMessageType.WarningMessage);
            // Enable only what we need
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            config.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);
            config.EnableMessageType(NetIncomingMessageType.StatusChanged);
            //config.EnableMessageType(NetIncomingMessageType.DebugMessage);
            //config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.WarningMessage);
            config.EnableMessageType(NetIncomingMessageType.ErrorMessage);
            config.AcceptIncomingConnections = true;
            // No need to assign a port, as the OS will automatically assign an available port
            return config;
        }

        private static void OnMessageReceivedCallback(object netPeerObject)
        {
            // It could be a library message
            var msg = LClient.ReadMessage();

            switch (msg.MessageType)
            {
                case NetIncomingMessageType.ConnectionApproval:
                    msg.SenderConnection.Approve();
                    break;

                case NetIncomingMessageType.UnconnectedData:
                    MessageBox.Show("Data arrived!");
                    // ParseData(msg);
                    break;
                case NetIncomingMessageType.Data:
                    MessageBox.Show("Data arrived!");
                    // ParseData(msg);
                    break;
            }


        }

        private void connect()
        {
            LClient.Connect("IP", 0, GetApproveData());

        }
        private static NetOutgoingMessage GetApproveData()
        {
            // create approval data
            NetOutgoingMessage approval = LClient.CreateMessage();
            approval.Write(42);
            approval.Write("secret");
            return approval;
        }

        public static bool SendLidgrenMessage(TransferData PartnerData, IPEndPoint host)
        {
            bool retval = false;

            NetOutgoingMessage msg = LClient.CreateMessage();
            byte[] array = PartnerData.ToByte();
            msg.Write(array);

            NetConnection connection = LClient.Connections.Where(t => t.RemoteEndPoint.Address.ToString() == host.Address.ToString()).FirstOrDefault();

            if (connection == null)
            {
                LClient.Connect(host);
                Thread.Sleep(500);
            }

            if (connection == null)
            {
                LClient.Connect(host);
                Thread.Sleep(500);
            }

            if (connection == null)
            {
                LClient.Connect(host);
                Thread.Sleep(500);
            }

            connection = LClient.Connections.Where(t => t.RemoteEndPoint.Address.ToString() == host.Address.ToString()).FirstOrDefault();

            if (connection != null)
            {
                LClient.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
                retval = true;
            }

            return retval;

            // IPEndPoint receiver = new IPEndPoint(NetUtility.Resolve(host.Address.ToString()), host.Port);
            // LClient.SendUnconnectedMessage(msg, receiver);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (partnerEndPoint == null) return;
             
            SendLidgrenMessage(new TransferData() { }, partnerEndPoint);

        }

        private IPEndPoint GetIPEndPoint()
        {

            IPAddress serverIP = IPAddress.Parse(txtPartnerIP.Text);
            int port = 80;
            int.TryParse(txtPartnerPort.Text, out port);
            return new IPEndPoint(serverIP, port);


        }

        private void Sender_Load(object sender, EventArgs e)
        {
            startListen(listenport);
        }

        private void btnEndless_Click(object sender, EventArgs e)
        {

            contineous = true;
            start();
        }

        public void start()
        {

            if (partnerEndPoint == null) return;

            if (theThread == null)
            {
                theThread = new Thread(new ThreadStart(SendEndless));
                theThread.Start();
            }
        }

        private void SendEndless()
        {
            while (contineous)
            {
                //  Bitmap bmp = CaptureWithWin32API.CaptureFrames();
                //byte[] arr = CaptureWithWin32API.ImageToByte2(bmp);
               
                SendLidgrenMessage(new TransferData() { strMessage = DateTime.Now.ToString(), ImgDATA = new TravelImage() { } }, partnerEndPoint);

            }

        }

        private void btnStopEndless_Click(object sender, EventArgs e)
        {
            contineous = false;
            theThread.Abort();

        }

        private void Sender_FormClosing(object sender, FormClosingEventArgs e)
        {
            LClient.Shutdown("bye");
        }

        private void btnSetPort_Click(object sender, EventArgs e)
        {
            partnerEndPoint = GetIPEndPoint();
        }
    }
}
