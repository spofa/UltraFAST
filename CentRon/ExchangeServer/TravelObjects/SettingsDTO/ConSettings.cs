using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelObjects
{
    [Serializable]
    public enum SendingMethods { Pull, Push }


    [Serializable]
    public class ConSettings
    {
        public bool Is8BitQuantize { get; set; }

        public Size ScaleDownSize { get; set; }

        public SendingMethods SendingMethod { get; set; }

        public int UpdateFrequency { get; set; }

        public bool UpScale { get; set; }

        public bool ConvertToJPEG { get; set; }

        public double JPEGQuality { get; set; }

        public bool CompressTiles { get; set; }

        public int Rows = 1;
        public int Columns = 1;

        public NetDeliveryMethod TileDeliveryMethod = NetDeliveryMethod.ReliableOrdered;



        public bool ResetDefault = false;

        public string ServerIP = "";

        public bool AutoConnectServer = false;



    }
}
