using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelObjects
{
    [Serializable]
    public class Sz1080p
    {
        public static Size getSize()
        {
            return new Size(1920, 1080);
        }
    }


    [Serializable]
    public class SzLaptop
    {
        public static Size getSize()
        {
            return new Size(1366,768);
        }
    }


    [Serializable]
    public class Sz720p
    {
        public static Size getSize()
        {
            return new Size(1280, 720);
        }
    }


    [Serializable]
    public class Sz480p
    {
        public static Size getSize()
        {
            return new Size(854, 480);
        }
    }


    [Serializable]
    public class Sz360p
    {
        public static Size getSize()
        {
            return new Size(640, 360);
        }
    }


    [Serializable]
    public class Sz240p
    {
        public static Size getSize()
        {
            return new Size(426, 240);
        }
    }



}
