/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 06-Apr-16
 * Time: 3:43 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;

namespace UltraFAST
{
	/// <summary>
	/// UDPP2PServer (http://jwhsmith.net/2014/03/udp-routers-hole-punching/)
	/// </summary>
	public class UDPP2PServer
	{		

		
		private bool IsProcessingOn {get; set;}
		private bool IsTerminate {get; set;}

        private Task BGThreadPTR = null;
		
		/// <summary>
		/// Create a running server instance
		/// </summary>
		/// <param name="UDPPort"></param>
		/// <param name="StartServer"></param>
		public UDPP2PServer(int UDPPort = UDPConstants.DefaultPort, bool StartServer = true)
		{
			//Set Basic Flags
			IsProcessingOn = StartServer;
			IsTerminate = false;
			
			//Launch async task
			if(BGThreadPTR == null)
			{
                BGThreadPTR = new Task(() => { BeginServer(UDPPort); });
                BGThreadPTR.Start();
            }
		}
		
		~UDPP2PServer()
		{
			//Set Basic Flags
			IsProcessingOn = false;
			IsTerminate = true;
        }

		public void StartServer()
		{
			IsProcessingOn = true;
		}
		
		public void PauseServer()
		{
			IsProcessingOn = false;
		}
		
		public void ExitServer()
		{
			IsTerminate = true;
		}

        /// <summary>
        /// Server's background processing thread
        /// BASED: http://jwhsmith.net/2014/03/udp-routers-hole-punching/
        ///        http://stackoverflow.com/questions/12136031/udp-hole-punching-have-server-talk-to-client
        ///        http://stackoverflow.com/questions/9140450/udp-hole-punching-implementation
        /// PUNCHING A HOLE:
        /// --------------------------------------------------------------------
        /// 1. Your machine sends a packet to www.google.com:5000
        /// 2. After DNS/Firewall, your router handles further transmission
        /// 3. Router assigns random port (say 12345) on its public IP (say 115.2.3.4)
        /// 4. This is now punched
        /// - Anything send to 115.2.3.4:12345 by www.google.com (or anybody else) will
        /// come to you
        /// - Anything you sent to google will flow out from 115.2.3.4:12345 to www.google.com:5000
        /// 
        /// TRICK: Once you close your socked descriptor (one you used to send to www.google.com)
        /// you cannot recive --> hole is closed
        /// TRICK: Other client sending to you, needs to tailor packet to use same local endpoint
        /// 
        /// </summary>
        /// <param name="UDPPort"></param>
        public void BeginServer(int UDPPort)
		{
        RunAgain:
            try
			{			
				//Listen to any client on UDPPort
				using(UdpClient Listner = new UdpClient(UDPPort))
				{
                    //Bind on all local interfaces
                    IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, UDPPort);
					
				NextRequest:
                    //Check if method is terminated
                    if(IsTerminate)
                    {
                        Listner.Close();
                        BGThreadPTR = null;
                        return;
                    }

					//Check if server is running
					if(IsProcessingOn == false) { Thread.Sleep(1000); goto NextRequest; }
					
					//Start Server & Wait for Recieve Messages from Client
					byte[] data = Listner.Receive(ref groupEP);

                    //Convert byte array to string
                    var rxedFrames = UDPConstants.ByteArrayToString(data);
					if(String.IsNullOrEmpty(rxedFrames)) { goto NextRequest; }
					
					//Check for headers and footers in the message
					if(rxedFrames.Contains(UDPConstants.FrameStrtChar.ToString()) && rxedFrames.Contains(UDPConstants.FrameEndChar.ToString()))
					{
                        //Split into messages
                        String[] rxEDTelegrams = UDPConstants.ExtractMessages(rxedFrames);
						if((rxEDTelegrams == null) || (rxEDTelegrams.Length <= 0)) { goto NextRequest; }
						
						//Process each command
						foreach(String sCmd in rxEDTelegrams)
						{
						   Logger.WriteLine(String.Format("SERVERGOT# [{0}]", sCmd));

                            if(sCmd.StartsWith(UDPRequests.CONNECT))
                            {
                                //Connect from client store if does not exsist (reply with OK)
                                //- As client is connected read client router's public IP 
                                //address and open port
                                var routerIP = groupEP.Address.ToString();
                                var routerPort = groupEP.Port;
                                var clntID = UDPConstants.getTokenValue(sCmd, "FROM");

                                Logger.WriteLine(String.Format("CONNECTED: {2} <ON ROUTER: {0}:{1}>", routerIP, routerPort, clntID));
                                
                                //Send OK Message With Extra Data
                                byte[] ackMsg = UDPP2PMessages.getConnectReply("SERVER", UDPStatus.ACK, clntID, routerIP.ToString(), routerPort.ToString());
                                Listner.Send(ackMsg, ackMsg.Length, routerIP, routerPort);
                            }
						}
					}

                    //Next request processing
                    goto NextRequest;
					
				}
			}
			catch(Exception Ex)
			{
				Debug.WriteLine(String.Format("ERR# {0}", Ex.ToString()));
				
				//If not exit then re-execute on any exception
				if(!IsTerminate) { Thread.Sleep(1500); goto RunAgain; }
			}
		}
	}
}

