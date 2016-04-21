using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
    public partial class Receiver : Form
    {
        public int listenport = 7891;
        public   NetPeer LClient;
        public bool receivingdata = false;
        Stopwatch stopWatch = new Stopwatch();
        public int datareceived = 0;
        public Receiver()
        {
            InitializeComponent();
        }

        public   void startListen(int listenport)
        {

            NetPeerConfiguration config = GetStandardConfiguration();

            config.Port = listenport;

            LClient = new NetClient(config);
            LClient.Configuration.AcceptIncomingConnections = true;
            
            LClient.RegisterReceivedCallback(new SendOrPostCallback(OnMessageReceivedCallback));
            LClient.Start();

            //LClient.Connect("192.168.12.161", 14242, GetApproveData());
        }

        public   NetPeerConfiguration GetStandardConfiguration()
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

        private   void OnMessageReceivedCallback(object netPeerObject)
        {
            // It could be a library message
            var msg = LClient.ReadMessage();



            switch (msg.MessageType)
            {
                case NetIncomingMessageType.ConnectionApproval:
                    msg.SenderConnection.Approve();
                    break;

                case NetIncomingMessageType.UnconnectedData:
                    //MessageBox.Show("Data arrived!");
                    // ParseData(msg);
                    break;
                case NetIncomingMessageType.Data:

                    if (!receivingdata) { receivingdata = true; stopWatch.Start(); }
                    
                    datareceived = datareceived + msg.LengthBytes;
                    lbldatatext.Text = " Total Bytes Received : " + datareceived.ToString();

                    long elapsed = (long)stopWatch.ElapsedMilliseconds / 1000;

                    long totalkb = datareceived / 1000;

                    if (totalkb >= 1)
                    {
                        long speed = totalkb / elapsed;
                        lblrate.Text = totalkb.ToString();
                        try {
                            stopWatch.Stop();
                             stopWatch.Reset();
                             stopWatch.Start();
                        }
                        catch (Exception ex) { }
                      //  receivingdata = false;
                        datareceived = 0;
                    }
                     
                    //MessageBox.Show("Data arrived!");
                    // ParseData(msg);
                    break;
            }


        }

        private void connect()
        {
            LClient.Connect("IP", 0, GetApproveData());

        }
        private   NetOutgoingMessage GetApproveData()
        {
            // create approval data
            NetOutgoingMessage approval = LClient.CreateMessage();
            approval.Write(42);
            approval.Write("secret");
            return approval;
        }

        public   bool SendLidgrenMessage(TransferData PartnerData, IPEndPoint host)
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

        private void Receiver_Load(object sender, EventArgs e)
        {
            startListen(listenport);
        }

        private void Receiver_FormClosing(object sender, FormClosingEventArgs e)
        {
            LClient.Shutdown("bye");
        }
    }
}
