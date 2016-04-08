using System;
using System.Diagnostics;
using System.Drawing;
using UltraFAST;

namespace RemoteClient
{
    public static class CaptureWithWin32API  
    {
        public static   Bitmap CaptureFrames()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            int numAttempts = 1;

            SIZE size;
            IntPtr hBitmap;
            IntPtr hDC = StuffWin32.GetDC(StuffWin32.GetDesktopWindow());
            IntPtr hMemDC = StuffGDI.CreateCompatibleDC(hDC);

            size.cx = StuffWin32.GetSystemMetrics
                      (StuffWin32.SM_CXSCREEN);

            size.cy = StuffWin32.GetSystemMetrics
                      (StuffWin32.SM_CYSCREEN);

            hBitmap = StuffGDI.CreateCompatibleBitmap(hDC, size.cx , size.cy);

            if (hBitmap != IntPtr.Zero)
            {
                IntPtr hOld = (IntPtr)StuffGDI.SelectObject
                                       (hMemDC, hBitmap);

                StuffGDI.BitBlt(hMemDC, 0, 0, size.cx  , size.cy, hDC,
                                               0, 0, TernaryRasterOperations.SRCCOPY);

                StuffGDI.SelectObject(hMemDC, hOld);
                StuffGDI.DeleteDC(hMemDC);
                StuffWin32.ReleaseDC(StuffWin32.GetDesktopWindow(), hDC);
                Bitmap bmp = System.Drawing.Image.FromHbitmap(hBitmap);
                StuffGDI.DeleteObject(hBitmap);
                GC.Collect();
                stopWatch.Stop();


                bmp.Save("D:\\Temp\\NewImage" + DateTime.Now.Millisecond.ToString() + ".bmp");
                return bmp;
            }


            stopWatch.Stop();
            return null;

            
            double elapsed = (double)stopWatch.ElapsedMilliseconds / 1000.0;
            double fps = (numAttempts / elapsed);

        }
    }
}
