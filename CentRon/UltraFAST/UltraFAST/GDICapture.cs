/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 28-Mar-16
 * Time: 5:24 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using ImageProcessor.Processors;

namespace UltraFAST
{
	/// <summary>
	/// Capture using GDI.dll of Windows (Very Fast), Better than DirectX
	/// : Saves output to this.bmpCaptured and this.capturedAt
	/// </summary>
	public class GDICapture : AHandler 
	{
		/// <summary>
		/// Captured bitmap object
		/// </summary>
		public Bitmap bmpCaptured {get; set;}
		
		/// <summary>
		/// Time of capture from screen
		/// </summary>
		public DateTime capturedAt {get; set;}
		
		/// <summary>
		/// Flips image as per setting
		///  Default: RotateNoneFlipNone
		///  FlipY: RotateNoneFlipY
		/// </summary>
		public RotateFlipType FlipCapturedImage = RotateFlipType.RotateNoneFlipNone;
		
		/// <summary>
		/// If true saved captured regions to C:\capture*.bmp
		/// </summary>
		public bool SaveCapturedImageToDisk = Program.SaveAllOutputsToDiskInTEMPFolder;
		
		/// <summary>
		/// Ultra Fast Screen Capture (Desktop (Whole Screen) to Bitmap) using GDI32
		/// BASED ON: http://www.codeproject.com/Articles/3024/Capturing-the-Screen-Image-in-C
		/// </summary>
		/// <param name="Input">typeof(Region) on desktop to capture</param>
		/// <returns></returns>
		public override bool Execute(ref object Input)
		{
			//BitBlt is windows method which copies a block (rectangle) of color data 
			//from as source device context to target device context. This method can
			//copy rectangle to memory as well as paste to device.
			//
			//- http://www.codeproject.com/Articles/6710/Using-BitBlt-to-Copy-and-Paste-Graphics
			//- http://www.codeproject.com/Articles/5736/Copying-graphics-with-BitBlt-NET-Style
			//
			// Device Context -> Can be get from graphics object, which is availaible on any NET object
			// like Graphics gPS = PictureBox1.CreateGraphics(); and 
			//      IntPtr MyDC = gPS.GetHdc(); Here HDC is handle of device context.
			// Graphics can be obtained from Bitmaps also (where we can copy)
			//      Bitmap Bmp = new Bitmap(Width, Height, gPS)
			//      memGOBJ = Graphics.FromImage(Bmp)
			//RULES:
			// - If using device context, can't write to graphics object. First call 
			//      gPS.ReleadeHdc(MyDC)
			// - Need to dispose gPS also to save leakage
			// 
			
			//BASED ON: http://www.codeproject.com/Articles/3024/Capturing-the-Screen-Image-in-C
			//
			try
			{
				//Speed Up This Thread To High Priority
				System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
				
				//Get the region object out (area to capture from desktop)
				ROIArea Rgn = (ROIArea)Input;
				
				//Get device context and its handle for Desktop
				IntPtr DCDesktop = StuffWin32.GetDC(StuffWin32.GetDesktopWindow());
				IntPtr hDCDesktop = StuffGDI.CreateCompatibleDC(DCDesktop);
				
				//We create a compatibe Bitmap of the region size compatible to Desktop's Device Context
				IntPtr hBitmap = StuffGDI.CreateCompatibleBitmap(DCDesktop, Rgn.Width, Rgn.Height);
				
				//Check for NULL with IntPtr.Zero
				if(hBitmap != IntPtr.Zero)
				{
					//Set this compatible Bitmap from Desktop device's handle (backup original handle)
					IntPtr hDCOldDesktop = (IntPtr)StuffGDI.SelectObject(hDCDesktop, hBitmap);
					
					//Copy the Region from Desktop Device Context (DCDesktop) to Handle Of Bitmap at (0,0)
					StuffGDI.BitBlt(hDCDesktop, 0, 0, Rgn.Width, Rgn.Height, DCDesktop, Rgn.Left, Rgn.Top, TernaryRasterOperations.SRCCOPY);
					DateTime DTStamp = DateTime.Now;
					
					//Restore original Bitmap for Desktop's Device
					StuffGDI.SelectObject(hDCDesktop, hDCOldDesktop);
					
					//We need to delete the desktop device context handle
					StuffGDI.DeleteDC(hDCDesktop);
					
					//We need to release the device context from memory
		            StuffWin32.ReleaseDC(StuffWin32.GetDesktopWindow(), DCDesktop);
		            
		            //Convert bitmap to NET image and release UnManaged image
		            Bitmap bmpConverted = System.Drawing.Image.FromHbitmap(hBitmap);
		            
					//If we're capturing rotated or inverted image flip+rotate correct it
		            if(FlipCapturedImage != RotateFlipType.RotateNoneFlipNone)
		            {
						bmpConverted.RotateFlip(RotateFlipType.RotateNoneFlipY);
		            }
		            
		            //Update Image to Central Location
		            bmpCaptured	=  bmpConverted;   		            
		            capturedAt = DTStamp;
		            
		            //Compute and Setup Stride Of Image
		            bool IsSrcNegativeStride = StuffImaging.IsNegativeStrideImage(bmpCaptured);
		            MainApplication.IsSrcNegativeStride = IsSrcNegativeStride;
		            
		            //Release and Collect
		            StuffGDI.DeleteObject(hBitmap);
		            GC.Collect();
		    
					//Save the captured image to disk is enabled            
		            if(SaveCapturedImageToDisk)
		            {
		            	String fName = Program.SaveOutputToDiskTEMPFolder + "imgGDICapture.bmp";
						System.IO.File.Delete(fName);
						bmpCaptured.Save(fName);		            	
		            }
		            			
		            //Success Image Captured
		            return true;
				}
				else
				{
					//Not able to capture screen
					throw new Exception("Not able to capture screen, hBitmap == IntPtr.Zero)");
				}
			}
			catch
			{
				//Write to log here	
				return false;
			}
		}
	}
}
