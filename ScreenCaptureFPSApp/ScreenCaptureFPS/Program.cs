/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 25-Mar-16
 * Time: 1:21 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace ScreenCaptureFPS
{
	class Program
	{
		//Captures screens using many methods and reports speed
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			
			int framestoget = 100;
			
			CaptureWithNETFns NETFn = new CaptureWithNETFns();
			Console.WriteLine(String.Format("FPS #{0}", NETFn.CaptureFrames(framestoget).ToString()));
			
			CaptureWithSlimDX3 SDx3 = new CaptureWithSlimDX3();
			Console.WriteLine(String.Format("FPS #{0}", SDx3.CaptureFrames(framestoget).ToString()));

			CaptureWithSlimDX2 SDx2 = new CaptureWithSlimDX2();
			Console.WriteLine(String.Format("FPS #{0}", SDx2.CaptureFrames(framestoget).ToString()));
			
			CaptureWithSlimDX SDx = new CaptureWithSlimDX();
			Console.WriteLine(String.Format("FPS #{0}", SDx.CaptureFrames(framestoget).ToString()));
			
			CaptureWithWin32API W = new CaptureWithWin32API();
			Console.WriteLine(String.Format("FPS #{0}", W.CaptureFrames(framestoget).ToString()));
			
			CaptureWithNETProviders N = new CaptureWithNETProviders();
			Console.WriteLine(String.Format("FPS #{0}", N.CaptureFrames(framestoget).ToString()));
				
			// TODO: Implement Functionality Here
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
}