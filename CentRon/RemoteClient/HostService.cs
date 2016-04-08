
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using ImageProcessor.Imaging.Quantizers;
using NVNCLibrary;
//using NVNCLibrary.Encodings;
using NVNCLibrary.Utils;
using NVNCLibrary.Utils.ScreenTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TravelObjects;
using AForge.Imaging.ColorReduction;
using AImg = AForge.Imaging;
using Lz4Net;
using UltraFAST;

namespace RemoteClient
{
    public class HostService : IService
    {
        private bool run = false;
        private Thread theThread;


        private dojo ImageDifference;
        private int ScreenClientX = 1920;           //Sizes of the client and our screen
        private int ScreenClientY = 1080;
        private int ScreenServerX = 1920;
        private int ScreenServerY = 1080;
        private int Padding = 3;
        private bool IsMetro = false;               //Flag for windows metro mode so we can nudge pen type devices to scroll the edge of the sceen 
        public int imageDelay = 2000;
        private PixelFormat ImageResoloution = PixelFormat.Format16bppRgb555;// PixelFormat.Format32bppArgb;

        public HostService() { }

        public void OnData(Data data)
        {
            if (data == null) { return; }

            switch (data.cmdCommand)
            {
                case Command.Read:
                    Globals.AppForm.PrintMessage(data.strMessage);
                    ChkAndTransmitCapturedTiles(Command.ReadResponse, data.strName);
                    break;
                case Command.Move:
                    MouseMove(data);
                    break;
                case Command.LClick:
                case Command.LDown:
                case Command.RClick:
                case Command.RDown:
                case Command.RUp:
                    // MouseClick(data);
                    break;
                case Command.Disconnect:
                    ChkAndTransmitCapturedTiles(Command.ReadResponse, data.strName);
                    Globals.AppForm.PrintMessage(data.strMessage);
                    stop();
                    break;

                default: break;
            }
        }

        public void start()
        {
            if (theThread == null)
            {
                run = true;
                theThread = new Thread(new ThreadStart(SGMainLoop));

                theThread.Start();
            }
        }

        DateTime tcapturePrevious = DateTime.Now;

        public void SGMainLoop()
        {
            System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Highest;
            MainApplication FastCapture = new MainApplication();
            bool firstCapture = false;
            bool IsImageGaptured = false;

            while (run)
            {
                //Initialie Row/Coloum Of Local App
                if (Globals.TileArray == null)
                {
                    Globals.rows = MainApplication.nRows;
                    Globals.cols = MainApplication.nCols;
                    Globals.TileArray = new TravelImage[Globals.rows * Globals.cols];

                    for (int i = 0; i < (Globals.rows * Globals.cols); i++)
                    {
                        Globals.TileArray[i] = new TravelImage();
                    }

                    //Continue While Loop
                    continue;
                }

                //Capture, tile, difference
                DateTime tcapture = DateTime.Now;
                TimeSpan span = tcapture.Subtract(tcapturePrevious);
                IsImageGaptured = false;
                if (span.Milliseconds >= Globals.TFrameGap)
                {
                    DateTime tnow = DateTime.Now;

                    //Fast Capture, Tile And Difference
                    Object Dummy = null;
                    FastCapture.Execute(ref Dummy);
                    firstCapture = true;
                    IsImageGaptured = true;

                    int mils = DateTime.Now.Subtract(tnow).Milliseconds;
                    tcapturePrevious = DateTime.Now;
                }

                //Iterate each tile and copy to local
                if (firstCapture && IsImageGaptured)
                {
                    FetchTilesAndCallTransmit();
                }
            }
        }

        private void FetchTilesAndCallTransmit()
        {
        	//Read Tiles from UltraFAST
            foreach (ROIArea roiAreaTile in MainApplication.DSKTopTileRegions)
            {
                TravelImage TImg = Globals.TileArray[roiAreaTile.RgnIndex];

                //Set up image to send
                byte[] bytArray;
                using (MemoryStream ms = new MemoryStream())
                {
                    Bitmap bmpTransmit = roiAreaTile.bmpToTransmit;

                    bmpTransmit.Save(ms, ImageFormat.Jpeg);

                    bytArray = ms.ToArray();
                }
                TImg.ByteArray = bytArray;

                //Set X, Y Coordinate
                TImg.x = roiAreaTile.Left;
                TImg.y = roiAreaTile.Top;

                //Put Flag for Changed Image
                TImg.IsImageChanged = roiAreaTile.IsROITileChanged;

                //Update the capture time
                TImg.CapturedTime = DateTime.Now;
            }

            //Send logic for images
            ChkAndTransmitCapturedTiles(Command.ReadResponse, "pri");
        }

