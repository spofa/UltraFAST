
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
//using NVNCLibrary.Encodings;
using System;
using System.Drawing;
using System.IO;
using TravelObjects;

namespace RemoteClient
{
    public static class Cutter
    {

        public static byte[] CreateSmaller(Bitmap image, int width, int height)
        {
            return ResizeImage(image, width, height);
        }

        public static void SliceImage(Byte[] arr)
        {
            Globals.InitializeTileArray();

            using (MemoryStream inStream = new MemoryStream(arr))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                    {
                        int index = 0;
                        for (int r = 0; r < Globals.rows; r++)
                        {
                            for (int c = 0; c < Globals.cols; c++)
                            {

                                int x = (Globals.Width / Globals.cols) * c;
                                int y = (Globals.Height / Globals.rows) * r;

                                int w = (Globals.Width / Globals.cols);
                                int h = (Globals.Height / Globals.rows);
                                Rectangle rect = new Rectangle(x, y, w, h);

                                string coordinate = "("+x.ToString()+","+y.ToString() + "-"+(x+w).ToString() + ","+(y+h).ToString() + ")";

                                imageFactory.Load(inStream).Crop(rect).Save(outStream);


                                if (Globals.TileArray[index] != null) {

                                    Globals.TileArray[index].ByteArray = outStream.ToArray();
                                    Globals.TileArray[index].CapturedTime = DateTime.Now;
                                    Globals.TileArray[index].Left = x;
                                    Globals.TileArray[index].Top = y;


                                }
                                else
                                {
                                    Globals.TileArray[index] = new TravelImage() { ByteArray = outStream.ToArray(), Left = x, Top = y };
                                }


                                index = index + 1;
                                Image img = Image.FromStream(outStream);
                                img.Save("D:\\Temp\\TileImage-" + coordinate + ".bmp");
                            }
                        }
                    }


                }
            }

        }


        public static byte[] ResizeImage(Bitmap image, int width, int height)
        {
            byte[] arr;
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                arr = ms.ToArray();
            }

            
            ISupportedImageFormat format = new JpegFormat { Quality = 100 };
            Size size = new Size(width, height);
            ResizeLayer l = new ResizeLayer(size, ResizeMode.Stretch);
            using (MemoryStream inStream = new MemoryStream(arr))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                    {

                        imageFactory.Load(inStream)
                                    .Resize(l)
                                    .Format(format)
                                    .Save(outStream);
                    }
                    arr = outStream.ToArray();
                }
            }

            return arr;
        }

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

    }

}
