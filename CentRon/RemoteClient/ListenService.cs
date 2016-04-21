using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TravelObjects;
namespace RemoteClient
{
    public class UdpEventArgs : EventArgs { public TransferData Request = null; }


    public static class ListenService
    {
        public static event EventHandler<UdpEventArgs> UdpSent;

        public static void OnUdpSent(UdpEventArgs e)
        {
            if (UdpSent != null)
            {
                UdpSent(null, e);
            }
        }

        public static IPEndPoint serverEndPoint = null;

        public static string IsHost { get; set; }

        public static bool ServiceStarted { get; set; }
   
        public static UdpClient udpClient;

        public static NetPeer LClient;
        public static NetConnection ServerConnection;
        public static ExchangeClient Partner { get; set; }

        public static bool ConnectedToExchangeServer { get; set; }
        public static bool ConnectedToPartner { get; set; }

        private static List<NetIncomingMessage> m_readList;



        public static void CloseAllConnections()
        {

            if (LClient != null)
            {
                if (LClient.ConnectionsCount > 0)
                {
                    LClient.Shutdown("Bye");
                }

            }

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
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
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




        public static void startListen(int listenport)
        {
            //udpClient = new UdpClient(listenport);
            //udpClient.BeginReceive(new AsyncCallback(OnData), null);



            NetPeerConfiguration config = GetStandardConfiguration();

            config.Port = listenport;
            m_readList = new List<NetIncomingMessage>();
            LClient = new NetClient(config);
            LClient.Configuration.AcceptIncomingConnections = true;

            LClient.RegisterReceivedCallback(new SendOrPostCallback(OnMessageReceivedCallback));
            LClient.Start();

            //LClient.Connect("192.168.12.161", 14242, GetApproveData());
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
                case NetIncomingMessageType.StatusChanged:

                    break;
                case NetIncomingMessageType.UnconnectedData:
                    ParseData(msg);
                    break;
                case NetIncomingMessageType.NatIntroductionSuccess:

                    break;
                case NetIncomingMessageType.Data:
                    ParseData(msg);
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

            LClient.Recycle(msg);
        }

        /// <summary>
        /// Connect to server
        /// </summary>
        /// <param name="PartnerData"></param>
        /// <param name="host"></param>
        /// <param name="method">Default: NetDeliveryMethod.ReliableOrdered</param>
        public static void ConnectServer(TransferData PartnerData, IPEndPoint host, NetDeliveryMethod method = NetDeliveryMethod.ReliableOrdered)
        {


            LClient.Connect(host.Address.ToString(), host.Port);

            Thread.Sleep(500);

            int ccnt = LClient.ConnectionsCount;


            NetOutgoingMessage msg = LClient.CreateMessage();

            byte[] array = PartnerData.ToByte();


            msg.Write(array);
            ServerConnection = LClient.Connections[0];
            LClient.SendMessage(msg, LClient.Connections[0], method);


            //IPEndPoint receiver = new IPEndPoint(NetUtility.Resolve(host.Address.ToString()), host.Port);

            //LClient.SendUnconnectedMessage(msg, receiver);



        }


        public static void DisconnectServer()
        { 
            NetConnection connection = LClient.Connections.Where(t => t.RemoteEndPoint.Address.Equals(serverEndPoint.Address)).FirstOrDefault();

            if (connection != null) {
                connection.Disconnect("bye");
            }

        }


        public static void ConnectPartner(TransferData PartnerData)
        {

            bool isLocal = false;
            if (ListenService.Partner.LocalEndPoint != null)
            {
                isLocal = IsLanIP(ListenService.Partner.LocalEndPoint.Address);
            }

            try
            {
                //Data PartnerData = new Data();
                //PartnerData.cmdCommand = cmd;
                //PartnerData.strMessage = message;
                //PartnerData.strName = message;
                byte[] data = PartnerData.ToByte();      // Convert our message to a byte array

                if (isLocal)
                {
                    LClient.Connect(ListenService.Partner.LocalEndPoint.Address.ToString(), ListenService.Partner.LocalEndPoint.Port, GetApproveData());


                }
                else
                {
                    LClient.Connect(ListenService.Partner.PublicEndPoint.Address.ToString(), ListenService.Partner.PublicEndPoint.Port, GetApproveData());

                }

                //        OnUdpSent(new UdpEventArgs() { Request = PartnerData });

            }
            catch (Exception ex)
            {

            }
        }

        private static NetOutgoingMessage GetApproveData()
        {
            // create approval data
            NetOutgoingMessage approval = LClient.CreateMessage();
            approval.Write(42);
            approval.Write("secret");
            return approval;
        }

        /// <summary>
        /// Calls from multiple location, send handshake and send tile
        /// </summary>
        /// <param name="PartnerData"></param>
        /// <param name="host"></param>
        /// <param name="method">Default: NetDeliveryMethod.ReliableOrdered (But Tiles Can Change)</param>
        /// <returns></returns>
        public static bool SendLidgrenMessage(TransferData PartnerData, IPEndPoint host, NetDeliveryMethod method = NetDeliveryMethod.ReliableOrdered)
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
                connection = LClient.Connections.Where(t => t.RemoteEndPoint.Address.ToString() == host.Address.ToString()).FirstOrDefault();
            }

            if (connection == null)
            {
                LClient.Connect(host);
                Thread.Sleep(500);
                connection = LClient.Connections.Where(t => t.RemoteEndPoint.Address.ToString() == host.Address.ToString()).FirstOrDefault();
            }

