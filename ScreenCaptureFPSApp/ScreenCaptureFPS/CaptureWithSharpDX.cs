/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 25-Mar-16
 * Time: 1:58 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Drawing;
//using SharpDX;
//using SharpDX.Direct3D9;
using System.Runtime.InteropServices;

namespace ScreenCaptureFPS
{
	/// <summary>
	/// SharpDX: Lightweight native driver for directx (http://sharpdx.org/ and http://sharpdx.org/wiki/class-library-api/)
	/// Samples: https://github.com/sharpdx/SharpDX-Samples
	/// http://www.floschnell.de/computer-science/super-fast-screen-capture-with-windows-8.html
	/// </summary>
	public class CaptureWithSharpDX : ACaptureBase
	{		
		public override double CaptureFrames(int numAttempts)
		{
			//Below code did not worked
			return -1.0;			
//			
//Using DX11 --> http://www.floschnell.de/computer-science/super-fast-screen-capture-with-windows-8.html
//This code did not compiled
//
//			uint numAdapter = 0; // # of graphics card adapter
//			uint numOutput = 0; // # of output device (i.e. monitor)
//			
//			// create device and factory
//			SharpDX.Direct3D11.Device device = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware);
//			Factory1 factory = new Factory1();
//			
//			// creating CPU-accessible texture resource
//			SharpDX.Direct3D11.Texture2DDescription texdes = new SharpDX.Direct3D11.Texture2DDescription();
//			texdes.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.Read;
//			texdes.BindFlags = SharpDX.Direct3D11.BindFlags.None;
//			texdes.Format = Format.B8G8R8A8_UNorm;
//			texdes.Height = factory.Adapters1[numAdapter].Outputs[numOutput].Description.DesktopBounds.Height;
//			texdes.Width = factory.Adapters1[numAdapter].Outputs[numOutput].Description.DesktopBounds.Width;
//			texdes.OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None;
//			texdes.MipLevels = 1;
//			texdes.ArraySize = 1;
//			texdes.SampleDescription.Count = 1;
//			texdes.SampleDescription.Quality = 0;
//			texdes.Usage = SharpDX.Direct3D11.ResourceUsage.Staging;
//			SharpDX.Direct3D11.Texture2D screenTexture = new SharpDX.Direct3D11.Texture2D(device, texdes);
//			
//			// duplicate output stuff
//			Output1 output = new Output1(factory.Adapters1[numAdapter].Outputs[numOutput].NativePointer);
//			OutputDuplication duplicatedOutput = output.DuplicateOutput(device);
//			Resource screenResource = null;
//			SharpDX.DataStream dataStream;
//			Surface screenSurface;
//			
//			int i = 0;
//			Stopwatch sw = new Stopwatch();
//			sw.Start();
//			
//			while (true)
//			{
//				i++;
//				// try to get duplicated frame within given time
//				try
//				{
//					OutputDuplicateFrameInformation duplicateFrameInformation;
//					duplicatedOutput.AcquireNextFrame(1000, out duplicateFrameInformation, out screenResource);
//				}
//				catch (SharpDX.SharpDXException e)
//				{
//					if (e.ResultCode.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
//					{
//						// this has not been a successful capture
//						// thanks @Randy
//						i--;
//			
//						// keep retrying
//						continue;
//					}
//					else
//					{
//						throw e;
//					}
//				}
//			
//				// copy resource into memory that can be accessed by the CPU
//				device.ImmediateContext.CopyResource(screenResource.QueryInterface(), screenTexture);
//				// cast from texture to surface, so we can access its bytes
//				screenSurface = screenTexture.QueryInterface();
//			
//				// map the resource to access it
//				screenSurface.Map(MapFlags.Read, out dataStream);
//			
//				// seek within the stream and read one byte
//				dataStream.Position = 4;
//				dataStream.ReadByte();
//			
//				// free resources
//				dataStream.Close();
//				screenSurface.Unmap();
//				screenSurface.Dispose();
//				screenResource.Dispose();
//				duplicatedOutput.ReleaseFrame();
//			
//				// print how many frames we could process within the last second
//				// note that this also depends on how often windows will &gt;need&lt; to redraw the interface
//				if (sw.ElapsedMilliseconds > 1000)
//				{
//					Console.WriteLine(i + "fps");
//					sw.Reset();
//					sw.Start();
//					i = 0;
//				}
//			}			
		}
	}
}
