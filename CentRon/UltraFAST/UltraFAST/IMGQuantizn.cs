/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 28-Mar-16
 * Time: 8:06 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Drawing.Imaging;
using ImageProcessor.Imaging.Quantizers;
using ImageProcessor.Imaging.Quantizers.WuQuantizer;

using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Imaging.ColorReduction;

namespace UltraFAST
{
	/// <summary>
	/// IMGQuantizn: Quantization is reducting image bpp  
	/// 
	/// 
	/// 
	/// BASED ON: https://msdn.microsoft.com/en-us/library/Aa479306.aspx
	/// </summary>
	public class IMGQuantizn : AHandler 
	{
		public static int def_maxColorsPellet = 255;
		public static int def_maxColorBits = 8;
		public static TypeOfQuantizers def_QuantizerToUse = TypeOfQuantizers.AForgeNET_ColorImage;
		public string def_saveFileNameDefault = "IMGQuantizn";
		
		/// <summary>
		/// The maximum number of colors to return (&le; 255)
		/// </summary>
		public int maxColorsPellet = def_maxColorsPellet;
		
		/// <summary>
		/// The number of significant bits (&le; 8)
		/// </summary>
		public int maxColorBits = def_maxColorBits;
		
		/// <summary>
		/// Default Quantizer (WuTransform Is Fastest)
		/// </summary>
		public TypeOfQuantizers QuantizerToUse = def_QuantizerToUse;
			
		/// <summary>
		/// If true saved cropped regions to C:\TEMP\*.*
		/// </summary>
		public bool SaveOutputTilesToDisk = Program.SaveAllOutputsToDiskInTEMPFolder;
		
		/// <summary>
		/// Output quantized bitmap result
		/// </summary>
		public Bitmap bmpQuantized {get; set;}
		
		// Time at which quantization done
		public DateTime quantizedAt {get; set;}
		
		/// <summary>
		/// Default quantizer
		/// </summary>
		/// <param name="Input"></param>
		/// <returns></returns>
		
