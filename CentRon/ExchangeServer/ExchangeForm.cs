using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using tcpServer;
using TravelObjects;

namespace ExchangeServer
{
    public partial class ExchangeForm : Form
    {

        public delegate void invokeDelegate();
        TcpServer tcpServer1 = new TcpServer();
        int tcpPort = 3002;

        public static NetPeer LClient;
        private static List<NetIncomingMessage> m_readList;

        //UdpClient udpListener = new UdpClient(161);

        delegate void SetListCallback(ListBox tb, IList<RemoteClient> client);
        delegate void UpdateListCallback(ListBox tb);

        private void SetList(ListBox tb, IList<RemoteClient> client)
        {
            if (tb.InvokeRequired)
            {
                SetListCallback d = new SetListCallback(SetList);
                this.Invoke(d, new object[] { tb, client });
            }
            else
            {

                lstClients.Items.Clear();

                foreach (RemoteClient c in client)
                {
                    lstClients.Items.Add(c);
                }
            }
        }

        List<RemoteClient> Clients;


        public static NetPeerConfiguration GetStandardConfiguration()
        {
            var config = new NetPeerConfiguration("NovaRat");
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
            config.EnableMessageType(NetIncomingMessageType.Data);
            //config.EnableMessageType(NetIncomingMessageType.DebugMessage); 
            //config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.WarningMessage);
            config.EnableMessageType(NetIncomingMessageType.ErrorMessage);
            config.AcceptIncomingConnections = true;
            // No need to assign a port, as the OS will automatically assign an available port
            return config;
        }



        public ExchangeForm()
        {
            InitializeComponent();
            Clients = new List<RemoteClient>();

            lstClients.DisplayMember = "Display";
            lstClients.ValueMember = "IPAddress";

            //UdpState state = new UdpState();

            //IPEndPoint IEP = new IPEndPoint(IPAddress.Any, 80);
            //state.e = IEP;
            //state.u = udpListener;
            //udpListener.BeginReceive(new AsyncCallback(DataReceived), state);

            NetPeerConfiguration m_config = new NetPeerConfiguration("ImageTransfer");
            NetPeerConfiguration config = m_config.Clone();
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            config.EnableMessageType(NetIncomingMessageType.DebugMessage);
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.AcceptIncomingConnections = true;
            config.SetMessageTypeEnabled(NetIncomingMessageType.UnconnectedData, false);

            config.Port = 161;

            m_readList = new List<NetIncomingMessage>();

            LClient = new NetClient(config);
            LClient.Configuration.AcceptIncomingConnections = true;



            LClient.RegisterReceivedCallback(new SendOrPostCallback(OnMessageReceivedCallback));

            //  LClient.RegisterReceivedCallback(OnMessageReceivedCallback);
            LClient.Start();


            tcpServer1.OnDataAvailable += TcpServer1_OnDataAvailable;
            tcpServer1.OnConnect += TcpServer1_OnConnect;
            openTcpPort(tcpPort);
        }
        private void openTcpPort(int port)
        {
            tcpServer1.Port = port;
            tcpServer1.Open();
        }



        private void OnMessageReceivedCallback(object netPeerObject)
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

                    //  Data data = new Data(msg.ReadBytes(msg.LengthBytes));

                  

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


