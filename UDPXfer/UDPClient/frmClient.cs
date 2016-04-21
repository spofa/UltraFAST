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

namespace UDPClient
{
    public partial class frmClient : Form
    {
        private Dictionary<String, APeerOnNw> RegisteredHosts = new Dictionary<string, APeerOnNw>();

        public bool IsConnected = false;

        private bool MsgLoopHooked = false;
        private bool IsStarted = false;

        /// <summary>
        /// MySelf As Peer (A)
        /// </summary>
        private APeerOnNw _PeerMySelf = null;

        /// <summary>
        /// MyExpected Partner As Peer (B)
        /// </summary>
        private APeerOnNw _PeerMyTarget = null;

        private NetClient _Server;

        public NetClient Server
        {
            get
            {
                if (_Server == null)
                {
                    _Server = new NetClient(this.NetPeerConfig);

                    //Construct MySelf & My Peer
                    _PeerMySelf = new APeerOnNw(tbMyName.Text.Trim());
                    _PeerMyTarget = new APeerOnNw(tbMyPartnerName.Text.Trim());

                    //Get my internal IP Address
                    IPAddress ipIPADDRMask;
                    var endpoint = new IPEndPoint(NetUtility.GetMyAddress(out ipIPADDRMask), int.Parse(tbServerPort.Text));
                    _PeerMySelf.InternalEndPoint = endpoint;
                }

                return _Server;
            }
        }

        private NetPeerConfiguration _NetPeerConfig;

        public NetPeerConfiguration NetPeerConfig
        {
            get
            {
                if (_NetPeerConfig == null)
                {
                    _NetPeerConfig = new NetPeerConfiguration(tbServerID.Text);
                    //_NetPeerConfig.Port = int.Parse(tbServerPort.Text);

                    CheckNetPeerConfiguration(ref _NetPeerConfig);
                    SetupNetPeerConfiguration(ref _NetPeerConfig);
                }

                return _NetPeerConfig;
            }
        }

