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

using AFFilters = AForge.Imaging.Filters;

	//ASPECT RATIO: http://www.digitalcitizen.life/what-screen-resolution-or-aspect-ratio-what-do-720p-1080i-1080p-mean
	//4:3 aspect ratio resolutions: 640×480, 800×600, 960×720, 1024×768, 1280×960, 1400×1050, 1440×1080 , 1600×1200, 1856×1392, 1920×1440, and 2048×1536
	//16:10 aspect ratio resolutions: - 1280×800, 1440×900, 1680×1050, 1920×1200 and 2560×1600
	//16:9 aspect ratio resolutions: 1024×576, 1152×648, 1280×720, 1366×768, 1600×900, 1920×1080, 2560×1440 and 3840×2160
			
namespace UltraFAST
{
	/// <summary>
	/// IMGReducer: Captured images in (MainApplication.bmpCaptured) larger than (ReduceToSizeX, ReduceToSizeY) are 
	/// resized to fit into box
	/// : Only images larger than (ReduceToSizeX, ReduceToSizeY) are resized. 
	/// : Default to FHD (1920 x 1080), these can dynamically change.
	/// : Saves output to this.bmpReduced and this.reducedAt
	/// </summary>
	public class IMGResizing : AHandler 
	{		
		/// <summary>
		/// Desktop snapshots larger than (ReduceToSizeX, ReduceToSizeY) are scaled down in width, height or both
		/// in order to be able to transmit. As we can't transmit 4K screen on slow network. Default value is FHD (can change dynamically)
		/// FHD - 1920 X 1080
		/// </summary>
		public static int def_maxAllowedScrSz4TransmitX = 1920;
		
		/// <summary>
		/// Desktop snapshots larger than (ReduceToSizeX, ReduceToSizeY) are scaled down in width, height or both
		/// in order to be able to transmit. As we can't transmit 4K screen on slow network. Default value is FHD (can change dynamically)
		/// FHD - 1920 X 1080
		/// </summary>
		public static int def_maxAllowedScrSz4TransmitY = 1080;

		/// <summary>
		/// Fixed name to limited image
		/// </summary>
		public static string def_saveFileNameDefault = "imgFHDLimited";
				
		/// <summary>
		/// This algorithm will be used for resizing the image (ResizeBilinear: Medium Fast, Work On All Images)
		/// </summary>
		public static TypesOfResizing AlgorithmToUse = TypesOfResizing.ResizeBilinear;
		
		/// <summary>
		/// This algorithm reduce image size to (ReduceToSizeX, ReduceToSizeY) dimension, Deafulted to  (ReduceToSizeX, ReduceToSizeY)
		/// </summary>
		public int ReduceToSizeX = def_maxAllowedScrSz4TransmitX;
		
		/// <summary>
		/// This algorithm reduce image size to (ReduceToSizeX, ReduceToSizeY) dimension, Deafulted to  (ReduceToSizeX, ReduceToSizeY)
		/// </summary>
		public int ReduceToSizeY = def_maxAllowedScrSz4TransmitY;
		
		/// <summary>
		/// Name of file saving
		/// </summary>
		public string SaveFileName = def_saveFileNameDefault;
		
		/// <summary>
		/// If true saved cropped regions to C:\TEMP\*.*
		/// </summary>
		public bool SaveOutputTilesToDisk = Program.SaveAllOutputsToDiskInTEMPFolder;

		/// <summary>
		/// Output reduced bitmap result
		/// </summary>
		public Bitmap bmpReduced {get; set;}
		
		// Time at which reduction done
		public DateTime reducedAt {get; set;}
		
