/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 25-Mar-16
 * Time: 1:47 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Drawing;

namespace ScreenCaptureFPS
{
	/// <summary>
	/// Description of CaptureWithNETProviders.
	/// </summary>
	public class CaptureWithNETProviders : ACaptureBase 
	{
		public override double CaptureFrames(int numAttempts)
		{
		    Stopwatch stopWatch = new Stopwatch();
		    stopWatch.Start();
		    
		    for (int i = 0; i < numAttempts; i++)
		    {
				Bitmap desktopBMP = new Bitmap(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
		    	                               System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
 
        		Graphics g = Graphics.FromImage(desktopBMP);
 
        		g.CopyFromScreen(0, 0, 0, 0,
           			new Size(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
        		             System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height));
        		
        		g.Dispose();
		    }
		    
		    stopWatch.Stop();
		    
		    double elapsed = (double)stopWatch.ElapsedMilliseconds / 1000.0;
		    double fps = (numAttempts / elapsed);
		    return fps;
		}
	}
}
