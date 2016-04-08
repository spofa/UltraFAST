/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 25-Mar-16
 * Time: 4:00 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace ScreenCaptureFPS
{
	/// <summary>
	/// Based On http://www.codeproject.com/Articles/12898/Screen-Capturing
	/// </summary>
	public class CaptureWithNETFns : ACaptureBase 
	{
		public override double CaptureFrames(int numAttempts)
		{
		    Stopwatch stopWatch = new Stopwatch();
		    
		    //Array of all display on system
		    Screen[] Scrs = Screen.AllScreens;
		    
		    //Setup area to capture
		    Rectangle RGle = Screen.PrimaryScreen.Bounds;
//		    = SystemInformation.VirtualScreen;
//		    = Screen.PrimaryScreen.WorkingArea;
		   
		    stopWatch.Start();
		    
		    for (int i = 0; i < numAttempts; i++)
		    {
		    	//Bitmap image 
				Bitmap memBmp = new Bitmap(RGle.Width, 
		    	                           RGle.Height,
		    	                           PixelFormat.Format32bppArgb);
		    	//Get graphics of bitmap image
		    	using(Graphics bmpGraph = Graphics.FromImage(memBmp))
		    	{
		    		//Copy screen to bitmap grpahics
		    		bmpGraph.CopyFromScreen(RGle.X, 
		    		                        RGle.Y,
		    		                        0,
		    		                        0,
		    		                        RGle.Size,
		    		                        CopyPixelOperation.SourceCopy);
		    		                        
		    	}
		    }
		    
		    stopWatch.Stop();
		    
		    double elapsed = (double)stopWatch.ElapsedMilliseconds / 1000.0;
		    double fps = (numAttempts / elapsed);
		    return fps;
		}
	}
}
