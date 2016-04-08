/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 29-Mar-16
 * Time: 6:41 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using ImageProcessor.Processors;
using AFFilters = AForge.Imaging.Filters;
using System.Windows.Forms;

namespace UltraFAST
{
	
	/// <summary>
	/// StuffImaging: Image processing functions
	/// </summary>
	public class StuffImaging
	{
		
		public static byte[] ImageToByteArray(Image img)
		{
		    ImageConverter converter = new ImageConverter();
		    return (byte[])converter.ConvertTo(img, typeof(byte[]));
		}
		
		public static byte[] ImageToByteArray(Bitmap bmp)
		{
		    ImageConverter converter = new ImageConverter();
		    return (byte[])converter.ConvertTo(bmp, typeof(byte[]));
		}
		
		/// <summary>
		/// Retrurns true if image is negative stride
		/// </summary>
		/// <param name="srcBitmap"></param>
		/// <returns></returns>
		public unsafe static bool IsNegativeStrideImage(Bitmap srcBitmap)
		{
			//Result of Stride
			bool Result = false;
			
			//Lock source image for faster processing (get its bitmapdata pointer)
			var srcImgBitmapData = srcBitmap.LockBits(new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height),
				                                      ImageLockMode.ReadOnly,
				                                      srcBitmap.PixelFormat);
			
			//Read stride of the image into Result
			if(srcImgBitmapData.Stride < 0) { Result = true; } else { Result=false; }
				
			//Unlock source and destination images (Job Done)
			srcBitmap.UnlockBits(srcImgBitmapData);
			
			//Retrun calculated stride of image
			return Result;
		}
		