        private void TcpServer1_OnConnect(tcpServer.TcpServerConnection connection)
        {

            string clientIPAddress = IPAddress.Parse(((IPEndPoint)connection.Socket.Client.RemoteEndPoint).Address.ToString()).ToString();

            string clientPort = ((IPEndPoint)connection.Socket.Client.RemoteEndPoint).Port.ToString();

            //   MessageBox.Show("Message form " + clientIPAddress + ":" + clientPort);

        }
        private void TcpServer1_OnDataAvailable(tcpServer.TcpServerConnection connection)
        {

            string clientIPAddress = IPAddress.Parse(((IPEndPoint)connection.Socket.Client.RemoteEndPoint).Address.ToString()).ToString();

            string clientPort = ((IPEndPoint)connection.Socket.Client.RemoteEndPoint).Port.ToString();

            //   MessageBox.Show("Message form " + clientIPAddress + ":" + clientPort);

            byte[] data = readStream(connection.Socket);

            Data bdata = new Data(data);


            if (bdata != null)
            {
                string dataStr = Encoding.ASCII.GetString(data);

                invokeDelegate del = () =>
                {
                    handleTCPInput(bdata, connection);
                };
                Invoke(del);

                data = null;
            }
        }
        protected byte[] readStream(TcpClient client)
        {
            int buffsize = client.Available;

            byte[] data = new byte[buffsize];

            NetworkStream stream = client.GetStream();

            //while (stream.DataAvailable)
            //{

            int recv = stream.Read(data, 0, data.Length);
            //call stream.Read(), read until end of packet/stream/other termination indicator
            //return data read as byte array
            //}
            return data;
        }
        private void handleTCPInput(Data data, tcpServer.TcpServerConnection connection)
        {
            IPEndPoint RemoteIpEndPoint = (IPEndPoint)connection.Socket.Client.RemoteEndPoint;


            switch (data.cmdCommand)
            {
                case Command.Login:
                    Random random = new Random();
                    int newid = random.Next(1000, 100000000);

                    RemoteClient rc = new RemoteClient() { IPAddress = RemoteIpEndPoint.Address.ToString(), Port = RemoteIpEndPoint.Port.ToString(), RemoteID = newid.ToString() };
                    Clients.Add(rc);
                    Data LoginData = new Data();
                    LoginData.cmdCommand = Command.Login;
                    LoginData.strMessage = "LoggedIn";
                    LoginData.strName = rc.RemoteID;
                    byte[] response = LoginData.ToByte();
                    UpdateList(lstClients);

                    byte[] ldata = LoginData.ToByte();

                    connection.Socket.GetStream().Write(ldata, 0, ldata.Length);
                    break;


                case Command.ConnectPartner:
                    RemoteClient partnerclient = Clients.Where(t => t.RemoteID == data.strName).FirstOrDefault();
                    Data PartnerData = new Data();
                    if (partnerclient != null)
                    {

                        PartnerData.cmdCommand = Command.Ping;
                        PartnerData.strMessage = partnerclient.Display;
                        byte[] data1 = PartnerData.ToByte();
                        connection.Socket.GetStream().Write(data1, 0, data1.Length);


                        RemoteClient myremoteclient = new RemoteClient() { IPAddress = RemoteIpEndPoint.Address.ToString(), Port = RemoteIpEndPoint.Port.ToString() };
                        RemoteClient myclient = new RemoteClient() { IPAddress = myremoteclient.IPAddress, Port = myremoteclient.Port };
                        if (myclient != null)
                        {
                            myremoteclient.LocalIPAddress = myclient.LocalIPAddress;
                            myremoteclient.LocalPort = myclient.LocalPort;
                        }

                        Data Data = new Data();
                        Data.cmdCommand = Command.Ping;
                        Data.strMessage = myclient.Display;
                        byte[] data2 = Data.ToByte();

                        tcpServer1.Connections[1].Socket.GetStream().Write(data2, 0, data2.Length);


                        //   PingToPartner(partnerclient.IPEndPoint, myclient); 


                    }

                    break;



            }







        }


        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void UpdateList(ListBox tb)
        {
            if (tb.InvokeRequired)
            {
                UpdateListCallback d = new UpdateListCallback(UpdateList);
                this.Invoke(d, new object[] { tb });
            }
            else
            {

                lstClients.Items.Clear();

                foreach (RemoteClient client in Clients)
                {
                    lstClients.Items.Add(client);
                }
            }



        }


        public static void SendLidgrenMessage(Data PartnerData, NetConnection connection)
        {
            NetOutgoingMessage msg = LClient.CreateMessage();

            byte[] array = PartnerData.ToByte();


            msg.Write(array);

            //IPEndPoint receiver = new IPEndPoint(NetUtility.Resolve(host.Address.ToString()), host.Port);




            LClient.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);

            //LClient.SendUnconnectedMessage(msg, receiver);



        }

