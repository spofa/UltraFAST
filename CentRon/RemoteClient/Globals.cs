using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelObjects;

namespace RemoteClient
{ 

    public static class Globals
    {

        public static bool arrayinitialized = false;
        public static Bitmap primaryImage;
        public static RemoteClientForm AppForm { get; set; }
        public static IService service { get; set; }
        public static Bitmap PreviousImage;
        public static Bitmap PreviousSmallImage;
        public static TravelImage TravelImage;
        public static byte[] PreviousImageArray;

        public static TravelImage[] TileArray;

        public static int Width = 1024;
        public static int Height = 768;
        public static int rows = 4;
        public static int cols = 4;

        public static int TotalTiles
        {
            get
            {
                return rows * cols;
            }
        }

        public static double FPS = 10.0;

        public static double TFrameGap
        {
            get
            {
                
                return 1000.0/ FPS;
            }
        }

        /// <summary>
        /// Captured tiles if remains in queue for more than this time gets 'expired'.
        /// (Ideal value may be 1 to 1.5 times of TFrameGap)
        /// </summary>
        public static double QueueExpiryTime
        {
            get
            {
                return Properties.Settings.Default.QueueExpiryTime;
            }
        }

        /// <summary>
        /// Any tile not sent for this long gets maked as 'delayed' and needs sending. 
        /// (Ideal value may be like once per second or 1000msec)
        /// </summary>
        public static double NotSent4LongTime
        {
            get
            {
                return Properties.Settings.Default.NotSent4LongTime;
            }
        }

        public static int lastTileSent = -1;

        public static void InitializeTileArray()
        {
            TileArray = new TravelImage[cols * rows];
            arrayinitialized = true;
        }
    }
}
