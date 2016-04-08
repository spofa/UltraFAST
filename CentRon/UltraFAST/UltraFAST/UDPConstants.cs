using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
namespace UltraFAST
{
    public class UDPConstants
    {
        public const int DefaultPort = 6001;
        public static char FrameStrtChar = '$';
        public static char FrameEndChar = '#';
        
        public static String getTokenValue(String Frame, String Token)
        {
            String LastValue = Frame.Substring(Frame.IndexOf(Token + "="));
            LastValue = LastValue.Substring(0, LastValue.IndexOf(";"));
            return LastValue;          
        }

        public static byte[] StringToByteArray(String frame)
        {
            //Convert string to byte array
            byte[] Result = Encoding.UTF8.GetBytes(frame);
            //Return the result
            return Result;
        }

        public static String ByteArrayToString(byte[] data)
        {
            //Convert byte array to string
            String Result = Encoding.UTF8.GetString(data);
            //Return the result
            return Result;
        }

        public static String[] ExtractMessages(byte[] frameData)
        {
            return ExtractMessages(ByteArrayToString(frameData));
        }

        public static String[] ExtractMessages(String frameData)
        {
            //Fetch all commands
            String[] AllCommands = frameData.Split(new char[] { UDPConstants.FrameStrtChar, UDPConstants.FrameEndChar },StringSplitOptions.RemoveEmptyEntries );
            //Retrun all commands
            return AllCommands;
        }

        public static byte[] getFrameBytes(String Message)
        {
            return StringToByteArray(getFrameString(Message));
        }

        public static String getFrameString(String Message)
        {
            return String.Format("{0}{1}{2}", UDPConstants.FrameStrtChar.ToString(), Message, UDPConstants.FrameEndChar.ToString());
        }
        
        /// <summary>
        /// Configure socket values
        /// </summary>
        /// <param name="_Socket"></param>
        /// <param name="Blocking"></param>
        /// <param name="DontFragment"></param>
        /// <param name="EnableBroadcast"></param>
        /// <param name="ExclusiveAddressUse"></param>
        /// <param name="ReceiveBufferSize"></param>
        /// <param name="ReceiveTimeout"></param>
        /// <param name="SendBufferSize"></param>
        /// <param name="SendTimeout"></param>
        /// <param name="Ttl"></param>
	    public static void SetupSocket(Socket _Socket,
	                                int ReceiveBufferSize = 8192,
	                                int ReceiveTimeout = 0,
	                                int SendBufferSize = 8192,
	                                int SendTimeout = 0,
	                                bool Blocking=true,
	                                bool DontFragment=false,
	                                bool EnableBroadcast =true,
	                                bool ExclusiveAddressUse = false,
	                                short Ttl = 128)
        {
                _Socket.Blocking = Blocking;
                _Socket.DontFragment = DontFragment;
                _Socket.EnableBroadcast = EnableBroadcast;
                _Socket.ExclusiveAddressUse = ExclusiveAddressUse;
                _Socket.ReceiveBufferSize = ReceiveBufferSize;
                _Socket.ReceiveTimeout = ReceiveTimeout;
                _Socket.SendBufferSize = SendBufferSize;
                _Socket.SendTimeout = SendTimeout;
                _Socket.Ttl = Ttl;
        }
    }
    
    public enum UDPStatus
    {
    	//Frame
    	NACK = 000,
    	ACK = 001,
    	RES = 002,
    	//Parts
    	pNACK = 010,
    	pACK = 011,
    	pRES = 012,
    }
}
