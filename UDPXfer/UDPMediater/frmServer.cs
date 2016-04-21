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

namespace UDPMediater
{
    public partial class frmServer : Form
    {
        public bool IsConnected = false;

        private bool MsgLoopHooked = false;
        private bool IsStarted = false;

        /// <summary>
        /// All registered clients with this server
        /// </summary>
        private Dictionary<String, APeerOnNw> RegisteredClients = new Dictionary<string, APeerOnNw>();

        private NetServer _Server;

        public NetServer Server
        {
            get
            {
                if(_Server == null)
                {
                    _Server = new NetServer(this.NetPeerConfig);
                }

                return _Server;
            }
        }

        private NetPeerConfiguration _NetPeerConfig;

        public NetPeerConfiguration NetPeerConfig
        {
            get
            {
                if(_NetPeerConfig == null)
                {
                    _NetPeerConfig = new NetPeerConfiguration(tbServerID.Text);
                    _NetPeerConfig.Port = int.Parse(tbServerPort.Text);

                    CheckNetPeerConfiguration(ref _NetPeerConfig);
                    SetupNetPeerConfiguration(ref _NetPeerConfig);
                }

                return _NetPeerConfig;
            }
        }

        public void CheckNetPeerConfiguration(ref NetPeerConfiguration _NetPeerCfg)
        {
            bool flag = false;

            flag = _NetPeerCfg.IsMessageTypeEnabled(NetIncomingMessageType.ConnectionApproval);
            flag = _NetPeerCfg.IsMessageTypeEnabled(NetIncomingMessageType.ConnectionLatencyUpdated);
            flag = _NetPeerCfg.IsMessageTypeEnabled(NetIncomingMessageType.Data);
            flag = _NetPeerCfg.IsMessageTypeEnabled(NetIncomingMessageType.DebugMessage);
            flag = _NetPeerCfg.IsMessageTypeEnabled(NetIncomingMessageType.DiscoveryRequest);
            flag = _NetPeerCfg.IsMessageTypeEnabled(NetIncomingMessageType.DiscoveryResponse);
            flag = _NetPeerCfg.IsMessageTypeEnabled(NetIncomingMessageType.Error);
            flag = _NetPeerCfg.IsMessageTypeEnabled(NetIncomingMessageType.ErrorMessage);
            flag = _NetPeerCfg.IsMessageTypeEnabled(NetIncomingMessageType.NatIntroductionSuccess);
            flag = _NetPeerCfg.IsMessageTypeEnabled(NetIncomingMessageType.Receipt);
            flag = _NetPeerCfg.IsMessageTypeEnabled(NetIncomingMessageType.StatusChanged);
            flag = _NetPeerCfg.IsMessageTypeEnabled(NetIncomingMessageType.UnconnectedData);
            flag = _NetPeerCfg.IsMessageTypeEnabled(NetIncomingMessageType.VerboseDebugMessage);
            flag = _NetPeerCfg.IsMessageTypeEnabled(NetIncomingMessageType.WarningMessage);

        }

        public void SetupNetPeerConfiguration(ref NetPeerConfiguration _NetPeerCfg)
        {
            _NetPeerCfg.DisableMessageType(NetIncomingMessageType.ConnectionApproval);
            _NetPeerCfg.DisableMessageType(NetIncomingMessageType.ConnectionLatencyUpdated);
            _NetPeerCfg.EnableMessageType(NetIncomingMessageType.Data);
            _NetPeerCfg.DisableMessageType(NetIncomingMessageType.DebugMessage);
            _NetPeerCfg.DisableMessageType(NetIncomingMessageType.DiscoveryRequest);
            _NetPeerCfg.DisableMessageType(NetIncomingMessageType.DiscoveryResponse);
            _NetPeerCfg.DisableMessageType(NetIncomingMessageType.Error);
            _NetPeerCfg.DisableMessageType(NetIncomingMessageType.ErrorMessage);
            _NetPeerCfg.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);
            _NetPeerCfg.EnableMessageType(NetIncomingMessageType.Receipt);
            _NetPeerCfg.EnableMessageType(NetIncomingMessageType.StatusChanged);
            _NetPeerCfg.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            _NetPeerCfg.DisableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            _NetPeerCfg.DisableMessageType(NetIncomingMessageType.WarningMessage);
        }
        public frmServer()
        {
            InitializeComponent();
        }

