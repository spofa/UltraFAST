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
	/// Description of CaptureWithSlimDX.
	/// </summary>
	public class CaptureWithSlimDX : ACaptureBase 
	{
		public override double CaptureFrames(int numAttempts)
		{
			Stopwatch stopWatch = new Stopwatch();
		    stopWatch.Start();
		    
		    int Width = Screen.PrimaryScreen.Bounds.Width;
			int Height = Screen.PrimaryScreen.Bounds.Height;
			
			//Input Parameters & Create Device
			PresentParameters PPrams = new PresentParameters();
			PPrams.Windowed = true;
			PPrams.SwapEffect = SwapEffect.Discard;	
				
			Device Dvc = new Device(new Direct3D(), 
			                        0, 
			                        DeviceType.Hardware, 
			                        IntPtr.Zero, 
			                        CreateFlags.SoftwareVertexProcessing, 
			                        PPrams);
			
		    for (int i = 0; i < numAttempts; i++)
		    {
				//Create surface
				Surface Surf = Surface.CreateOffscreenPlain(Dvc, 
				                                            Width, 
				                                            Height, 
				                                            Format.A8R8G8B8, 
				                                            Pool.Scratch);
	            Dvc.GetFrontBufferData(0, Surf);
	
	            //Data Rectangle (Capturing Area) and Data Stream
				DataRectangle dr = Surf.LockRectangle(LockFlags.None);
				DataStream gStream = dr.Data;

				//Convert Data Stream to Image
				var b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
				var BoundsRect = new Rectangle(0, 0, Width, Height);
				BitmapData bmpData = b.LockBits(BoundsRect, ImageLockMode.WriteOnly, b.PixelFormat);
				int bytes = bmpData.Stride * b.Height;
			
				var rgbValues = new byte[bytes * 4];
			
				// copy bytes from the surface's data stream to the bitmap stream
				for (int y = 0; y < Height; y++)
				{
					for (int x = 0; x < Width; x++)
					{
						gStream.Seek(y * (Width * 4) + x * 4, System.IO.SeekOrigin.Begin);
						gStream.Read(rgbValues, y * (Width * 4) + x * 4, 4);
					}
				}
		
				Marshal.Copy(rgbValues, 0, bmpData.Scan0, bytes);
				b.UnlockBits(bmpData);
				
				//Unlock And Dispose Data R
				Surf.UnlockRectangle();
				Surf.Dispose();
		    }
		    
		    stopWatch.Stop();
		    
		    double elapsed = (double)stopWatch.ElapsedMilliseconds / 1000.0;
		    double fps = (numAttempts / elapsed);
		    return fps;			
		}
	}
}
