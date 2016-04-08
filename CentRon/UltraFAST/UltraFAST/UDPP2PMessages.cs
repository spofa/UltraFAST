/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 07-Apr-16
 * Time: 1:41 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace UltraFAST
{
	/// <summary>
	/// Description of UDPP2PMessages.
	/// </summary>
	public class UDPP2PMessages
	{
        public static byte[] ConnectRequest(String _ClientID)
        {
            return UDPConstants.getFrameBytes(String.Format("{0}: FROM={1};", UDPRequests.CONNECT, _ClientID));
        }
        
        public static byte[] MessageRequest(String _ClientID, String _MsgID, String _Message)
        {
        	return UDPConstants.getFrameBytes(String.Format("{0}: FROM={1};MSGID={2};MSG={3}", UDPRequests.PACKET, _ClientID, _MsgID, _Message));
        }
        
        public static byte[] getConnectReply(String _ServerID, 
                                             UDPStatus Status, 
                                             String _ClientID, 
                                             String _YourRouterPublicIP, 
                                             String _YourRouterPublicPort)
        {
    
        	return UDPConstants.getFrameBytes(String.Format("{0}: FROM={5};STATUS={1};ClientID={2};YourRouterPublicIP={3};YourRouterPublicPort={4};", 
        	                                               UDPRequests.CONNECT, 
        	                                               Status.ToString(), 
        	                                               _ClientID,
        	                                               _YourRouterPublicIP,
        	                                               _YourRouterPublicPort,
        	                                               _ServerID));
        }
	}
}
