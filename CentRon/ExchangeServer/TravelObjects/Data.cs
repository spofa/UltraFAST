using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace TravelObjects
{
    [Serializable]
    public class Data
    {
        //Default constructor
        public Data()
        {
            this.cmdCommand = Command.NULL;
            this.strMessage = null;
            this.strName = null;
            this.IPAddress = null;
            this.port = null;
            this.Image = null;
            this.pointers = null;
        }

        //Converts the bytes into an object of type Data
        //public Data(byte[] data)
        //{
        //    //The first four bytes are for the Command
        //    this.cmdCommand = (Command)BitConverter.ToInt32(data, 0);

        //    //The next four store the length of the name
        //    int nameLen = BitConverter.ToInt32(data, 4);

        //    //The next four store the length of the message
        //    int msgLen = BitConverter.ToInt32(data, 8);


        //    //The next four store the length of the IPAddress
        //    int ipLen = BitConverter.ToInt32(data, 12);


        //    //The next four store the length of the port
        //    int portLen = BitConverter.ToInt32(data, 16);


        //    //Makes sure that strName has been 
        //    //passed in the array of bytes
        //    if (nameLen > 0)
        //        this.strName =
        //          Encoding.UTF8.GetString(data, 20, nameLen);
        //    else
        //        this.strName = null;

        //    //This checks for a null message field
        //    if (msgLen > 0)
        //        this.strMessage =
        //          Encoding.UTF8.GetString(data, 20 + nameLen, msgLen);
        //    else
        //        this.strMessage = null;



        //    if (ipLen > 0)
        //        this.IPAddress = Encoding.UTF8.GetString(data, 20 + nameLen + msgLen, ipLen);
        //    else
        //        this.IPAddress = string.Empty;


        //    if (portLen > 0)
        //        this.port = Encoding.UTF8.GetString(data, 20 + nameLen + msgLen + ipLen, portLen);
        //    else
        //        this.port = string.Empty;

        //}

        //Converts the Data structure into an array of bytes
        //public byte[] ToByte()
        //{
        //    List<byte> result = new List<byte>();

        //    //First four are for the Command
        //    result.AddRange(BitConverter.GetBytes((int)cmdCommand));

        //    //Add the length of the name
        //    if (strName != null)
        //        result.AddRange(BitConverter.GetBytes(strName.Length));
        //    else
        //        result.AddRange(BitConverter.GetBytes(0));

        //    //Length of the message
        //    if (strMessage != null)
        //        result.AddRange(
        //          BitConverter.GetBytes(strMessage.Length));
        //    else
        //        result.AddRange(BitConverter.GetBytes(0));


        //    ////Length of the IPAddress
        //    if (IPAddress != null)
        //        result.AddRange(
        //          BitConverter.GetBytes(IPAddress.Length));
        //    else
        //        result.AddRange(BitConverter.GetBytes(0));


        //    ////Length of the port
        //    if (port != null)
        //        result.AddRange(
        //          BitConverter.GetBytes(port.Length));
        //    else
        //        result.AddRange(BitConverter.GetBytes(0));




        //    //Add the name
        //    if (strName != null)
        //        result.AddRange(Encoding.UTF8.GetBytes(strName));

        //    //And, lastly we add the message 
        //    //text to our array of bytes
        //    if (strMessage != null)
        //        result.AddRange(Encoding.UTF8.GetBytes(strMessage));


        //    if (IPAddress != null)
        //        result.AddRange(Encoding.UTF8.GetBytes(IPAddress));

        //    if (port != null)
        //        result.AddRange(Encoding.UTF8.GetBytes(port));



        //    return result.ToArray();
        //}

        //Name by which the client logs into the room



        public byte[] ToByte()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this);

            return ms.ToArray();
        }

        // Convert a byte array to an Object
        public Data(byte[] data)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(data, 0, data.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            if (obj != null)
            {
                Data d = (Data)obj;
                this.cmdCommand = d.cmdCommand;
                this.IPAddress = d.IPAddress;
                this.port = d.port;
                this.strMessage = d.strMessage;
                this.strName = d.strName;
                this.Image = d.Image;
                this.region = d.region;
                this.pointers = d.pointers;
            }
        }


        public string strName;
        //Message text
        public string strMessage;
        //Command type (login, logout, send message, etc)
        public Command cmdCommand;
        public int[] pointers = null;
        public TravelImage Image = null;
        public string IPAddress;
        public string port;
        public System.Drawing.Rectangle region;
        public int x = 0;
        public int y = 0;

    }


    [Serializable]
    public class TravelImage
    {
        
        public DateTime CapturedTime;

        public DateTime PreviousSendTime;

        public bool IsImageChanged = true;

        public bool sent = false;

        public TravelImage()
        {
            this.CapturedTime = DateTime.Now;
            this.PreviousSendTime = DateTime.Now;
        }

        public int x = 0;
        public int y = 0;
        public byte[] ByteArray;


        /// <summary>
        /// Checks if tile is expired in queue by QueueTimeMSec
        /// </summary>
        /// <param name="dtCurrent">Current instance</param>
        /// <param name="QueueTimeMSec">Expiry threshold</param>
        /// <returns>true if expired</returns>
        public bool ExpiredInQueue(DateTime dtCurrent, double QueueTimeMSec)
        {
            //Bypass delayed if -ive value
            if (QueueTimeMSec < 0.0) { return false; }
            //Check from previous sent time
            TimeSpan span = dtCurrent.Subtract(this.CapturedTime);
            //If dealyed than DelayTimeMSec return true else return false
            if (span.Milliseconds >= QueueTimeMSec) return true;
            return false;
        }

        /// <summary>
        /// Check if tile is delayed in sending by DelayTimeMSec
        /// </summary>
        /// <param name="dtCurrent">Current instance</param>
        /// <param name="DelayTimeMSec">Delay threshold</param>
        /// <returns>true if delayed</returns>
        public bool TooMuchDelayedInSend(DateTime dtCurrent, double DelayTimeMSec)
        {
            //Bypass delayed if -ive value
            if(DelayTimeMSec < 0.0) { return false; }
            //Check from previous sent time
            TimeSpan span = dtCurrent.Subtract(this.PreviousSendTime);
            //If dealyed than DelayTimeMSec return true else return false
            if (span.Milliseconds >= DelayTimeMSec) return true;
            return false;
        }
    }


}