		public override bool Execute(ref object Input)
		{
			//Speed Up This Thread To High Priority
			System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
			
			//Recieve the inputs (MainApplication.gCapture.bmpCaptured) or Input
			Bitmap InpImage = null;
			
			//- If scaling Desktop take captured bitmap and set scaling and file name to default
			if(Input.GetType().Name.Equals(typeof(MainApplication).Name))
			{
				InpImage = MainApplication.bmpCaptured;
				ReduceToSizeX = def_maxAllowedScrSz4TransmitX;
				ReduceToSizeY = def_maxAllowedScrSz4TransmitY;
				SaveFileName = def_saveFileNameDefault;
			}
			
			//- If scaling another image read image from input and user needs to set scaling and filename
			if(Input.GetType().Name.Equals(typeof(Bitmap).Name))
			{
				InpImage = (Bitmap)Input;
			}
			
			//Reduce Image Using Sutiable Method
			bmpReduced = BringImageToManageableSize(InpImage);
			reducedAt = DateTime.Now;

			//Save cropped region (tile) to disk in C drive		
			if(SaveOutputTilesToDisk)
			{
				//Save this current cropped image to disk
				String fName = (Program.SaveOutputToDiskTEMPFolder + SaveFileName + ".bmp");
				System.IO.File.Delete(fName);
				bmpReduced.Save(fName);
			};
			
			return true;
        }


        /// <summary>
        /// Captured images in (MainApplication.bmpCaptured) larger than (ReduceToSizeX, ReduceToSizeY) are resized to fit into box 
        /// </summary>
        /// <param name="InpImage"></param
        /// <param name = "MaintainAspect" >If true scaling will maintain aspect</ param >
        /// <returns></returns>
        private Bitmap BringImageToManageableSize(Bitmap inBitmap, bool MaintainAspect = false)
		{		
			//: Dont Resize Shorter Images, Within (ReduceToSizeX, ReduceToSizeY) Box
			if((inBitmap.Width <= ReduceToSizeX) && (inBitmap.Height <= ReduceToSizeY)) { goto NoReSizingRequired;}
			
			//: Compute Target Width and Height for Resizing
			double tarWidth = 0;
			double tarHeight = 0;

            if (MaintainAspect)
            {
                //#1) Over Width
                if ((inBitmap.Width > ReduceToSizeX) && (inBitmap.Height <= ReduceToSizeY))
                {
                    tarWidth = ReduceToSizeX;
                    tarHeight = (tarWidth * inBitmap.Height) / inBitmap.Width;
                }
                //#2) Over Height
                if ((inBitmap.Width <= ReduceToSizeX) && (inBitmap.Height > ReduceToSizeY))
                {
                    tarHeight = ReduceToSizeY;
                    tarWidth = (tarHeight * inBitmap.Width) / inBitmap.Height;
                }
            }
			//#3) Over Height And Width
			if((inBitmap.Width > ReduceToSizeX) && (inBitmap.Height > ReduceToSizeY)) 
			{
				tarWidth = ReduceToSizeX;
				tarHeight = ReduceToSizeY;
			}
			
			//Resize The Image Using AForge Routine
			switch(AlgorithmToUse)
			{
				case TypesOfResizing.ResizeBicubic:
						AFFilters.ResizeBicubic fltResizeBicubic = new AFFilters.ResizeBicubic((int)tarWidth, (int)tarHeight);
						return fltResizeBicubic.Apply(inBitmap);
					break;
				case TypesOfResizing.ResizeBilinear:
						AFFilters.ResizeBilinear fltResizeBilinear = new AFFilters.ResizeBilinear((int)tarWidth, (int)tarHeight);
						return fltResizeBilinear.Apply(inBitmap);
					break;
				case TypesOfResizing.ResizeNearestNeighbor:
						AFFilters.ResizeNearestNeighbor fltResizeNearestNeighbor = new AFFilters.ResizeNearestNeighbor((int)tarWidth, (int)tarHeight);
						return fltResizeNearestNeighbor.Apply(inBitmap);
					break;
				default:
						AFFilters.ResizeNearestNeighbor fltDefault = new AFFilters.ResizeNearestNeighbor((int)tarWidth, (int)tarHeight);
						return fltDefault.Apply(inBitmap);
				break;
			}

			
		NoReSizingRequired:
			//Just copy original bmp and return
			return (Bitmap)inBitmap.Clone();
		}
	}
}
