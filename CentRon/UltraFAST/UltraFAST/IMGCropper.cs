/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 29-Mar-16
 * Time: 2:33 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media.Imaging;


//SEE: http://www.emgu.com/wiki/index.php/Setting_up_EMGU_C_Sharp
//NET DLLs for Emgu.CV (OpenCV) Wrapper Whose Reference Is Added:
//    Emgu.CV.dll
//    Emgu.CV.UI.dll 
//    Emgu.Util.dll  
//You also need to add always copy to output directory (C++) opencv
//files: opencv_core220.dll and opencv_imgproc220.dll
//NET Emgu.CV's Classes Those Are Used In This Project (First Three Are Basics)
//using Emgu.CV;
//using Emgu.CV.UI;
//using Emgu.CV.Structure;

namespace UltraFAST
{
	/// <summary>
	/// IMGCropper: Crop ReSized (Managable Sized) Image in (MainApplication.bmpReduced) 
	/// into regions in (MainApplication.DSKTopRegions.CroppedImage)
	/// </summary>
	public class IMGCropper : AHandler 
	{
				
		/// <summary>
		/// !!! FPS for 100 Frames !!!
		/// Win32Native - Serial: 11.5, Parallel: 9.5
		/// AForgeNET   - Serial: 12.0, Parallel: 11.5
		/// EmguCV = 2, (Could notget its dll)
		/// </summary>
		private TypeOfIMGAlgos LibraryToUse = TypeOfIMGAlgos.AForgeNET;
		
		/// <summary>
		/// Cropped bitmap image
		/// </summary>
		public Bitmap bmpCaptured {get; set;}
		
		/// <summary>
		/// If true saved cropped regions to C:\TEMP\*.*
		/// </summary>
		public bool SaveOutputTilesToDisk = Program.SaveAllOutputsToDiskInTEMPFolder;
		
		/// <summary>
		/// If true uses prallel for cloning, cropping etc.
		/// </summary>
		public bool IsParallelApproach = false;		

		public override bool Execute(ref object Input)
		{
			//Speed Up This Thread To High Priority
			System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
			
			//Recieve the inputs (MainApplication.gCapture.bmpCaptured) and its Regions 
			Bitmap InpWrkBitmap = MainApplication.bmpReduced;
			List<ROIArea> lstRegions = MainApplication.DSKTopTileRegions;

			//: FlipY source image as it is negative stride before cropping
			bool IsSrcNegativeStride = MainApplication.IsSrcNegativeStride;
			if(IsSrcNegativeStride)
			{
				InpWrkBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
			}
			
			//Clone Bitmaps To Regions [[To Avoid BitmapAlreadyLocked Exception on Source Image among Threads]]
			Parallel.ForEach<ROIArea>(lstRegions, roiArea =>
			{
              	if(IsParallelApproach)
              	{
              		//Clone if parallel cropping to avoid bitmap locked error
					roiArea.ParentBitmap = (Bitmap)InpWrkBitmap.Clone();
			    }
              	else
              	{
              		//Dont clone just point if sequential cropping
              		roiArea.ParentBitmap = InpWrkBitmap;
              	}
			});
			
			//SERIAL OR PARALLEL PROCESSING OF EACH REGION
			if(!IsParallelApproach)
			{
				foreach(ROIArea roiArea in lstRegions)
				{
					CropRegionsToROIArea(roiArea, IsSrcNegativeStride);
				}
			}
			else
			{
				Parallel.ForEach<ROIArea>(lstRegions, roiArea => CropRegionsToROIArea(roiArea, IsSrcNegativeStride));				
			}

			//As individual copies of InpBitmap in roiArea.SourceImage are cleared to save RAM. Point all roiArea.SourceImage
			//to InpBitmap 
			Parallel.ForEach<ROIArea>(lstRegions, roiArea => 
			{ 
				roiArea.ParentBitmap = InpWrkBitmap;
			});
		
			
			//PARALLEL: Save cropped region (tile) to disk in C drive		
			if(SaveOutputTilesToDisk)
			{
				Parallel.ForEach<ROIArea>(lstRegions, roiArea =>
				{
					//Save this current cropped image to disk
					String fName = (Program.SaveOutputToDiskTEMPFolder + "TILE_" + roiArea.RgnIndex.ToString() + "_Extracted.bmp");
					System.IO.File.Delete(fName);
					roiArea.CroppedTileBitmap.Save(fName);
				});				
			};
			
			return true;
		}