        ~frmClient()
        {
            //Disconnect Existing Client
            if (_Server != null)
            {
                _Server.Disconnect("[[SHUTDOWN]]");
                _Server = null;
                _NetPeerConfig = null;
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


        public frmClient()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            //Stop existing client
            if (_Server != null) { _Server.Disconnect("[[SHUTDOWN]]"); }

            //Resolve IP (if like www.google.com)
            var ResolvedServerIP = NetUtility.Resolve(tbServerIP.Text).ToString();

            //Start the server
            Server.Start();
            Server.Connect(host: ResolvedServerIP, port: int.Parse(tbServerPort.Text));

            //Disable this button, Enable Other
            btnConnect.Enabled = false;
            btnDisConnect.Enabled = true;

            //Setup IsStarted
            IsStarted = true;
        }

        private void btnDisConnect_Click(object sender, EventArgs e)
        {
            //Disconnect Existing Client
            if (_Server != null)
            {
                _Server.Disconnect("[[SHUTDOWN]]");
                _Server = null;
                _NetPeerConfig = null;
            }

            //Disable this button, Enable Other
            btnConnect.Enabled = true;
            btnDisConnect.Enabled = false;

            //Setup IsStarted
            IsStarted = false;
        }

        private void frmClient_Load(object sender, EventArgs e)
        {
            //Hook message processing event
            if (!MsgLoopHooked)
            {
                Application.Idle += Application_Idle;
                MsgLoopHooked = true;
            }
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

        private void SendUnConnectedMessageToServer(MessageType _TypeOfMessage, 
            APeerOnNw _Sender, 
            params Object[] _ObjectsToBeSent)
        {
            //Take name of this peer from text box
            _Sender.PeerName = tbMyName.Text.Trim();

            //If not connected return
            if (!IsConnected) { return; }
            //Create A MessageObject Which Will Be Send 
            NetOutgoingMessage MsgObject = Server.CreateMessage();
            //Write TypeOfMessage to MessageObject
            MsgObject.Write((byte)_TypeOfMessage);
            //Write Sender to MessageObject
            MsgObject.WriteAllProperties(_Sender);
            //Write each additional object to MessageObject
            if (_ObjectsToBeSent != null)
            {
                //Iterate through each of the users input
                foreach (Object iObj in _ObjectsToBeSent)
                {
                    //Write ObjectToBeSent to MessageObject
                    MsgObject.WriteAllProperties(iObj);
                }
            }
            //Write to Server
            var ResolvedServerIP = NetUtility.Resolve(tbServerIP.Text).ToString();
            IPEndPoint RemoteEndPt = Server.ServerConnection.RemoteEndPoint;
            RemoteEndPt = (RemoteEndPt == null) ? NetUtility.Resolve(ResolvedServerIP, int.Parse(tbServerPort.Text)) : RemoteEndPt;
            Server.SendUnconnectedMessage(MsgObject, RemoteEndPt);
        }

        private void ProcessAnyData(NetIncomingMessage iMsg, bool IsConnectedData)
        {
            //Read Message Type [[Always the first byte in all Messages]]
            MessageType mType = (MessageType)iMsg.ReadByte();

            //Reply Code Variable
            ReplyCode _ReplyCode = ReplyCode.NACK;

            //Other shared data
            APeerOnNw replyPeer = null;

            //Process Messages As Per Type
            switch (mType)
            {
                case MessageType.IntroduceREPLY:
                    //For replies, check if ACK given
                    _ReplyCode = (ReplyCode)iMsg.ReadByte();

                    //Fetch replied peer
                    replyPeer = new APeerOnNw("DUMMY");
                    iMsg.ReadAllProperties(replyPeer);

                    //NACK - Host not found
                    if (_ReplyCode == ReplyCode.ACK)
                    {
                        Logger.WriteLine(String.Format("INTROOK: {0}", replyPeer.ToString()));
                        //Wipe existing peer
                        _PeerMyTarget = new APeerOnNw(tbMyPartnerName.Text);
                    }
                    //ACK - Host found
                    if (_ReplyCode == ReplyCode.NACK)
                    {
                        Logger.WriteLine(String.Format("INTROFAIL: {0}", replyPeer.ToString()));
                        //Read introduced peer
                        _PeerMyTarget = replyPeer;
                    }
                    break;

                case MessageType.GetHostDetailsREPLY:
                    //For replies, check if ACK given
                    _ReplyCode = (ReplyCode)iMsg.ReadByte();

                    //Fetch Requested Peer Here for this Request
                    replyPeer = new APeerOnNw("DUMMY");
                    iMsg.ReadAllProperties(replyPeer);

                    //NACK - Host not found
                    if (_ReplyCode == ReplyCode.NACK)
                    {
                        Logger.WriteLine(String.Format("HOSTABSENT: {0}", replyPeer.PeerName));
                        //Wipe existing peer
                        _PeerMyTarget = new APeerOnNw(tbMyPartnerName.Text);
                    }
                    //ACK - Host found
                    if(_ReplyCode == ReplyCode.ACK)
                    {
                        Logger.WriteLine(String.Format("HOSTLOCATED: {0}", replyPeer.ToString()));
                        //Read introduced peer
                        _PeerMyTarget = replyPeer;
                    }
                    break;
                case MessageType.GetHostsListREPLY:
                    //For replies, check if ACK given
                    _ReplyCode = (ReplyCode)iMsg.ReadByte();
                    if (_ReplyCode != ReplyCode.ACK) { return; }

                    //ACK obtained, extract message body
                    APeerOnNw ithPeer = new APeerOnNw("DUMMY");
                    iMsg.ReadAllProperties(ithPeer);
                    if(RegisteredHosts.ContainsKey(ithPeer.PeerName))
                    {
                        RegisteredHosts[ithPeer.PeerName] = ithPeer;
                    }
                    else
                    {
                        RegisteredHosts.Add(ithPeer.PeerName, ithPeer);
                    }
                    Logger.WriteLine(String.Format("HOSTLISTACCPT: {0}", RegisteredHosts.Count.ToString()));
                    break;
                case MessageType.RegistrationREPLY:
                    //For replies, check if ACK given
                    _ReplyCode = (ReplyCode)iMsg.ReadByte();
                    if(_ReplyCode != ReplyCode.ACK) { return;  }

                    //ACK obtained, extract body of message
                    APeerOnNw msgPeer = new APeerOnNw("DUMMY");
                    iMsg.ReadAllProperties(msgPeer);
                    Logger.WriteLine(String.Format("REGREQ# {0}", msgPeer.ToString()));
                    //Update self endpoints and unique id
                    if(_PeerMySelf.PeerName.Equals(msgPeer.PeerName))
                    {
                        _PeerMySelf.UniqueIdentifier = msgPeer.UniqueIdentifier;
                        _PeerMySelf.ExternalEndPoint = new IPEndPoint(msgPeer.ExternalEndPoint.Address, msgPeer.ExternalEndPoint.Port);
                        Logger.WriteLine(String.Format("REGACCPT: {0}", _PeerMySelf.ToString()));
                    }
                    else
                    {
                        Logger.WriteLine(String.Format("REGREJECTED: {1} BY {0}", msgPeer.ToString(), _PeerMySelf.ToString()));
                    }
                    break;
                default:
                    Logger.WriteLine("Cant Process UnKnown Message");
                    break;
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            SendUnConnectedMessageToServer(MessageType.RegistrationREQ, _PeerMySelf);
        }

        private void btnFetchAll_Click(object sender, EventArgs e)
        {
            SendUnConnectedMessageToServer(MessageType.GetHostsListREQ, _PeerMySelf);
        }

        private void btnFetchPartner_Click(object sender, EventArgs e)
        {
            //Take name of partner from text box
            _PeerMyTarget.PeerName = tbMyPartnerName.Text.Trim();
            //Request updation of partner endpoint
            SendUnConnectedMessageToServer(MessageType.GetHostDetailsREQ, _PeerMySelf, _PeerMyTarget);
        }

        private void btnIntroduce_Click(object sender, EventArgs e)
        {
            //Take name of partner from text box
            _PeerMyTarget.PeerName = tbMyPartnerName.Text.Trim();
            //Request updation of partner endpoint
            SendUnConnectedMessageToServer(MessageType.IntroduceREQ, _PeerMySelf, _PeerMyTarget);
        }

        private void btnTransmitMessage_Click(object sender, EventArgs e)
        {
            String Msg = String.Format("Hello {0} this is {1}", _PeerMyTarget.PeerName, _PeerMySelf.PeerName);
            
            
        }
    }
}