            if (connection == null)
            {
                LClient.Connect(host);
                Thread.Sleep(500);
                connection = LClient.Connections.Where(t => t.RemoteEndPoint.Address.ToString() == host.Address.ToString()).FirstOrDefault();
            }

         
            if (connection != null)
            {
                LClient.SendMessage(msg, connection, method);
                retval = true;
            }
            
            return retval;

            // IPEndPoint receiver = new IPEndPoint(NetUtility.Resolve(host.Address.ToString()), host.Port);
            // LClient.SendUnconnectedMessage(msg, receiver);
        }

        private static void ParseData(NetIncomingMessage message)
        {

            string datareceived = string.Empty;

            try
            {

                if (message == null) return;
                TransferData data = new TransferData(message.ReadBytes(message.LengthBytes));

                switch (data.cmdCommand)
                {
                    case Command.Login:
                    case Command.ConnectPartner:
                    case Command.Message:
                    case Command.NULL:
                    case Command.Ping:
                        Globals.AppForm.OnData(data);
                        break;
                    default:
                        if (Globals.service != null)
                        {
                            Globals.service.OnData(data);
                        }

                        break;
                }

                //    ListenService.udpClient.BeginReceive(new AsyncCallback(OnData), null);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); Application.ExitThread(); Application.Exit();
            }


        }


        /// <summary>
        /// Lydgrn callback
        /// </summary>
        /// <param name="ar"></param>
        private static void OnData(IAsyncResult ar)
        {
            string datareceived = string.Empty;

            try
            {
                //DeSerialize To Data Object from Lyndgryn
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 80);
                byte[] received = ListenService.udpClient.EndReceive(ar, ref RemoteIpEndPoint);
                TransferData data = new TransferData(received);

                //Process Self Commands
                switch (data.cmdCommand)
                {
                    case Command.Login:
                    case Command.ConnectPartner:
                    case Command.Message:
                    case Command.NULL:
                    case Command.Ping:
                        Globals.AppForm.OnData(data);
                        break;
                    default:
                        //Peer-To-Peer (Data Recieved)
                        if (Globals.service != null)
                        {
                            Globals.service.OnData(data);
                        }
                        break;
                }

                ListenService.udpClient.BeginReceive(new AsyncCallback(OnData), null);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); Application.ExitThread(); Application.Exit();
            }

        }

        public static void  InitializeService()
        {
            ServiceStarted = true;

            if (IsHost=="yes")
            {
                Globals.service = new HostService();
            //    Globals.service.start();
                Globals.AppForm.PrintMessage("Acting as Host!");
            }
            else
            {
                Globals.service = new ClientService();

             //   Globals.service.SendSettings();

              //  Globals.service.start();
                Globals.AppForm.PrintMessage("Acting as Client!");
            }

        }

        public static IPEndPoint GetPartnerEndPoint()
        {
            IPEndPoint ep = null;

            bool isLocal = false;
            if (ListenService.Partner.LocalEndPoint != null)
            {
              //  isLocal = IsLanIP(ListenService.Partner.LocalEndPoint.Address);
            }

            try
            { 
                if (isLocal)

                {
                    ep = ListenService.Partner.LocalEndPoint;
                }
                else
                {
                    ep = ListenService.Partner.PublicEndPoint;
                }

            }
            catch (Exception ex)
            {

            }
            return ep;
        }

        public static void SendUDPMessage_OBSO(TransferData PartnerData)
        {

            bool isLocal = false;
            if (ListenService.Partner.LocalEndPoint != null)
            {
                isLocal = IsLanIP(ListenService.Partner.LocalEndPoint.Address);
            }

            try
            {
                //Data PartnerData = new Data();
                //PartnerData.cmdCommand = cmd;
                //PartnerData.strMessage = message;
                //PartnerData.strName = message;
                byte[] data = PartnerData.ToByte();      // Convert our message to a byte array

                if (isLocal)
                {
                    ListenService.udpClient.Send(data, data.Length, ListenService.Partner.LocalEndPoint);
                }
                else
                {
                    ListenService.udpClient.Send(data, data.Length, ListenService.Partner.PublicEndPoint);
                }

                //        OnUdpSent(new UdpEventArgs() { Request = PartnerData });

            }
            catch (Exception ex)
            {

            }
        }

        #region Helper Methods

        private static bool IsLanIP(IPAddress address)
        {
            bool status = false;
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var iface in interfaces)
            {
                var properties = iface.GetIPProperties();
                foreach (var ifAddr in properties.UnicastAddresses)
                {
                    if (ifAddr.IPv4Mask != null &&
                        ifAddr.Address.AddressFamily == AddressFamily.InterNetwork &&
                        CheckMask(ifAddr.Address, ifAddr.IPv4Mask, address))
                        status = true;
                }
            }


            Ping ping = new Ping();
            PingReply pingReply = ping.Send(address.ToString());

            if (pingReply.Status == IPStatus.Success)
            {
                status = true;
            }
            else
            {
                status = false;
            }




            return status;
        }

        private static bool CheckMask(IPAddress address, IPAddress mask, IPAddress target)
        {
            if (mask == null)
                return false;

            var ba = address.GetAddressBytes();
            var bm = mask.GetAddressBytes();
            var bb = target.GetAddressBytes();

            if (ba.Length != bm.Length || bm.Length != bb.Length)
                return false;

            for (var i = 0; i < ba.Length; i++)
            {
                int m = bm[i];

                int a = ba[i] & m;
                int b = bb[i] & m;

                if (a != b)
                    return false;
            }

            return true;
        }

        #endregion

    }




}