		private void CropRegionsToROIArea(ROIArea roiArea, bool IsSrcNegStride)
		{
			//Get bounding rectangle from roi area
			Rectangle rectROI = roiArea.ConvertToRectangle();

			//Remove older cropped image
			roiArea.CroppedTileBitmap = null;
			
			//Variable to hold cropped image of tile at (row, col) 
			//represented in regions
			Bitmap iRegionCroppedTileBmp = null;				
			
			//Crop image using suitable library to have fastest speed
         	switch(LibraryToUse)
			{
         		case TypeOfIMGAlgos.Win32Native:
					//Crop using LockBits and Pixel Copying
					iRegionCroppedTileBmp = Win32NativeCrop(roiArea.ParentBitmap, rectROI, IsSrcNegStride);
					break;
					
				case TypeOfIMGAlgos.AForgeNET:
					//Crop using AForge.NET library [http://www.aforgenet.com/framework/docs/html/197f820b-f5d0-57cb-a509-5dbcacdda446.htm]
					AForge.Imaging.Filters.Crop iCropFilter = new AForge.Imaging.Filters.Crop(rectROI);
					iRegionCroppedTileBmp = iCropFilter.Apply(roiArea.ParentBitmap);
					if(IsSrcNegStride)
					{
						iRegionCroppedTileBmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
					}
					break;					
				default:
					//Crop using LockBits and Pixel Copying
					iRegionCroppedTileBmp = Win32NativeCrop(roiArea.ParentBitmap, rectROI, IsSrcNegStride);
					break;
         	}
         	
			//Save cropped image to region
			roiArea.CroppedTileBitmap = iRegionCroppedTileBmp;
			
			//Clear SourceBitmap of roiArea to save RAM in parallel approach as they're 
			//clones and eating RAM
			if(IsParallelApproach)
			{
				roiArea.ParentBitmap.Dispose();
				roiArea.ParentBitmap = null;
			}
		}
		
		/// <summary>
		/// Crop bitmap into rectangle, FlipY all rectangles if negative stride. Input bitmap has to be positive stride
		/// [[ Bitmap can be Top-Down or Bottom-Up (https://msdn.microsoft.com/en-us/library/windows/desktop/aa473780%28v=vs.85%29.aspx) ]]
		/// 
		/// FAST Processing: http://csharpexamples.com/fast-image-processing-c/
		/// 
		/// </summary>
		/// <param name="srcBitmap"></param>
		/// <param name="ROIRect"></param>
		/// <param name="IsCapturedAsNegativeStride">True if original capture was flipped</param>
		/// <param name="UseParallelProcessing">true to copy using parallel threads (default: false)</param>
		/// <returns></returns>
		private unsafe Bitmap Win32NativeCrop(Bitmap srcBitmap, Rectangle ROIRect, 
		                                      bool IsCapturedSrcImageWasNegativeStride, 
		                                      bool UseParallelProcessing = false)
		{		
			//If larger ROI return original image
			if ((srcBitmap.Width <= ROIRect.Width) && (srcBitmap.Height <= ROIRect.Height))
			{
				return srcBitmap;
			}
	
			//Fetch parameters from source bitmap
			int srcWidth = srcBitmap.Width;
			int srcHeight = srcBitmap.Height;
			PixelFormat srcPixelFormat = srcBitmap.PixelFormat;
			
			//Lock source image for faster processing (get its bitmapdata pointer)
			var srcImgBitmapData = srcBitmap.LockBits(new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height),
				                                      ImageLockMode.ReadOnly,
				                                      srcBitmap.PixelFormat);
			//Image can't be negative stride
			if(srcImgBitmapData.Stride < 0)
			{
				//Unlock before flipping image
				srcBitmap.UnlockBits(srcImgBitmapData);
				
				//Throw the error
				throw new Exception("Input Image Can't Be Negative Stride, FlipY Image First");
			}
			