        private void ChkAndTransmitCapturedTiles(Command cmd, string reqType)
        {
            try
            {
                //Iterate through each tile in the collection
                for (int idxTile = 0; idxTile < Globals.TileArray.Length; idxTile++)
                {
                    //Timing when iTile gets addressed by sending routine. Otherwise check expiry etc
                    DateTime dtCurrTile = DateTime.Now;

                    //Get properties of current tile into variables (saves time)
                    TravelImage iTile = Globals.TileArray[idxTile];   //Our tile
                    bool IsTileChanged = iTile.IsImageChanged;                                                  //Changed or not
                    bool IsTileDeadB4Sending = iTile.ExpiredInQueue(dtCurrTile, Globals.QueueExpiryTime);       //Expired or not
                    bool IsTileNotSend4Long = iTile.TooMuchDelayedInSend(dtCurrTile, Globals.NotSent4LongTime);//Resent delay

                    //If a tile is dead (not sent for long) then we must send it (provided this feature is turned on)
                    if(IsTileNotSend4Long && Properties.Settings.Default.EnSendNotSent4LongTiles)
                    {
                        //Send and proceed to next tile
                        TransmitTile(cmd, idxTile); continue;
                    }

                    //If a tile is changed (not-expired should be immediately sent, expired ones not to sent unless overridden)
                    //If tile is unchanged (not-expired not to be send unless overridden, expired not to be send unless overridden)
                    if(IsTileChanged && !IsTileDeadB4Sending)
                    {
                        //Send and proceed to next tile
                        TransmitTile(cmd, idxTile); continue;
                    }
                    else if (IsTileChanged && IsTileDeadB4Sending)
                    {
                        //Send only overidden tiles
                        if(Properties.Settings.Default.EnAlwaysSendChangedTiles)
                        {
                            //Send and proceed to next tile
                            TransmitTile(cmd, idxTile); continue;
                        }
                    }
                    else if(!IsTileChanged)
                    {
                        //Send only overridden tiles
                        if(Properties.Settings.Default.EnAlwaysSendUnchangedTiles)
                        {
                            //Send and proceed to next tile
                            TransmitTile(cmd, idxTile); continue;
                        }
                    }
                }


                //if(Globals.TotalTiles== Globals.lastTileSent) { Globals.lastTileSent = -1; }
                //Globals.lastTileSent = Globals.lastTileSent+1;
            }
            catch (Exception ex) { }
        }

        /// <summary>
        /// Transmit Tile
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <param name="iTile">Tile Index</param>
        private static void TransmitTile(Command cmd, int iTile)
        {
            //Send tile
            ListenService.SendLidgrenMessage(new Data()
            {
                cmdCommand = cmd,
                strMessage = "Relay from server--",
                Image = Globals.TileArray[iTile],
                strName = "pri"
            }, ListenService.GetPartnerEndPoint(), 
            Properties.Settings.Default.TileSendingMethod);
            //Update last sent time
            Globals.TileArray[iTile].PreviousSendTime = DateTime.Now;
        }

        public void MainLoop()
        {
            System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Highest;

            while (run)
            {
                DateTime tcapture = DateTime.Now;
                TimeSpan span = tcapture.Subtract(tcapturePrevious);
                if (span.Milliseconds >= Globals.TFrameGap)
                {
                    DateTime tnow = DateTime.Now;

                    CaptureAndSlice();

                    int mils = DateTime.Now.Subtract(tnow).Milliseconds;
                    tcapturePrevious = DateTime.Now;
                }


                ChkAndTransmitCapturedTiles(Command.ReadResponse, "pri");
            }


        }
        private void CaptureAndSlice()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Globals.PreviousImage = CaptureWithWin32API.CaptureFrames();

            Globals.TravelImage.x = 0; Globals.TravelImage.y = 0;
            Globals.TravelImage.ByteArray = Cutter.CreateSmaller(Globals.PreviousImage, Globals.Width, Globals.Height);

            Cutter.SliceImage(Globals.TravelImage.ByteArray);

            stopWatch.Stop();
            double elapsed = (double)stopWatch.ElapsedMilliseconds / 1000.0;
            double fps = (1 / elapsed);
        }
        
        private byte[] Compress(Bitmap Image)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();


