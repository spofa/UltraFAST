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
		        IntPtr DCDesktop = Win32Stuff.GetDC(Win32Stuff.GetDesktopWindow());
		        IntPtr hDCDesktop = GDIStuff.CreateCompatibleDC(DCDesktop);
		 
		        size.cx = Win32Stuff.GetSystemMetrics
		                  (Win32Stuff.SM_CXSCREEN);
		 
		        size.cy = Win32Stuff.GetSystemMetrics
		                  (Win32Stuff.SM_CYSCREEN);
		 
		        hBitmap = GDIStuff.CreateCompatibleBitmap(DCDesktop, size.cx, size.cy);
		 
		        if (hBitmap != IntPtr.Zero)
		        {
		            IntPtr hDCOldDesktop = (IntPtr)GDIStuff.SelectObject
		                                   (hDCDesktop, hBitmap);
		 
		            GDIStuff.BitBlt(hDCDesktop, 0, 0, size.cx, size.cy, DCDesktop,
		                                           0, 0, GDIStuff.SRCCOPY);
		 
		            GDIStuff.SelectObject(hDCDesktop, hDCOldDesktop);
		            GDIStuff.DeleteDC(hDCDesktop);
		            Win32Stuff.ReleaseDC(Win32Stuff.GetDesktopWindow(), DCDesktop);
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