			//Create a blank target image for copying
			var dstImg = new Bitmap(ROIRect.Width, ROIRect.Height, srcPixelFormat);
			
			//Lock destination image for faster processing (get its bitmapdata pointer)
			var dstImgBitmapData = dstImg.LockBits(new Rectangle(0, 0, dstImg.Width, dstImg.Height), 
			                                       ImageLockMode.WriteOnly, 
			                                       dstImg.PixelFormat);
			
			//Compute bits per pixel for image
			//Height:The pixel height of the Bitmap object. Also sometimes referred to as the number of scan lines. 
			//Width :The pixel width of the Bitmap object. This can also be thought of as the number of pixels in one scan line.
			//Stride:The stride width (also called scan width) of the Bitmap object
			//       (Is Stride is Negative Bitmap is Bottom up)
			//Scan0 :The address of the first pixel data in the bitmap. This can also be thought of as the first scan line in the bitmap.
			//IMAGE RESOLUTION CAN BE - 8/16/24/32/48/64 Bits
			//THUS BPP                = 1/2 / 3/ 4/ 6/ 8
			int bpp = srcImgBitmapData.Stride / srcImgBitmapData.Width;
			
			//Fetch address of first pixels of source and destination bitmaps
			IntPtr srcIntPtr = srcImgBitmapData.Scan0;
			IntPtr dstIntPtr = dstImgBitmapData.Scan0;
			var srcPtr = (byte *)srcIntPtr.ToPointer();
			var dstPtr = (byte *)dstIntPtr.ToPointer();
			
			//Compute pointer of top left corner of ROI in bitmap
			var srcTLPtr = srcPtr + ROIRect.Y * srcImgBitmapData.Stride + ROIRect.X * bpp;
			var dstTLPtr = dstPtr;
			
			//Compute values of stride (maybe -ive)
			var srcStride = srcImgBitmapData.Stride;
			var dstStride = dstImgBitmapData.Stride;
			
			//Copy each raster line from source bitmap to target bitmap
			if(UseParallelProcessing)
			{
				//Parallel Copy Each Line From Source Bitmap To Target Bitmap
				Parallel.For(0, ROIRect.Height, y =>
				{
					//Calculate Starting Pointer Of This Line
					var srcLineLeftPtr = srcTLPtr + y * (srcStride);
					var dstLineLeftPtr = dstTLPtr + y * (dstStride);
					//Fast Copy Memory Bytes (Using UnSafe Code)
					StuffMem.MemCopy(dstLineLeftPtr, srcLineLeftPtr, dstStride);
				});
			}
			else
			{
				//Sequential Copy Each Line From Source Bitmap To Target Bitmap
				for(int y = 0; y < ROIRect.Height; y++)
				{
					//Copy one line from bitmap
					int nBytesInROI = ROIRect.Width * bpp;
					StuffMem.MemCopy(dstTLPtr, srcTLPtr, dstStride);
					//Shift top left pointer to next line
					srcTLPtr += srcStride;
					dstTLPtr += dstStride;
				}
			}
			
			//Unlock source and destination images (Job Done)
			srcBitmap.UnlockBits(srcImgBitmapData);
			dstImg.UnlockBits(dstImgBitmapData);
			
			//Flip dstImage for negative stride input images
			if(IsCapturedSrcImageWasNegativeStride)
			{
				dstImg.RotateFlip(RotateFlipType.RotateNoneFlipY);
			}
			
			//Return cropped image from method to caller
			return dstImg;
         }		
	}
}