		/// <summary>
		/// Fast comparision of two bitmaps (rgb or gray, 1bpp to Nbpp)
		/// BASED: http://www.dotnetexamples.com/2012/07/fast-bitmap-comparison-c.html
		/// BASED: http://stackoverflow.com/questions/2031217/what-is-the-fastest-way-i-can-compare-two-equal-size-bitmaps-to-determine-whethe
		/// 
		/// !!! If two bitmaps are changed then return true else false.
		/// </summary>
		/// <param name="firstBmp">Bitmap-A</param>
		/// <param name="secondBmp">Bitmap-B</param>
		/// <param name="UseInternalAlternateLogic">Use Internal Filters</param>
		/// <param name="percentLinesChanged">If [0.0-1.0] line compare else block compare</param>
		/// <returns></returns>
		public unsafe static bool IsBitmapsDiffer(Bitmap firstBmp, 
		                                          Bitmap secondBmp,
		                                          bool UseInternalAlternateLogic = false,
		                                          double perLinesChanged = 0.0,
		                                          int execMethod = 1)
		{
				
			//If matching objects bitmap not changed
			if(Object.Equals(firstBmp, secondBmp)) 
			{ 
				return false; 
			}
			
			//If any image is null bitmap is changed
			if(firstBmp==null || secondBmp==null) 
			{ 
				return true; 
			}
			
			//If size or pixelformat doesnot match bitmap is changed
			if(!firstBmp.Size.Equals(secondBmp.Size) || !firstBmp.PixelFormat.Equals(secondBmp.PixelFormat))
			{
				return true;
			}
			
			//Compute size of images
			int inImgWidth = firstBmp.Width;
			int inImgHeight = firstBmp.Height;
			
			//If UseFastLogic enforce one shot comparision
			if(UseInternalAlternateLogic) { perLinesChanged = -1.0; }
			
			//If not use fast logic take original images
			Bitmap lftBmp = null;
			Bitmap rgtBmp = null;
			if(!UseInternalAlternateLogic) 
			{ 
				lftBmp=firstBmp; 
				rgtBmp=secondBmp; 
				goto SkipAlternateLogic;
			}
			
			//If fast logic use reduced images for detection of speed
			if(UseInternalAlternateLogic)
			{
				//Cyclic Bitmap
				Bitmap leftCycl=firstBmp;
				Bitmap rightCycl=secondBmp;		
				
				//Grayscale filter (to 8bit conversion)
				if(execMethod != 1) { goto Skip1; }
				AFFilters.Grayscale filt8Bit = new AFFilters.Grayscale(0.2125, 0.7154, 0.0721);
				leftCycl = filt8Bit.Apply(leftCycl);
				rightCycl = filt8Bit.Apply(rightCycl);
			Skip1:
				//Reduce size by 50% (pixel reduction)
				if(execMethod != 2) { goto Skip2; }
				double RSZfactor = 0.5;
				int imgWidthNew = (int)(RSZfactor*inImgWidth);
				int imgHeightNew = (int)(RSZfactor*inImgHeight);
				AFFilters.ResizeNearestNeighbor filtReSizePercent = new AFFilters.ResizeNearestNeighbor(imgWidthNew, imgHeightNew);
				leftCycl = filtReSizePercent.Apply(leftCycl);
				rightCycl = filtReSizePercent.Apply(rightCycl);
			Skip2:
				//Reduce to min rectangle
				if(execMethod != 3) { goto Skip3; }
				int imgWidthNew1 = Math.Min(inImgWidth, 256);
				int imgHeightNew1 = Math.Min(inImgHeight, 256);
				AFFilters.ResizeNearestNeighbor filtReSizeSize = new AFFilters.ResizeNearestNeighbor(imgWidthNew1, imgHeightNew1);
				leftCycl = filtReSizeSize.Apply(leftCycl);
				rightCycl = filtReSizeSize.Apply(rightCycl);
			Skip3:
				//Take results ahead
				lftBmp = leftCycl;
				rgtBmp = rightCycl;
			}
//			
//	Bitmap bmpGrayedTiles = filtGrayScale.Apply(roiArea.CroppedBitmap);
			
		SkipAlternateLogic:
			
			//Lock both bitmaps for faster processing
			var leftBitmapData = lftBmp.LockBits(new Rectangle(0, 0, lftBmp.Width, lftBmp.Height), ImageLockMode.ReadOnly, lftBmp.PixelFormat);
			var rightBitmapData = rgtBmp.LockBits(new Rectangle(0, 0, rgtBmp.Width, rgtBmp.Height), ImageLockMode.ReadOnly, rgtBmp.PixelFormat);
			
			//Fetch first byte address in image
			IntPtr leftStartIntPtr = leftBitmapData.Scan0;
			var leftStartPtr = (byte *)leftStartIntPtr.ToPointer();
			IntPtr rightStartIntPtr = rightBitmapData.Scan0;
			var rightStartPtr = (byte *)rightStartIntPtr.ToPointer();
			
			//Get Stride : one row of data in memory [RGBA, or G, or RGB, ...] wrt one line
			//Get other values
			int imgLineWidthPx = leftBitmapData.Width;
			int imgLineStrideSz = Math.Abs(leftBitmapData.Stride); //Negative to positive stride
			int imgTotalLines = leftBitmapData.Height;
			
			//Compute total lines that should be changed
			int cntQlfyLines = (int)(perLinesChanged * imgTotalLines);
			
			//Our Result (Default = true, yes images are changed, send them)
			bool Result = true;
			
			//If one shot comparision is required, compre entire bitmap array using msvcrt.dll.
			//Otherwise compare linewise and if changed lines > desired %age say differ
			if((perLinesChanged <= 0.0) || (perLinesChanged > 1.0))
			{
				//Compare whole image (entire array)
				int intResult = StuffMem.MemCmp(leftStartIntPtr, rightStartIntPtr, (imgLineStrideSz * imgTotalLines));
				//intResult=Zero --> Images Are Same (Differ = False)
				if(intResult == 0) { Result=false; } else { Result=true; }
				//Return the result
				goto AlgoEnd;
			}
			else
			{			
				//Compare and find changed lines in image (if > cntQlfyLines say differ)
				//Base Value (Images Are Same)
				Result=false;
				//Iterate through each line
				for(int y = 0; y < imgTotalLines; y++)
				{
					//Compare whole image (entire array)
					int intResult = StuffMem.MemCmp(leftStartIntPtr + y * leftBitmapData.Stride, 
					                                rightStartIntPtr + y *leftBitmapData.Stride, 
					                                imgLineStrideSz);
					//intResult!=Zero --> Images Differ (cntQlfyLines decrease)
					if(intResult != 0) 
					{ 
						cntQlfyLines--; 
					}
					else
					{
						int dummy = 0;
						dummy++;
					}
					//If cntQlfyLines <= 0 images differ by threshold
					if(cntQlfyLines <= 0) 
					{ 
						Result = true; 
						break; 
					}
				}
			}
	
		AlgoEnd:
			
			//Unlock both bitmaps
			lftBmp.UnlockBits(leftBitmapData);
			rgtBmp.UnlockBits(rightBitmapData);
			
			//Return the Result
			return Result;
		}
	
