using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using TravelObjects;

namespace RemoteClient
{

    public static class Globals
    {

        public static string SettingsPath = DirectoryPath() + "\\consettings.xml";

        public static ConSettings Settings = null;

        private static ConSettings DefaultSettings = new ConSettings()
        {
            Columns = 4,
            Rows = 4,
            ConvertToJPEG = true,
            JPEGQuality = 1.0,
            Is8BitQuantize = false,
            ScaleDownSize = SzLaptop.getSize(),
            UpScale = true,
            SendingMethod = SendingMethods.Pull,
            UpdateFrequency = 250,
            TileDeliveryMethod = Lidgren.Network.NetDeliveryMethod.ReliableOrdered,
            AutoConnectServer = true,
            ServerIP = "192.168.11.124",
            ResetDefault = true,
            CompressTiles = true,           
        };


        public static bool IsOpen = false;

        public static bool arrayinitialized = false;
        public static Bitmap bmpSurface;
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

        public static bool PaintingStarted = false;


        public static int ClientFullRequestInterval = 1500;
        public static int ClientPartialRequestInterval = 2000;

        public static int TotalTiles
        {
            get
            {
                return rows * cols;
            }
        }

        public static double FPS = 15.0;

        public static double TFrameGap
        {
            get
            {

                return Settings.UpdateFrequency / FPS;
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

        public static void LoadConnectionSettings()
        {

            XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(ConSettings));

            // A FileStream is needed to read the XML document.
            FileStream fs = new FileStream(Globals.SettingsPath, FileMode.Open);
            XmlReader reader = XmlReader.Create(fs);

            // Declare an object variable of the type to be deserialized.
            ConSettings i;
            i = (ConSettings)serializer.Deserialize(reader);
            if (i != null)
            {
                Globals.Settings = i;
            }
            fs.Close();

        }



        public static void SaveDefaultConnectionSettings()
        {
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(ConSettings));
            var path = Globals.SettingsPath;
            System.IO.FileStream file = System.IO.File.Create(path);
            writer.Serialize(file,Globals.DefaultSettings);
            file.Close();

        }


        public static string DirectoryPath()
        {

            string path;
            path = System.IO.Path.GetDirectoryName(
               System.Reflection.Assembly.GetExecutingAssembly().Location);


            return path;

        }

    }
}