        public static void SendLidgrenMessage(Data PartnerData, IPEndPoint  host)
        {
            NetOutgoingMessage msg = LClient.CreateMessage();

            byte[] array = PartnerData.ToByte(); 
            msg.Write(array);

           IPEndPoint receiver = new IPEndPoint(NetUtility.Resolve(host.Address.ToString()), host.Port); 

           LClient.SendUnconnectedMessage(msg, receiver); 
        }




        private void ParseData(NetIncomingMessage message)
        {
            if (message == null) return;
            Data data = new Data(message.ReadBytes(message.LengthBytes));


            switch (data.cmdCommand)
            {
                case Command.Login:
                    Random random = new Random();
                    int newid = random.Next(1000, 100000000);

                    RemoteClient rc = new RemoteClient()
                    {
                        IPAddress = message.SenderEndPoint.Address.ToString(),
                        Port = message.SenderEndPoint.Port.ToString(),
                        RemoteID = newid.ToString(),
                        LocalIPAddress = data.IPAddress,
                        LocalPort = data.port
                    };
                    Clients.Add(rc);
                    Data LoginData = new Data();
                    LoginData.cmdCommand = Command.Login;
                    LoginData.strMessage = "LoggedIn";
                    LoginData.strName = rc.RemoteID;
                    byte[] response = LoginData.ToByte();
                    UpdateList(lstClients);

                    SendLidgrenMessage(LoginData,  message.SenderConnection);
                    // udpListener.Send(response, response.Length, RemoteIpEndPoint);
                    break;
                case Command.ConnectPartner: // request from one who is willing to see remote connection
                    RemoteClient partnerclient = Clients.Where(t => t.RemoteID == data.strName).FirstOrDefault();

                    RemoteClient myclient = Clients.Where(t => t.RemoteID == data.strMessage).FirstOrDefault();

                    if (partnerclient != null)
                    {


                        Data Data = new Data();
                        Data.cmdCommand = Command.Ping;
                        Data.strMessage = partnerclient.Display;
                        Data.strName = "client";
                        byte[] data1 = Data.ToByte();
                         SendLidgrenMessage(Data, message.SenderConnection);
                        // udpListener.Send(data1, data1.Length, RemoteIpEndPoint);  // Send the Remote Client application The IP data of RemoteHost
                        Thread.Sleep(8000);


                        RemoteClient myremoteclient = new RemoteClient() { IPAddress = message.SenderEndPoint.Address.ToString(), Port = message.SenderEndPoint.Port.ToString() };
                        if (myclient != null)
                        {
                            myremoteclient.LocalIPAddress = myclient.LocalIPAddress;
                            myremoteclient.LocalPort = myclient.LocalPort;
                        }

                        // Sending Host the IP of remote client , so that Host can also know public IP of its client. 
                        Data NewData = new Data();
                        NewData.cmdCommand = Command.Ping;
                        NewData.strMessage = myremoteclient.Display;
                        NewData.strName = "host";
                        byte[] ndata = NewData.ToByte();
                        NetConnection connect = LClient.Connections.Where(t => t.RemoteEndPoint.Address.Equals(partnerclient.IPEndPoint)).FirstOrDefault();
                         SendLidgrenMessage(NewData, partnerclient.IPEndPoint);
                        //udpListener.Send(ndata, ndata.Length, partnerclient.IPEndPoint);

                    }
                    break;
            }




        }


        //private void DataReceived(IAsyncResult ar)
        //{

        //    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 80);

        //    // if (RemoteIpEndPoint.Address.Address.ToString() == "0") { return; }
        //    byte[] received = udpListener.EndReceive(ar, ref RemoteIpEndPoint);
        //    Data data = new Data(received);

        //    switch (data.cmdCommand)
        //    {
        //        case Command.Login:
        //            Random random = new Random();
        //            int newid = random.Next(1000, 100000000);