		/// <summary>
		/// https://msdn.microsoft.com/en-in/library/system.drawing.imaging.encoderparameter%28v=vs.110%29.aspx
		/// </summary>
		/// <param name="format"></param>
		/// <returns></returns>
		public static ImageCodecInfo GetEncoderForImageFormat(ImageFormat format)
		{
		    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
		    return codecs.Single(i => (i.FormatID == format.Guid));
		}
		
//		JPEG = 1,
//		PNG = 2,
//		GIF = 3,
//		TIFF = 4,
//		WMF = 5,
//		EXIF = 6
		/// <summary>
		/// Convert image to desired type
		/// </summary>
		/// <param name="inpBitmap">Bitmap</param>
		/// <param name="ImageType">JPEG, ....</param>
		/// <param name="imgQuality">Percentage, 0.0 to 1.0 (100%)</param>
		/// <param name="AlgoFamily">NETLanguage Or ImageProcessor</param>
		/// <param name="saveFileName">Name Of File To SaveTo</param>
		/// <returns></returns>
		public static Bitmap BitmapConvert(Bitmap inpBitmap, 
		                                   TypesOfImageConversion ImageType = TypesOfImageConversion.JPEG, 
		                                   double imgQuality = 1.0,
		                                   TypeOfIMGAlgos AlgoFamily = TypeOfIMGAlgos.NETLanguage,
		                                   String saveFileName = "")
		{
			//Resultant Bitmap
			Bitmap outBitmap = null;
			
			//BASED: https://msdn.microsoft.com/en-in/library/system.drawing.imaging.encoderparameter%28v=vs.110%29.aspx
			//NET Native Methods (AlgoFamily==1)
			if(AlgoFamily == TypeOfIMGAlgos.NETLanguage)
			{
				//Endoer Object And Parameter Of Quality InSide All Parameters Object
				ImageCodecInfo lclEncoder = null;
				EncoderParameters objAllParams = new EncoderParameters(1); //Total One Parameter in List
				EncoderParameter objQualityParam = new EncoderParameter(Encoder.Quality, (long)(imgQuality * 100L));
				objAllParams.Param[0] = objQualityParam;

				//Encode input image using encoder and its quality parameter 
				//to memory stream (tmpStream)
				using(MemoryStream outStream = new MemoryStream())
				{					
					//Encode input image using encoder and its quality parameter 
					//to memory stream (tmpStream)
			    	switch(ImageType)
	    			{
			    			case TypesOfImageConversion.BMP:
			    				//Encode input image using encoder and its parameter to memory stream
				    			lclEncoder = GetEncoderForImageFormat(ImageFormat.Bmp);
				    			inpBitmap.Save(outStream, lclEncoder, objAllParams);			    			break;
		    				case TypesOfImageConversion.JPEG:
			    				//Encode input image using encoder and its parameter to memory stream
				    			lclEncoder = GetEncoderForImageFormat(ImageFormat.Jpeg);
				    			inpBitmap.Save(outStream, lclEncoder, objAllParams);
		    					break;
		    				case TypesOfImageConversion.PNG:
		    					//Encode input image using encoder and its parameter to memory stream
		    					lclEncoder = GetEncoderForImageFormat(ImageFormat.Png);
				    			inpBitmap.Save(outStream, lclEncoder, objAllParams);
		    					break;
		    				case TypesOfImageConversion.GIF:
		    					//Encode input image using encoder and its parameter to memory stream
		    					lclEncoder = GetEncoderForImageFormat(ImageFormat.Gif);
				    			inpBitmap.Save(outStream, lclEncoder, objAllParams);
		    					break;
		    				case TypesOfImageConversion.TIFF:
		    					//Encode input image using encoder and its parameter to memory stream
		    					lclEncoder = GetEncoderForImageFormat(ImageFormat.Tiff);
				    			inpBitmap.Save(outStream, lclEncoder, objAllParams);
		    					break;
		    				case TypesOfImageConversion.WMF:
		    					//Encode input image using encoder and its parameter to memory stream
		    					lclEncoder = GetEncoderForImageFormat(ImageFormat.Wmf);
				    			inpBitmap.Save(outStream, lclEncoder, objAllParams);
		    					break;
		    				case TypesOfImageConversion.EXIF:
		    					//Encode input image using encoder and its parameter to memory stream
				    			inpBitmap.Save(outStream, lclEncoder, objAllParams);
		    					lclEncoder = GetEncoderForImageFormat(ImageFormat.Exif);
		    					break;
		    				default:
			    				//Encode input image using encoder and its parameter to memory stream
				    			lclEncoder = GetEncoderForImageFormat(ImageFormat.Jpeg);
				    			inpBitmap.Save(outStream, lclEncoder, objAllParams);
				    			break;
		    		}
			    	
			    	//Output Image To Be Sent
					outBitmap = new Bitmap(outStream);

					//Save the Bitmap
					if(!String.IsNullOrEmpty(saveFileName) && Program.SaveAllOutputsToDiskInTEMPFolder)
					{
						String fExtension = ".jpg";
						
						if(ImageType == TypesOfImageConversion.BMP) {fExtension = ".bmp"; } else
							if(ImageType == TypesOfImageConversion.EXIF) {fExtension = ".exif"; } else
								if(ImageType == TypesOfImageConversion.GIF) {fExtension = ".gif"; } else
									if(ImageType == TypesOfImageConversion.JPEG) {fExtension = ".jpg"; } else
										if(ImageType == TypesOfImageConversion.PNG) {fExtension = ".png"; } else
											if(ImageType == TypesOfImageConversion.TIFF) {fExtension = ".tiff"; } else
												if(ImageType == TypesOfImageConversion.WMF) {fExtension = ".wmf"; }
						

						outBitmap.Save(Program.SaveOutputToDiskTEMPFolder + saveFileName  + fExtension);
					}
			
					//OR Slower:
			    	//outBitmap = (Bitmap)Bitmap.FromStream(outStream);
			    	goto AlgoEND;
				}
			}
			else if(AlgoFamily != TypeOfIMGAlgos.NETLanguage)
			{
				//ImageProcessor Methods (AlgoFamily > 0)	
				
				//1. Input Bitmap As MemoryStream
				using(MemoryStream inStream = new MemoryStream())
				{
					//Save input bitmap to memory stream
					if (ImageFormat.Jpeg.Equals(inpBitmap.RawFormat))
					{
	    				// JPEG
	    				inpBitmap.Save(inStream, ImageFormat.Jpeg);
					}
					else if (ImageFormat.Bmp.Equals(inpBitmap.RawFormat))
					{
					    // Bmp
	    				inpBitmap.Save(inStream, ImageFormat.Bmp);
					}
					else if (ImageFormat.Png.Equals(inpBitmap.RawFormat))
					{
					    // Png
	    				inpBitmap.Save(inStream, ImageFormat.Png);
					}
					else if (ImageFormat.Gif.Equals(inpBitmap.RawFormat))
					{
					    // Gif
	    				inpBitmap.Save(inStream, ImageFormat.Gif);
					}
					else if (ImageFormat.Tiff.Equals(inpBitmap.RawFormat))
					{
					    // Tiff
	    				inpBitmap.Save(inStream, ImageFormat.Tiff);
					}
					else if (ImageFormat.Emf.Equals(inpBitmap.RawFormat))
					{
					    // Emf
	    				inpBitmap.Save(inStream, ImageFormat.Emf);
					}
					else if (ImageFormat.Exif.Equals(inpBitmap.RawFormat))
					{
					    // Exif
	    				inpBitmap.Save(inStream, ImageFormat.Exif);
					}
					else if (ImageFormat.Wmf.Equals(inpBitmap.RawFormat))
					{
					    // Wmf
	    				inpBitmap.Save(inStream, ImageFormat.Wmf);
					}
					else if (ImageFormat.MemoryBmp.Equals(inpBitmap.RawFormat))
					{
					    // MemoryBmp
	    				inpBitmap.Save(inStream, ImageFormat.Bmp);
					}

					//2. Encode input image using imageProcessor into outStream
					using(MemoryStream outStream = new MemoryStream())
					{
						//3. Encoding factory which will write to stream
						using(ImageFactory imgProcFactory = new ImageFactory(true))
						{						
							//ImageProcessor.Imaging.Formats
							//: BitmapFormat, JpegFormat, GifFormat, PngFormat, TiffFormat, 
		    				ISupportedImageFormat format = null;
		    				
		    				//Set quality of image
		    				imgProcFactory.Quality((int)(imgQuality * 100.0));
							
							//Encode input image using encoder and its quality parameter 
							//to memory stream (tmpStream)
					    	switch(ImageType)
			    			{
					    			case TypesOfImageConversion.BMP:
					    				format = new BitmapFormat();
					    			break;
				    				case TypesOfImageConversion.JPEG:
					    				format = new JpegFormat();
				    					break;
				    				case TypesOfImageConversion.PNG:
				    					format = new PngFormat();
				    					break;
				    				case TypesOfImageConversion.GIF:
				    					format = new GifFormat();
				    					break;
				    				case TypesOfImageConversion.TIFF:
				    					format = new TiffFormat();
				    					break;
				    				case TypesOfImageConversion.WMF:
				    					throw new Exception("WMF Conversion Not Supported");
				    					break;
				    				case TypesOfImageConversion.EXIF:
				    					throw new Exception("EXIF Conversion Not Supported");
				    					break;
				    				default:
				    					format = new JpegFormat();
						    			break;
				    		}
					    	//Lod InStream, Format, Save to outStream
					    	imgProcFactory.Load(inStream).Format(format).Save(outStream);
					    	
					    	//Output Image To Be Sent
							outBitmap = new Bitmap(outStream);
							//OR Slower:
					    	//outBitmap = (Bitmap)Bitmap.FromStream(outStream);

			
							//Save the Bitmap
							if(!String.IsNullOrEmpty(saveFileName) && Program.SaveAllOutputsToDiskInTEMPFolder)
							{
								String fExtension = ".jpg";
								
								if(ImageType == TypesOfImageConversion.BMP) {fExtension = ".bmp"; } else
									if(ImageType == TypesOfImageConversion.EXIF) {fExtension = ".exif"; } else
										if(ImageType == TypesOfImageConversion.GIF) {fExtension = ".gif"; } else
											if(ImageType == TypesOfImageConversion.JPEG) {fExtension = ".jpg"; } else
												if(ImageType == TypesOfImageConversion.PNG) {fExtension = ".png"; } else
													if(ImageType == TypesOfImageConversion.TIFF) {fExtension = ".tiff"; } else
														if(ImageType == TypesOfImageConversion.WMF) {fExtension = ".wmf"; }
								
								outBitmap.Save(Program.SaveOutputToDiskTEMPFolder + saveFileName  + fExtension);
							}					    	
							
							
					    	goto AlgoEND;
						}
					}
				}
			}
			
		AlgoEND:
			if(outBitmap == null) 
			{
				//Nothing computed, sent original
				outBitmap = (Bitmap)inpBitmap.Clone();
			}


            //Dispose outBitmap
            Bitmap bmpTransmit = new Bitmap(outBitmap);
            outBitmap.Dispose();
            outBitmap = null;

            //Return the computed data
            return bmpTransmit;
		}

