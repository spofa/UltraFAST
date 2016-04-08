/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 06-Apr-16
 * Time: 3:43 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UltraFAST
{
    /// <summary>
    /// UDPP2PClient  (http://jwhsmith.net/2014/03/udp-routers-hole-punching/)
    /// </summary>
    public class UDPP2PClient
    {
        public string ServerURL { get; set; }
        public string ServerIP { get; set; }
        public string ClientID { get; set; }
        public int UDPPort {get; set;}
        
//        private IPEndPoint _ptrHostEndPt = null;
//        
//        public IPEndPoint ptrHostEndPt
//        {
//        	get
//        	{
//        		if(_ptrHostEndPt == null)
//        		{
//        			_ptrHostEndPt = new IPEndPoint(IPAddress.Parse(this.ServerIP), 
//        			                               this.UDPPort);
//        		}
//        		return _ptrHostEndPt;
//        	}
//        }
//        	
//        private Socket _ptrHostSocket = null;
//        
//        public Socket ptrHostSocket
//        {
//        	get
//        	{
//        		if(_ptrHostSocket == null)
//        		{
//        			_ptrHostSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
//        			SetupServerSocket(_ptrHostSocket);
//        		}
//        		return _ptrHostSocket;
//        	}
//        }
        
        /// <summary>
        /// 128KB, 0mSec ==> (send/recieve)
        /// </summary>
        /// <param name="ptrSocket"></param>
        private void SetupServerSocket(Socket ptrSocket)
        {
        	UDPConstants.SetupSocket(ptrSocket, 131072, 0, 131072, 500);
        }
        
//        ~UDPP2PClient()
//        {
//        	if(_ptrHostSocket != null)
//        	{
//        		try { _ptrHostSocket.Disconnect(true); } catch {}
//        		try { _ptrHostSocket.Dispose(); } catch {}
//        		_ptrHostSocket = null;
//        		_ptrHostEndPt = null;
//        	}
//        }
        
        public UDPP2PClient(string _ClientID, string _ServerURL, int _UDPPort = UDPConstants.DefaultPort, bool Connect = true)
        {
        	using(Socket ptrHostSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        	{
	            //Store URL & IPAddress of Server
	            this.ServerURL = _ServerURL;
	            this.ClientID = _ClientID;
	            this.UDPPort = _UDPPort;
	            this.ServerIP = (Dns.GetHostAddresses(_ServerURL)[0]).ToString();
	            
	            //Configure Socket & EndPoint
        		SetupServerSocket(ptrHostSocket);
        		IPEndPoint ptrHostEndPt = new IPEndPoint(IPAddress.Parse(this.ServerIP), this.UDPPort);
        		
	            //Send message to the server
	            if(Connect)
	            {
	            	ptrHostSocket.SendTo(UDPP2PMessages.ConnectRequest(this.ClientID), ptrHostEndPt);
	            }
        	}
        }
        
        public void SendToServer(String _MsgID, String _Message)
        {
        	using(Socket ptrHostSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        	{
        		//Configure Socket & EndPoint
        		SetupServerSocket(ptrHostSocket);
        		IPEndPoint ptrHostEndPt = new IPEndPoint(IPAddress.Parse(this.ServerIP), this.UDPPort);
        		
	          	//Fit socket length to data
	          	var prevBufferSz = ptrHostSocket.SendBufferSize;
	          	if(ptrHostSocket.SendBufferSize < _Message.Length)
	          	{
	          		ptrHostSocket.SendBufferSize = _Message.Length;
	          	}
	          	
	            //Send message to the server
	            ptrHostSocket.SendTo(UDPP2PMessages.MessageRequest(this.ClientID, _MsgID, _Message), ptrHostEndPt);
	            ptrHostSocket.SendBufferSize = prevBufferSz;
        	}
        }
        
        public String ReadFromServer(int _Size=-1, int _ReceiveTimeOut = -1, int _RecieveBufferSize=-1)
        {
        	using(Socket ptrHostSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        	{
        		//Configure Socket & EndPoint
        		SetupServerSocket(ptrHostSocket);
        		IPEndPoint ptrHostEndPt = new IPEndPoint(IPAddress.Parse(this.ServerIP), this.UDPPort);
	        		
	          	//Configure ReceiveTimeout
	          	var prevRXTimeOut = ptrHostSocket.ReceiveTimeout; 
	          	if(_ReceiveTimeOut > 100)
	          	{
	          		ptrHostSocket.ReceiveTimeout = _ReceiveTimeOut;
	          	}
	          	
	          	//Configure ReceiveBufferSize
	          	var prevBufferSz = ptrHostSocket.ReceiveBufferSize;
	          	if(_RecieveBufferSize > 8192)
	          	{
	          		ptrHostSocket.ReceiveBufferSize = _RecieveBufferSize;
	          	}
	          	
	            //Send message to the server
	            byte[] Telegram = null;
	            if(_Size < 0)
	            {
	            	_Size = (ptrHostSocket.Available == 0)?8192:ptrHostSocket.Available;
	            	Telegram = new byte[_Size];
	            	ptrHostSocket.Receive(Telegram, _Size, SocketFlags.None);
	            }
	            else
	            {
	            	Telegram = new byte[_Size];
	            	ptrHostSocket.Receive(Telegram, _Size, SocketFlags.None);
	            }
	            
	            //Convert to String
	            ptrHostSocket.ReceiveTimeout = prevRXTimeOut;
	            ptrHostSocket.ReceiveBufferSize = prevBufferSz;
	            
	            //Return the result
	            return UDPConstants.ByteArrayToString(Telegram);
        	}
        }
    }
}
