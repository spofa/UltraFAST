/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 25-Mar-16
 * Time: 1:26 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Drawing;

namespace ScreenCaptureFPS
{
	/// <summary>
	/// WIN32 API to capture the screenshot
	/// </summary>
	public class CaptureWithWin32API : ACaptureBase 
	{		
		public override double CaptureFrames(int numAttempts)
		{
		    Stopwatch stopWatch = new Stopwatch();
		    stopWatch.Start();
		    
		    for (int i = 0; i < numAttempts; i++)
		    {
		        SIZE size;
		        IntPtr hBitmap;
		        IntPtr hDC = Win32Stuff.GetDC(Win32Stuff.GetDesktopWindow());
		        IntPtr hMemDC = GDIStuff.CreateCompatibleDC(hDC);
		 
		        size.cx = Win32Stuff.GetSystemMetrics
		                  (Win32Stuff.SM_CXSCREEN);
		 
		        size.cy = Win32Stuff.GetSystemMetrics
		                  (Win32Stuff.SM_CYSCREEN);
		 
		        hBitmap = GDIStuff.CreateCompatibleBitmap(hDC, size.cx / 2, size.cy);
		 
		        if (hBitmap != IntPtr.Zero)
		        {
		            IntPtr hOld = (IntPtr)GDIStuff.SelectObject
		                                   (hMemDC, hBitmap);
		 
		            GDIStuff.BitBlt(hMemDC, 0, 0, size.cx / 2, size.cy, hDC,
		                                           0, 0, GDIStuff.SRCCOPY);
		 
		            GDIStuff.SelectObject(hMemDC, hOld);
		            GDIStuff.DeleteDC(hMemDC);
		            Win32Stuff.ReleaseDC(Win32Stuff.GetDesktopWindow(), hDC);
		            Bitmap bmp = System.Drawing.Image.FromHbitmap(hBitmap);
		            GDIStuff.DeleteObject(hBitmap);
		            GC.Collect();
		            //return bmp;
		        }
		        //return null;
		    }
		    
		    stopWatch.Stop();
		    
		    double elapsed = (double)stopWatch.ElapsedMilliseconds / 1000.0;
		    double fps = (numAttempts / elapsed);
		    return fps;
		}
	}
}
