/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 28-Mar-16
 * Time: 5:34 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace ByTeCounter
{
	/// <summary>
	/// StuffGDI: Wrapper for "gdi32.dll" functions for Graphics
	/// (http://www.codeproject.com/Articles/2055/Flicker-Free-Drawing-in-C)
	/// </summary>
	public class StuffGDI
	{
	
		#region Class Functions   
		[DllImport("gdi32.dll", EntryPoint = "CreateDC")]
		public static extern IntPtr CreateDC(IntPtr lpszDriver, string lpszDevice, IntPtr lpszOutput, IntPtr lpInitData);
		
		[DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
		public static extern IntPtr DeleteDC(IntPtr hDc);
		
		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		public static extern IntPtr DeleteObject(IntPtr hDc);
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="hdcDest">A handle to the destination device context.</param>
		/// <param name="xDest">The x-coordinate, in logical units, of the upper-left corner of the destination rectangle.</param>
		/// <param name="yDest">The y-coordinate, in logical units, of the upper-left corner of the destination rectangle.</param>
		/// <param name="wDest">The width, in logical units, of the source and destination rectangles.</param>
		/// <param name="hDest">The height, in logical units, of the source and the destination rectangles.</param>
		/// <param name="hdcSource">A handle to the source device context.</param>
		/// <param name="xSrc">The x-coordinate, in logical units, of the upper-left corner of the source rectangle.</param>
		/// <param name="ySrc">The y-coordinate, in logical units, of the upper-left corner of the source rectangle.</param>
		/// <param name="RasterOp">A raster-operation code. These codes define how the color data for the source rectangle is to be combined with the color data for the destination rectangle to achieve the final color.</param>
		/// <returns></returns>
		[DllImport("gdi32.dll", EntryPoint = "BitBlt")]
		public static extern bool BitBlt(IntPtr hdcDest, 
		                                 int xDest,
		                                 int yDest, 
		                                 int wDest,
		                                 int hDest, 
		                                 IntPtr hdcSource,
		                                 int xSrc, 
		                                 int ySrc, 
		                                 TernaryRasterOperations RasterOp);
		
		/// <summary>
		/// Creates a bitmap compatible with the device that is associated with the specified device context.
		/// </summary>
		/// <param name="hdc"></param>
		/// <param name="nWidth"></param>
		/// <param name="nHeight"></param>
		/// <returns></returns>
		[DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
		public static extern IntPtr CreateCompatibleBitmap
		                            (IntPtr hdc, int nWidth, int nHeight);
		
		/// <summary>
		/// The CreateCompatibleDC function creates a memory device context (DC) compatible with the specified device
		/// </summary>
		/// <param name="hdc"></param>
		/// <returns></returns>
		[DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC")]
		public static extern IntPtr CreateCompatibleDC(IntPtr hdc);
		
		/// <summary>
		/// Selects an object into the specified device context (DC). The new object replaces the previous object of the same type.
		/// (hdc=bmp)
		/// </summary>
		/// <param name="hdc"></param>
		/// <param name="bmp"></param>
		/// <returns></returns>
		[DllImport("gdi32.dll", EntryPoint = "SelectObject")]
		public static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
		#endregion       
	}
}
