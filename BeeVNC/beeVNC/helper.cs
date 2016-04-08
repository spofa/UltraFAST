/************************************
 * Developed by Kristian Reukauff
 * License and Project:
 * https://beevnc.codeplex.com/
 * Published under NewBSD-License
 * without any warrenties
 * provided "as is"
 ************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.IO;

namespace beeVNC
{
    public class Helper
    {
        public static UInt32 ConvertToUInt32(byte[] data, bool isBigEndian)
        {
            if (isBigEndian == false)
                data = ReverseBytes(data);

            return(BitConverter.ToUInt32(data,0));
        }

        public static UInt16 ConvertToUInt16(byte[] data, bool isBigEndian)
        {
            if (isBigEndian == false)
                data = ReverseBytes(data);

            return (BitConverter.ToUInt16(data, 0));
        }

        public static Int32 ConvertToInt32(byte[] data, bool isBigEndian)
        {
            if (isBigEndian == false)
                data = ReverseBytes(data);

            return (BitConverter.ToInt32(data, 0));
        }

        public static Byte[] ConvertToByteArray(UInt16 data, bool isBigEndian)
        {
            Byte[] ret = BitConverter.GetBytes(data);

            if (isBigEndian == false)
                ret = ReverseBytes(ret);
            
            return (ret);
        }

        public static Byte[] ConvertToByteArray(UInt32 data, bool isBigEndian)
        {
            Byte[] ret = BitConverter.GetBytes(data);

            if (isBigEndian == false)
                ret = ReverseBytes(ret);

            return (ret);
        }

        public static Byte[] ConvertToByteArray(Int32 dataSigned, bool isBigEndian)
        {
            Byte[] ret = BitConverter.GetBytes(dataSigned);

            if (isBigEndian == false)
                ret = ReverseBytes(ret);

            return (ret);
        }

        public static Byte[] ConvertToByteArray(bool data)
        {
            return (BitConverter.GetBytes(data));
        }


        private static byte[] ReverseBytes(byte[] inArray)
        {
            byte temp;
            int highCtr = inArray.Length - 1;

            for (int ctr = 0; ctr < inArray.Length / 2; ctr++)
            {
                temp = inArray[ctr];
                inArray[ctr] = inArray[highCtr];
                inArray[highCtr] = temp;
                highCtr -= 1;
            }
            return inArray;
        }

        public static bool IsInDesignMode()
        {
            System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();
            bool res = process.ProcessName == "devenv";
            process.Dispose();
            return res;
        }

        public static void CreateThumbnail(string filename, BitmapSource image5)
        {
            if (filename != string.Empty)
            {
                using (FileStream stream5 = new FileStream(filename, FileMode.Create))
                {
                    PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                    encoder5.Frames.Add(BitmapFrame.Create(image5));
                    encoder5.Save(stream5);
                    stream5.Close();
                }
            }
        }
    }
}
