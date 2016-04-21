using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByTeCounter
{
    /// <summary>
    /// Description of ACaptureBase.
    /// </summary>
    public abstract class ACaptureBase
    {
        public abstract Bitmap CaptureFrames();
    }

    //This structure shall be used to keep the size of the screen.
    public struct SIZE
    {
        public int cx;
        public int cy;
    }
}