		/// <summary>
		/// Performs Octree-based Quantization. For Image Processing Uses http://imageprocessor.org/
		/// [[Fast NET Library For On-the-fly Processing of images]]
		/// 
		/// SITE: http://imageprocessor.org/
		/// NUGET: https://www.nuget.org/packages/ImageProcessor/
		/// GITHUB: https://github.com/JimBobSquarePants/ImageProcessor/
		/// SRC CODE: http://sourcebrowser.io/Browse/JimBobSquarePants/ImageProcessor/src/ImageProcessor/Imaging/Quantizers/OctreeQuantizer.cs
		/// 
		/// This library can
		/// : Encode/Decode: JPEG, BMP, PNG, GIF
		/// : Quantize Images: Octree, Wu, Palette
		/// : Crop Image: Rectangle, Elliptical, Entropy 
		/// etc.
		/// </summary>
		/// <param name="Input"></param>
		/// <returns></returns>
		public override bool Execute(ref object Input)
		{
			//Speed Up This Thread To High Priority
			System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
		
			//Get the input bitmap
			Bitmap InpBitmap = (Bitmap)Input;
			
			//Pointer to hold output bitmap
			Bitmap bmpConverted = null;
			
			//Variables For AForge.NET
			ColorImageQuantizer ciq = null;
			Color[] colorTable = null;
			Bitmap outBitmap = null;
			
//16 bit per pixel in NET

			switch(QuantizerToUse)
			{
				case TypeOfQuantizers.NETFormat16bppRgb555:
						outBitmap = new Bitmap(InpBitmap.Width, InpBitmap.Height, PixelFormat.Format16bppRgb555);
						using (Graphics grInpBitmap = Graphics.FromImage(outBitmap)) 
						{
						    grInpBitmap.DrawImage(InpBitmap, new Rectangle(0, 0, outBitmap.Width, outBitmap.Height));
						}
						bmpConverted = outBitmap;
					break;
					
				case TypeOfQuantizers.NETFormat16bppRgb565:
						outBitmap = new Bitmap(InpBitmap.Width, InpBitmap.Height, PixelFormat.Format16bppRgb565);
						using (Graphics grInpBitmap = Graphics.FromImage(outBitmap)) 
						{
						    grInpBitmap.DrawImage(InpBitmap, new Rectangle(0, 0, outBitmap.Width, outBitmap.Height));
						}
						bmpConverted = outBitmap;
					break;

                case TypeOfQuantizers.NETFormat8bppIndexed:
                    Format8bppIndexConverter2 f = new Format8bppIndexConverter2();

                    outBitmap = f.ConvertTo8bppFormat(InpBitmap);
                    
                    bmpConverted = outBitmap;
                    break;

                case TypeOfQuantizers.ImageProcessor_OctTree:
						OctreeQuantizer OctTreeQuantizer = new OctreeQuantizer(maxColorsPellet, maxColorBits);
						bmpConverted = OctTreeQuantizer.Quantize(InpBitmap);
					break;
				case TypeOfQuantizers.ImageProcessor_WuTransform:
						WuQuantizer WuTransQuantizer = new WuQuantizer();
						bmpConverted = WuTransQuantizer.Quantize(InpBitmap);
					break;
					
				case TypeOfQuantizers.AForgeNET_Burkes:
						// create color image quantization routine
						ciq = new ColorImageQuantizer(new MedianCutQuantizer());
						// create 8 colors table
						colorTable = ciq.CalculatePalette(InpBitmap, maxColorsPellet);
						// create dithering routine
						BurkesColorDithering dithering1 = new BurkesColorDithering();
						dithering1.ColorTable = colorTable;
						// apply the dithering routine
						bmpConverted = dithering1.Apply( InpBitmap );
					break;
					
				case TypeOfQuantizers.AForgeNET_ColorImage:
						// instantiate the images' color quantization class
						ciq = new ColorImageQuantizer( new MedianCutQuantizer( ) );
						// get 16 color palette for a given image
						colorTable = ciq.CalculatePalette( InpBitmap, maxColorsPellet );
						// ... or just reduce colors in the specified image
						bmpConverted = ciq.ReduceColors( InpBitmap, maxColorsPellet );					
					break;
					
				case TypeOfQuantizers.AForgeNET_FloydSteinberg:
						// create color image quantization routine
						ciq = new ColorImageQuantizer(new MedianCutQuantizer());
						// create 8 colors table
						colorTable = ciq.CalculatePalette(InpBitmap, maxColorsPellet);
						// create dithering routine
						FloydSteinbergColorDithering dithering2 = new FloydSteinbergColorDithering();
						dithering2.ColorTable = colorTable;
						// apply the dithering routine
						bmpConverted = dithering2.Apply( InpBitmap );
					break;
					
				case TypeOfQuantizers.AForgeNET_JarvisJudiceNinke:
						// create color image quantization routine
						ciq = new ColorImageQuantizer(new MedianCutQuantizer());
						// create 8 colors table
						colorTable = ciq.CalculatePalette(InpBitmap, maxColorsPellet);
						// create dithering routine
						JarvisJudiceNinkeColorDithering dithering3 = new JarvisJudiceNinkeColorDithering ();
						dithering3.ColorTable = colorTable;
						// apply the dithering routine
						bmpConverted = dithering3.Apply( InpBitmap );
					break;
					
				case TypeOfQuantizers.AForgeNET_OrderedColor:
						// create color image quantization routine
						ciq = new ColorImageQuantizer(new MedianCutQuantizer());
						// create 8 colors table
						colorTable = ciq.CalculatePalette(InpBitmap, maxColorsPellet);
						// create dithering routine
						OrderedColorDithering dithering4 = new OrderedColorDithering();
						dithering4.ColorTable = colorTable;
						// apply the dithering routine
						bmpConverted = dithering4.Apply( InpBitmap );
					break;
					
				case TypeOfQuantizers.AForgeNET_SierraColor:
						// create color image quantization routine
						ciq = new ColorImageQuantizer(new MedianCutQuantizer());
						// create 8 colors table
						colorTable = ciq.CalculatePalette(InpBitmap, maxColorsPellet);
						// create dithering routine
						SierraColorDithering  dithering5 = new SierraColorDithering ();
						dithering5.ColorTable = colorTable;
						// apply the dithering routine
						bmpConverted = dithering5.Apply( InpBitmap );
					break;
					
				case TypeOfQuantizers.AForgeNET_StuckiColor:
						// create color image quantization routine
						ciq = new ColorImageQuantizer(new MedianCutQuantizer());
						// create 8 colors table
						colorTable = ciq.CalculatePalette(InpBitmap, maxColorsPellet);
						// create dithering routine
						StuckiColorDithering   dithering6 = new StuckiColorDithering  ();
						dithering6.ColorTable = colorTable;
						// apply the dithering routine
						bmpConverted = dithering6.Apply( InpBitmap );
					break;
					
				default:
						OctreeQuantizer DefQuantizer = new OctreeQuantizer(maxColorsPellet, maxColorBits);
						bmpConverted = DefQuantizer.Quantize(InpBitmap);
					break;
			}

			//Read the result
			quantizedAt = DateTime.Now;
			bmpQuantized = bmpConverted;
			
			//Save cropped region (tile) to disk in C drive		
			if(SaveOutputTilesToDisk)
			{
				//Save this current cropped image to disk
				String fName = (Program.SaveOutputToDiskTEMPFolder + def_saveFileNameDefault + ".bmp");
				System.IO.File.Delete(fName);
				bmpQuantized.Save(fName);
			};
			
			return true;
		}
	}
}