        //            RemoteClient rc = new RemoteClient()
        //            {
        //                IPAddress = RemoteIpEndPoint.Address.ToString(),
        //                Port = RemoteIpEndPoint.Port.ToString(),
        //                RemoteID = newid.ToString(),
        //                LocalIPAddress = data.IPAddress,
        //                LocalPort = data.port
        //            };
        //            Clients.Add(rc);
        //            Data LoginData = new Data();
        //            LoginData.cmdCommand = Command.Login;
        //            LoginData.strMessage = "LoggedIn";
        //            LoginData.strName = rc.RemoteID;
        //            byte[] response = LoginData.ToByte();
        //            UpdateList(lstClients);
        //            udpListener.Send(response, response.Length, RemoteIpEndPoint);
        //            break;
        //        case Command.ConnectPartner: // request from one who is willing to see remote connection
        //            RemoteClient partnerclient = Clients.Where(t => t.RemoteID == data.strName).FirstOrDefault();

        //            RemoteClient myclient = Clients.Where(t => t.RemoteID == data.strMessage).FirstOrDefault();

        //            if (partnerclient != null)
        //            {
        //                Data Data = new Data();
        //                Data.cmdCommand = Command.Ping;
        //                Data.strMessage = partnerclient.Display;
        //                Data.strName = "client";
        //                byte[] data1 = Data.ToByte();
        //                udpListener.Send(data1, data1.Length, RemoteIpEndPoint);  // Send the Remote Client application The IP data of RemoteHost

        //                Thread.Sleep(1000);


        //                RemoteClient myremoteclient = new RemoteClient() { IPAddress = RemoteIpEndPoint.Address.ToString(), Port = RemoteIpEndPoint.Port.ToString() };
        //                if (myclient != null)
        //                {
        //                    myremoteclient.LocalIPAddress = myclient.LocalIPAddress;
        //                    myremoteclient.LocalPort = myclient.LocalPort;
        //                }

        //                // Sending Host the IP of remote client , so that Host can also know public IP of its client. 
        //                Data NewData = new Data();
        //                NewData.cmdCommand = Command.Ping;
        //                NewData.strMessage = myremoteclient.Display;
        //                NewData.strName = "host";
        //                byte[] ndata = NewData.ToByte();
        //                udpListener.Send(ndata, ndata.Length, partnerclient.IPEndPoint);


        //            }
        //            break;
        //    }



        //    //  MessageBox.Show(Encoding.UTF8.GetString(received));
        //    udpListener.BeginReceive(new AsyncCallback(DataReceived), null);
        //    string tip = RemoteIpEndPoint.ToString();
        //}

        //public void OnSend(IAsyncResult ar)
        //{
        //    try
        //    {
        //        udpListener.EndSend(ar);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, "SGSServerUDP", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}

        private void HandShake()
        {

        }

        //private void SendUDPMessage(IPEndPoint RemoteIpEndPoint, RemoteClient rc)
        //{
        //    //using (UdpClient client = new UdpClient())
        //    //{
        //    Data Data = new Data();
        //    Data.cmdCommand = Command.Ping;
        //    Data.strMessage = rc.Display;
        //    byte[] data = Data.ToByte();

        //    //RemoteIpEndPoint.Port = 65444;
        //    udpListener.Send(data, data.Length, RemoteIpEndPoint);      // Send the date to the server
        //    //}
        //}

        private void btnPingFromServer_Click(object sender, EventArgs e)
        {
            //RemoteClient rc = (RemoteClient)lstClients.SelectedItem;

            //Data Data = new Data();
            //Data.cmdCommand = Command.Message;
            //Data.strMessage = "Message from Server!!!!";
            //byte[] data = Data.ToByte();

            //if (rc != null)
            //{
            //    udpListener.Send(data, data.Length, rc.IPEndPoint);
            //}


            //tcpServer1.Connections[0].Socket.GetStream().Write(data, 0, data.Length);


        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            tcpServer1.Close();

        }


        #region "NEW TCP"

        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the 
                    // client. Display it on the console.
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);
                    // Echo the data back to the client.
                    Send(handler, content);
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion


    }

    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    public class UdpState
    {
        public IPEndPoint e { get; set; }
        public UdpClient u { get; set; }
    }


}