        ~frmServer()
        {
            //Disconnect Existing Server
            if (_Server != null)
            {
                _Server.Shutdown("[[SHUTDOWN]]");
                _Server = null;
                _NetPeerConfig = null;
            }
        }

        private void frmServer_Load(object sender, EventArgs e)
        {
            //Hook message processing event
            if (!MsgLoopHooked)
            {
                Application.Idle += Application_Idle;
                MsgLoopHooked = true;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //Disconnect Existing Server
            if (_Server != null)
            {
                _Server.Shutdown("[[SHUTDOWN]]");
                _Server = null;
                _NetPeerConfig = null;
            }

            //Start the server
            Server.Start();

            //Disable this button, Enable Other
            btnStart.Enabled = false;
            btnStop.Enabled = true;

            //Setup IsStarted
            IsStarted = true;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            //Disconnect Existing Server
            if (_Server != null)
            {
                _Server.Shutdown("[[SHUTDOWN]]");
                _Server = null;
                _NetPeerConfig = null;
            }

            //Disable this button, Enable Other
            btnStart.Enabled = true;
            btnStop.Enabled = false;

            //Setup IsStarted
            IsStarted = false;
        }


        private void Application_Idle(object sender, EventArgs e)
        {
            while (NativeMethods.AppStillIdle)
            {
                //Incoming Message
                NetIncomingMessage iMsg;

                //Fetch A Message
                while ((iMsg = Server.ReadMessage()) != null)
                {
                    //Skip if not running
                    if (IsStarted != true) { continue; }

                    //Process this message
                    String sMsg = String.Empty;
                    String Chat = String.Empty;
                    switch (iMsg.MessageType)
                    {
                        case NetIncomingMessageType.DebugMessage:
                            sMsg = iMsg.ReadString();
                            Logger.WriteLine(String.Format("DebugMessage: {0}", sMsg));
                            break;
                        case NetIncomingMessageType.ErrorMessage:
                            sMsg = iMsg.ReadString();
                            Logger.WriteLine(String.Format("ErrorMessage: {0}", sMsg));
                            break;
                        case NetIncomingMessageType.WarningMessage:
                            sMsg = iMsg.ReadString();
                            Logger.WriteLine(String.Format("WarningMessage: {0}", sMsg));
                            break;
                        case NetIncomingMessageType.VerboseDebugMessage:
                            sMsg = iMsg.ReadString();
                            Logger.WriteLine(String.Format("VerboseDebugMessage: {0}", sMsg));
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            //Status
                            NetConnectionStatus iStatus = (NetConnectionStatus)iMsg.ReadByte();
                            //Reason
                            sMsg = iMsg.ReadString();
                            //Sender
                            String ClntPtr = NetUtility.ToHexString(iMsg.SenderConnection.RemoteUniqueIdentifier);
                            //Print Data
                            Logger.WriteLine(String.Format("StatusChanged: Msg={0}, NetConnectionStatus={1}, Sender={2}", sMsg, iStatus.ToString(), ClntPtr));
                            //Check Connected
                            if (iStatus == NetConnectionStatus.Connected)
                            {
                                Logger.WriteLine("!!! CONNECTED !!!");
                                IsConnected = true;
                            }
                            else if (iStatus == NetConnectionStatus.Disconnected)
                            {
                                Logger.WriteLine("!!! DISCONNECTED !!!");
                                IsConnected = false;
                            }
                            else
                            {
                                Logger.WriteLine(String.Format("!!! {0} !!!", iStatus.ToString().ToUpper()));
                            }                            
                            break;
                        case NetIncomingMessageType.Data:
                            Logger.WriteLine("!!!Data!!!");
                            ProcessAnyData(iMsg, false);
                            break;
                        case NetIncomingMessageType.UnconnectedData:
                            Logger.WriteLine("!!!UnconnectedData!!!");
                            ProcessAnyData(iMsg, true);
                            break;
                        case NetIncomingMessageType.ConnectionApproval:
                            sMsg = iMsg.ReadString();
                            Logger.WriteLine(String.Format("ConnectionApproval: {0}", sMsg));
                            break;
                        case NetIncomingMessageType.Receipt:
                            sMsg = iMsg.ReadString();
                            Logger.WriteLine(String.Format("Receipt: {0}", sMsg));
                            break;
                        case NetIncomingMessageType.DiscoveryRequest:
                            sMsg = iMsg.ReadString();
                            Logger.WriteLine(String.Format("DiscoveryRequest: {0}", sMsg));
                            break;
                        case NetIncomingMessageType.NatIntroductionSuccess:
                            sMsg = iMsg.ReadString();
                            Logger.WriteLine(String.Format("NatIntroductionSuccess: {0}", sMsg));
                            break;
                        case NetIncomingMessageType.ConnectionLatencyUpdated:
                            sMsg = iMsg.ReadString();
                            Logger.WriteLine(String.Format("ConnectionLatencyUpdated: {0}", sMsg));
                            break;
                    }

                    //Sleep
                    Thread.Sleep(1);
                }
            }
        }

        private void ProcessAnyData(NetIncomingMessage iMsg, bool IsConnectedData)
        {
            //Read Message Type [[Always the first byte in all Messages]]
            MessageType mType = (MessageType)iMsg.ReadByte();

            //Fetch Sender of Calls/Requests
            APeerOnNw callingPeer = null;
            callingPeer = new APeerOnNw("DUMMY");
            iMsg.ReadAllProperties(callingPeer);

            //Fetch unique client ID
            String UniqueClientID = NetUtility.ToHexString(iMsg.SenderConnection.RemoteUniqueIdentifier);

            //Other shared variables
            APeerOnNw reqPeer = null;

            //Process Messages As Per Type
            switch (mType)
            {
                case MessageType.IntroduceREQ:
                    //Fetch Requested Peer Here for this Request
                    reqPeer = new APeerOnNw("DUMMY");
                    iMsg.ReadAllProperties(reqPeer);

                    Logger.WriteLine(String.Format("IntroduceREQ# {0}", reqPeer.ToString()));

                    //Send ACK or NACK if "reqPeer" found or not
                    if (RegisteredClients.ContainsKey(reqPeer.PeerName))
                    {
                        //Located peer from dictionary
                        APeerOnNw foundPeer = RegisteredClients[reqPeer.PeerName];

                        //Introduce this peer
                        Server.Introduce(foundPeer.InternalEndPoint, foundPeer.ExternalEndPoint, callingPeer.InternalEndPoint, callingPeer.ExternalEndPoint, "token");

                        //ACK - Send ack of introduced peer
                        ReplyUnConnectedMessageToClient(MessageType.IntroduceREPLY,
                        foundPeer,
                        iMsg.SenderEndPoint,
                        ReplyCode.ACK);
                        Logger.WriteLine(String.Format("IntroduceREQACKSent# {0}", foundPeer.ToString()));
                    }
                    else
                    {
                        //NACK - Send back requested peer
                        ReplyUnConnectedMessageToClient(MessageType.IntroduceREPLY,
                        reqPeer,
                        iMsg.SenderEndPoint,
                        ReplyCode.NACK);
                        Logger.WriteLine(String.Format("IntroduceREQNACKSent# {0}", reqPeer.ToString()));
                    }
                    break;
                case MessageType.GetHostDetailsREQ:
                    //Fetch Requested Peer Here for this Request
                    reqPeer = new APeerOnNw("DUMMY");
                    iMsg.ReadAllProperties(reqPeer);

                    Logger.WriteLine(String.Format("GetHostDetailsREQ# {0}", reqPeer.ToString()));

                    //Send ACK or NACK if "reqPeer" found or not
                    if (RegisteredClients.ContainsKey(reqPeer.PeerName))
                    {
                        //ACK - Send details of found peer from list
                        ReplyUnConnectedMessageToClient(MessageType.GetHostDetailsREPLY,
                        RegisteredClients[reqPeer.PeerName],
                        iMsg.SenderEndPoint,
                        ReplyCode.ACK);
                        Logger.WriteLine(String.Format("HostDetailsACKSent# {0}", RegisteredClients[reqPeer.PeerName].ToString()));
                    }
                    else
                    {
                        //NACK - Send back requested peer
                        ReplyUnConnectedMessageToClient(MessageType.GetHostDetailsREPLY,
                        reqPeer,
                        iMsg.SenderEndPoint,
                        ReplyCode.NACK);
                        Logger.WriteLine(String.Format("HostDetailsNACKSent# {0}", reqPeer.ToString()));
                    }
                    
                    break;
                case MessageType.GetHostsListREQ:
                    //Nothing to Fetch Here for this Request
                    Logger.WriteLine(String.Format("GetHostsListREQ# {0}", callingPeer.ToString()));

                    //Send entire list to the client
                    foreach(var iPeerKVP in RegisteredClients)
                    {
                        APeerOnNw iPeer = iPeerKVP.Value;
                        ReplyUnConnectedMessageToClient<APeerOnNw>(MessageType.GetHostsListREPLY,
                        iPeer,
                        iMsg.SenderEndPoint,
                        ReplyCode.ACK);
                    }
                    Logger.WriteLine(String.Format("SentHostsList# {0}", callingPeer.ToString()));
                    break;
                case MessageType.RegistrationREQ:
                    //Nothing to Fetch Here for this Request
                    Logger.WriteLine(String.Format("RegistrationREQ# {0}", callingPeer.ToString()));

                    //Update Unique Client ID and Public EndPoint In Request
                    callingPeer.ExternalEndPoint = iMsg.SenderEndPoint;
                    callingPeer.UniqueIdentifier = UniqueClientID;

                    //Register Client to Our List and Replay Back Public Port
                    if(RegisteredClients.ContainsKey(callingPeer.PeerName))
                    {
                        //Client exsist, update it as it may be reconnected
                        RegisteredClients[callingPeer.PeerName] = callingPeer;
                        Logger.WriteLine(String.Format("REGOK [UPDATED]: {0}", callingPeer.ToString()));
                    }
                    else
                    {
                        //Client is new register/store it
                        RegisteredClients.Add(callingPeer.PeerName, callingPeer);
                        Logger.WriteLine(String.Format("REGOK [ADDED]: {0}", callingPeer.ToString()));
                    }                   

                    //Send Registration Acknowledgement
                    ReplyUnConnectedMessageToClient(MessageType.RegistrationREPLY, 
                        callingPeer, 
                        iMsg.SenderEndPoint,
                        ReplyCode.ACK);

                    break;
                default:
                    Logger.WriteLine("Cant Process UnKnown Message");
                    break;
            }
        }


        private void ReplyUnConnectedMessageToClient(MessageType _TypeOfMessage, 
            Object _ObjectToBeSent, 
            IPEndPoint _ClientEndPt, 
            ReplyCode _ReplyCode)
        {
            ReplyUnConnectedMessageToClient<Object>(_TypeOfMessage, _ObjectToBeSent, _ClientEndPt, _ReplyCode);
        }

        private void ReplyUnConnectedMessageToClient<T>(MessageType _TypeOfMessage, 
            T _ObjectToBeSent, 
            IPEndPoint _ClientEndPt,
            ReplyCode _ReplyCode)
        {
            //If not connected return
            if (!IsConnected) { return; }
            //Create A MessageObject Which Will Be Send 
            NetOutgoingMessage MsgObject = Server.CreateMessage();
            //Write TypeOfMessage to MessageObject
            MsgObject.Write((byte)_TypeOfMessage);
            //Write ReplyCode to MessageObject
            MsgObject.Write((byte)_ReplyCode);
            //Write ObjectToBeSent to MessageObject
            MsgObject.WriteAllProperties(_ObjectToBeSent);
            //Write to Client [[Remote End Point]]
            IPEndPoint RemoteEndPt = _ClientEndPt;
            Server.SendUnconnectedMessage(MsgObject, RemoteEndPt);
        }
    }
}