            MemoryStream ms = new MemoryStream();
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 25L);
            Image.Save(ms, GetEncoder(ImageFormat.Jpeg), encoderParameters);

            System.Drawing.Image imgSave = System.Drawing.Image.FromStream(ms);
            imgSave.Save("D:\\Temp\\JPGImage" + DateTime.Now.Millisecond.ToString() + ".bmp");

            stopWatch.Stop();
            double elapsed = (double)stopWatch.ElapsedMilliseconds / 1000.0;
            double fps = (1 / elapsed);

            return ms.ToArray();
        }
        
        private Byte[] Quantize(Bitmap source)
        {
            Byte[] byteArray = null;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();


            OctreeQuantizer Quantizer = new OctreeQuantizer(255, 8);
            Bitmap bmp = Quantizer.Quantize(source);
            bmp.Save("D:\\Temp\\QuanImage" + DateTime.Now.Millisecond.ToString() + ".bmp");

            using (MemoryStream stream = new MemoryStream())
            {
                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                stream.Close();

                byteArray = stream.ToArray();
            }


            stopWatch.Stop();
            double elapsed = (double)stopWatch.ElapsedMilliseconds / 1000.0;
            double fps = (1 / elapsed);


            return byteArray;
        }

        public Image ResizeImage(Image image, int width, int height, string name)
        {
            byte[] arr;
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                arr = ms.ToArray();
            }

            ISupportedImageFormat format = new JpegFormat { Quality = 100 };
            Size size = new Size(width, 0);

            using (MemoryStream inStream = new MemoryStream(arr))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                    using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                    {
                        // Load, resize, set the format and quality and save an image.
                        imageFactory.Load(inStream)
                                    .Resize(size)
                                    // .Format(format)
                                    .Save(outStream);
                    }

                    Image img = Bitmap.FromStream(outStream);

                    img.Save("D:\\Temp\\" + name + DateTime.Now.Millisecond.ToString() + ".jpg");
                    Globals.TravelImage.ByteArray = outStream.ToArray();


                    return img;
                    // Do something with the stream.
                }
            }






            //var destRect = new Rectangle(0, 0, width, height);
            //var destImage = new Bitmap(width, height);

            //destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //using (var graphics = Graphics.FromImage(destImage))
            //{
            //    graphics.CompositingMode = CompositingMode.SourceCopy;
            //    graphics.CompositingQuality = CompositingQuality.HighQuality;
            //    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //    graphics.SmoothingMode = SmoothingMode.HighQuality;
            //    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            //    using (var wrapMode = new ImageAttributes())
            //    {
            //        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
            //        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            //    }
            //}

            //   return destImage;
        }
        
        static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            return codecs.Single(codec => codec.FormatID == format.Guid);
        }
        
        private void StartSendingData()
        {
            try
            {
                while (true)
                {
                    ChkAndTransmitCapturedTiles(Command.ReadResponse, "pri");

                }
            }
            catch (Exception ex)
            {
            }

        }

        public byte[] ToByte(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }
        public static byte[] Compress(byte[] raw)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory,
                CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }


                stopWatch.Stop();

                double elapsed = (double)stopWatch.ElapsedMilliseconds / 1000.0;
                double fps = (1 / elapsed);


                return memory.ToArray();
            }
        }

        bool islocked = false;

        public dojo Difference(Bitmap bmp0, Bitmap bmp1)
        {
            dojo scream = null;
            Bitmap bmp2 = new Bitmap(bmp0.Width, bmp0.Height, bmp0.PixelFormat);
            var bmpData0 = new BitmapData(); var bmpData1 = new BitmapData(); var bmpData2 = new BitmapData();
            try
            {

                if (islocked == true) { islocked = false; return null; }


                int Bpp = bmp0.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 4;

                bmp2 = new Bitmap(bmp0.Width, bmp0.Height, bmp0.PixelFormat);
                bmpData0 = bmp0.LockBits(
                             new Rectangle(0, 0, bmp0.Width, bmp0.Height),
                             ImageLockMode.ReadOnly, bmp0.PixelFormat);
                bmpData1 = bmp1.LockBits(
                              new Rectangle(0, 0, bmp1.Width, bmp1.Height),
                              ImageLockMode.ReadOnly, bmp1.PixelFormat);
                bmpData2 = bmp2.LockBits(
                              new Rectangle(0, 0, bmp2.Width, bmp2.Height),
                              ImageLockMode.ReadWrite, bmp2.PixelFormat);
                islocked = true;
                // MessageBox.Show(bmpData0.Stride.ToString());
                int len = bmpData0.Height * bmpData0.Stride;

                int len1 = bmpData1.Height * bmpData1.Stride;


                //   MessageBox.Show(bmpData0.Stride.ToString());
                bool changed = false;

                byte[] data0 = new byte[len];
                byte[] data1 = new byte[len];
                byte[] data2 = new byte[len];

                Marshal.Copy(bmpData0.Scan0, data0, 0, len);
                Marshal.Copy(bmpData1.Scan0, data1, 0, len);
                Marshal.Copy(bmpData2.Scan0, data2, 0, len);

                scream = new dojo();
                for (int i = 0; i < len; i++)
                {
                    if (data0[i] != data1[i])
                    {
                        scream.pointer.Add(i);
                        scream.data.Add(data1[i]);
                    }
                }

                //  this.Invoke(new Action(() => this.Text = changed.ToString()));
                Marshal.Copy(data2, 0, bmpData2.Scan0, len);
                //bmp0.UnlockBits(bmpData0);
                //bmp1.UnlockBits(bmpData1);
                //bmp2.UnlockBits(bmpData2);


            }
            catch (Exception ex)
            {

            }
            finally
            {
                bmp0.UnlockBits(bmpData0);
                bmp1.UnlockBits(bmpData1);
                bmp2.UnlockBits(bmpData2);
                islocked = false;

            }

            return scream;
        }

        private void WriteScreenSize()
        {

            this.ScreenClientX = Screen.PrimaryScreen.Bounds.Width;
            this.ScreenClientY = Screen.PrimaryScreen.Bounds.Height;
            this.ScreenServerX = Screen.PrimaryScreen.Bounds.Width;
            this.ScreenServerY = Screen.PrimaryScreen.Bounds.Height;
        }

        private Image ResizeImage(Image original, int targetWidth)
        {
            //double percent = (double)original.Width / targetWidth;
            //int destWidth = (int)(original.Width / percent);
            //int destHeight = (int)(original.Height / percent);
            //Bitmap b = new Bitmap(destWidth, destHeight);
            //Graphics g = Graphics.FromImage((Image)b);
            //try
            //{
            //    g.InterpolationMode = InterpolationMode.NearestNeighbor;
            //    g.DrawImage(original, 0, 0, destWidth, destHeight);
            //}
            //finally
            //{
            //    g.Dispose();
            //}

            EncoderParameter myEncoderParameter;
            EncoderParameters myEncoderParameters;
            ImageCodecInfo myImageCodecInfo;
            //  myImageCodecInfo = GetEncoderInfo("image/png");
            Encoder myEncoder;
            myEncoder = Encoder.ColorDepth;
            myEncoderParameters = new EncoderParameters(1);

            myEncoderParameter =
          new EncoderParameter(myEncoder, 25L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            MemoryStream mst = new MemoryStream();
            //  original.Save(mst, myImageCodecInfo, myEncoderParameters);
            Image ib = Image.FromStream(mst);
            return ib;

            //  return (Image)b;
        }

        [Serializable]
        public class dojo
        {
            public dojo()
            {
                pointer = new List<int>();
                data = new List<byte>();
            }

            public List<int> pointer;
            public List<byte> data;

        }

        private void MouseClick(Data data)
        {

            switch (data.cmdCommand)
            {

                case Command.LClick:
                    Helper.mouse_event(Helper.MOUSEEVENTF_LEFTDOWN | Helper.MOUSEEVENTF_LEFTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                    break;
                case Command.LDown:
                    Helper.mouse_event(Helper.MOUSEEVENTF_LEFTDOWN, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                    break;
                case Command.LUp:
                    Helper.mouse_event(Helper.MOUSEEVENTF_LEFTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                    break;

                case Command.RClick:
                    Helper.mouse_event(Helper.MOUSEEVENTF_RIGHTDOWN | Helper.MOUSEEVENTF_LEFTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                    break;
                case Command.RUp:
                    Helper.mouse_event(Helper.MOUSEEVENTF_RIGHTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                    break;

                case Command.RDown:
                    Helper.mouse_event(Helper.MOUSEEVENTF_RIGHTDOWN, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                    break;

            }

        }

        private void MouseMove(Data data)
        {

            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(data.Image.ByteArray, 0, data.Image.ByteArray.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            if (obj != null)
            {
                int[] arr = (int[])obj;
                Cursor.Position = new Point(arr[0], arr[1]);
            }
        }


        public void stop()
        {
            run = false;
            ListenService.SendLidgrenMessage(new Data() { cmdCommand = Command.Disconnect }, ListenService.GetPartnerEndPoint());

        }
    }

}
