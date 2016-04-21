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
    public class TransferData
    {
        //Default constructor
        public TransferData()
        {
            this.cmdCommand = Command.NULL;
            this.strMessage = null;
            this.strName = null;
            this.IPAddress = null;
            this.port = null;
            this.UDPTravelData = null;
            this.pointers = null;
            this.Settings = null;
        }




        public byte[] ToByte()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this);

            return ms.ToArray();
        }

        // Convert a byte array to an Object
        public TransferData(byte[] data)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(data, 0, data.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            if (obj != null)
            {
                TransferData d = (TransferData)obj;
                this.cmdCommand = d.cmdCommand;
                this.IPAddress = d.IPAddress;
                this.port = d.port;
                this.strMessage = d.strMessage;
                this.strName = d.strName;
                this.UDPTravelData = d.UDPTravelData;
                this.region = d.region;
                this.pointers = d.pointers;
                this.ProcessedRequestNo = d.ProcessedRequestNo;
                this.RequestNo = d.RequestNo;
                this.TotalTiles = d.TotalTiles;
                this.Settings = d.Settings;
            }
        }


        public string strName;
        //Message text
        public string strMessage;
        //Command type (login, logout, send message, etc)
        public Command cmdCommand;
        public int[] pointers = null;
        public TravelImage UDPTravelData = null;
        public string IPAddress;
        public string port;
        public System.Drawing.Rectangle region;
        public int x = 0;
        public int y = 0;
        public double RequestNo = 0;
        public double ProcessedRequestNo = 0;
        public int TotalTiles = 0;
        public ConSettings Settings = null;
    }


    [Serializable]
    public class TravelImage
    {
        public DateTime CapturedTime;

        public DateTime PreviousSendTime;

        public bool IsTileChanged = true;

        public bool sent = false;

        /// <summary>
        /// Desktop Width
        /// </summary>
        public int DskWidth { get; set; }

        /// <summary>
        /// Desktop Width (Reduced)
        /// </summary>
        public int ResizedWidth { get; set; }

        /// <summary>
        /// Desktop Height
        /// </summary>
        public int DskHeight { get; set; }

        /// <summary>
        /// Desktop Height (Reduced)
        /// </summary>
        public int ResizedHeight { get; set; }

        public int nRows { get; set; }
        public int nCols { get; set; }

        public TravelImage()
        {
            this.CapturedTime = DateTime.Now;
            this.PreviousSendTime = DateTime.Now;
        }

        public int Left = 0;
        public int Top = 0;
        public int Height = 0;
        public int Width = 0;
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
            if (span.TotalMilliseconds >= QueueTimeMSec) return true;
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
            if (DelayTimeMSec < 0.0) { return false; }
            //Check from previous sent time
            TimeSpan span = dtCurrent.Subtract(this.PreviousSendTime);
            //If dealyed than DelayTimeMSec return true else return false
            if (span.TotalMilliseconds >= DelayTimeMSec) return true;
            return false;
        }
    }


}
