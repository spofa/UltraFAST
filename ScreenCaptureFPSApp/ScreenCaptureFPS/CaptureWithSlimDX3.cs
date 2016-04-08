/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 25-Mar-16
 * Time: 2:54 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D9;

namespace ScreenCaptureFPS
{
	/// <summary>
	/// Copy of CaptureWithSlimDX (uses faster bmp conversion)
	/// </summary>
	public class CaptureWithSlimDX3 : ACaptureBase 
	{
		public override double CaptureFrames(int numAttempts)
		{    
		    int Width = Screen.PrimaryScreen.Bounds.Width;
			int Height = Screen.PrimaryScreen.Bounds.Height;
			
			//Input Parameters & Create Device & Surface
			PresentParameters PPrams = new PresentParameters();
			PPrams.Windowed = true;
			PPrams.SwapEffect = SwapEffect.Discard;	
				
			Device Dvc = new Device(new Direct3D(), 
			                        0, 
			                        DeviceType.Hardware, 
			                        IntPtr.Zero, 
			                        CreateFlags.SoftwareVertexProcessing, 
			                        PPrams);
			
			Surface Surf = Surface.CreateOffscreenPlain(Dvc, 
			                                            Width, 
			                                            Height, 
			                                            Format.A8R8G8B8, 
			                                            Pool.SystemMemory);
			Stopwatch stopWatch = new Stopwatch();
		    stopWatch.Start();
		    
		    for (int i = 0; i < numAttempts; i++)
		    {
		    	//Can't lock
	            Dvc.GetFrontBufferData(0, Surf);
				Bitmap bmp = new Bitmap(SlimDX.Direct3D9.Surface.ToStream(Surf, SlimDX.Direct3D9.ImageFileFormat.Bmp));
		    }
		    
		  	stopWatch.Stop();
		  	
		    Surf.Dispose();
		    Dvc.Dispose();
		    
		    double elapsed = (double)stopWatch.ElapsedMilliseconds / 1000.0;
		    double fps = (numAttempts / elapsed);
		    return fps;			
		}
	}
}