        /// <summary>
        /// Configure graphics for faster graphics painting along with form
        /// </summary>
        /// <param name="frm"></param>
        /// <param name="mode"></param>
        public static void SetupFastGraphics(Form frm,
                                             ScrPaintingMode mode = ScrPaintingMode.DefaultMode)
        {
            //Get and setup the graphics object
            using (Graphics grph = frm.CreateGraphics())
            {
                StuffImaging.SetupFastGraphics(grph, mode);
            }
        }

        /// <summary>
        /// Configure graphics  for faster graphics painting
        /// (http://www.codeproject.com/Tips/66909/Rendering-fast-with-GDI-What-to-do-and-what-not-to)
        /// 
        /// Don't mix bitmap formats
        /// Don't even think about threads
        /// 
        /// ALSO: (http://stackoverflow.com/questions/3841905/fastest-way-to-draw-a-series-of-bitmaps-with-c-sharp)
        /// </summary>
        /// <param name="grph"></param>
        public static void SetupFastGraphics(Graphics grph, 
		                                     ScrPaintingMode mode = ScrPaintingMode.DefaultMode)
		{
            if (mode == ScrPaintingMode.DefaultMode)
            {
                //Fastest settings for simply copying the bitmap to the output window.
                grph.CompositingMode = CompositingMode.SourceOver;
                grph.CompositingQuality = CompositingQuality.HighSpeed;
                grph.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                grph.SmoothingMode = SmoothingMode.None;
                grph.InterpolationMode = InterpolationMode.NearestNeighbor;
            }
            else if (mode == ScrPaintingMode.OneToOnePainting)
			{
				//Fastest settings for simply copying the bitmap to the output window.
			    grph.CompositingMode = CompositingMode.SourceCopy;
			    grph.CompositingQuality = CompositingQuality.HighSpeed;
			    grph.PixelOffsetMode = PixelOffsetMode.None;
			    grph.SmoothingMode = SmoothingMode.None;
			    grph.InterpolationMode = InterpolationMode.Default;			
			}else if (mode == ScrPaintingMode.ScalingSpritesText)
			{
				//Fastest settings for if you are doing any scaling, rendering of sprites or text.
			    grph.CompositingMode = CompositingMode.SourceOver;
			    grph.CompositingQuality = CompositingQuality.HighSpeed;
			    grph.PixelOffsetMode = PixelOffsetMode.HighSpeed;
			    grph.SmoothingMode = SmoothingMode.HighSpeed;
			    grph.InterpolationMode = InterpolationMode.HighQualityBilinear;					
			}else if (mode == ScrPaintingMode.SpritesWithTransparency)
			{
				//Fastest settings for if you are doing any scaling, rendering of sprites or text.
			    grph.CompositingMode = CompositingMode.SourceOver;
			    grph.CompositingQuality = CompositingQuality.HighSpeed;
			    grph.PixelOffsetMode = PixelOffsetMode.Half;
			    grph.SmoothingMode = SmoothingMode.HighSpeed;
			    grph.InterpolationMode = InterpolationMode.NearestNeighbor;					
			}else if (mode == ScrPaintingMode.SpritesWithTransparency)
			{
				//Fastest settings for if you are doing any scaling, rendering of sprites or text.
			    grph.CompositingMode = CompositingMode.SourceCopy;
			    grph.CompositingQuality = CompositingQuality.HighSpeed;
			    grph.PixelOffsetMode = PixelOffsetMode.HighSpeed;
			    grph.SmoothingMode = SmoothingMode.HighSpeed;
			    grph.InterpolationMode = InterpolationMode.HighQualityBicubic;					
			}else if (mode == ScrPaintingMode.TextWithDrawString)
			{
				//Fastest settings for if you are doing any scaling, rendering of sprites or text.
			    grph.CompositingMode = CompositingMode.SourceOver;
			    grph.CompositingQuality = CompositingQuality.HighSpeed;
			    grph.PixelOffsetMode = PixelOffsetMode.HighSpeed;
			    grph.SmoothingMode = SmoothingMode.None;
			    grph.InterpolationMode = InterpolationMode.HighQualityBicubic;					
			}
		}
	}	
}

