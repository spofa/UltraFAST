/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 29-Mar-16
 * Time: 12:01 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;

namespace UltraFAST
{
	/// <summary>
	/// Description of Region.
	/// </summary>
	public class ROIArea
	{
		/// <summary>
		/// Index Of Region (0 .. nRows x nCols)
		/// </summary>
		public int RgnIndex {get; set;}
		
		/// <summary>
		/// Region box in pixels on desktop (Left, Top) to (Width, Height)
		/// </summary>
		public int Left {get; set;}

		/// <summary>
		/// Region box in pixels on desktop (Left, Top) to (Width, Height)
		/// </summary>
		public int Top {get; set;}

		/// <summary>
		/// Region box in pixels on desktop (Left, Top) to (Width, Height)
		/// </summary>
		public int Width  {get; set;}

		/// <summary>
		/// Region box in pixels on desktop (Left, Top) to (Width, Height)
		/// </summary>
		public int Height {get; set;}

		/// <summary>
		/// Size of desktop (captured image)
		/// </summary>
		public int DskWidth {get; set;}
		
		/// <summary>
		/// Size of desktop (captured image)
		/// </summary>
		public int DskHeight {get; set;}
		
		/// <summary>
		/// ParentBitmap [Copy or Pointer] -> CroppedTileBitmap
		/// </summary>
		public Bitmap ParentBitmap { get; set; }
		
		/// <summary>
		/// ParentBitmap -> CroppedTileBitmap
		/// </summary>
		public Bitmap CroppedTileBitmap { get; set; }

		/// <summary>
		/// CroppedTileBitmap -> prevCroppedTileBitmap 
		/// </summary>
		public Bitmap prevCroppedTileBitmap {get; set;}

        /// <summary>
        /// Final result to be transmitted [[If compression is on transmit byteArrayToTransmit rather than bmpToTransmit]]
        /// </summary>
        public Bitmap bmpToTransmit { get; set; }

        /// <summary>
        /// Final result to be transmitted [[If compression is on transmit byteArrayToTransmit rather than bmpToTransmit]]
        /// </summary>
        public byte[] byteArrayToTransmit { get; set; }

		/// <summary>
		/// If (CroppedTileBitmap != prevCroppedTileBitmap), we make this value true. 
		/// And this signifies transmission of tile
		/// </summary>
		public bool IsROITileChanged {get; set;}
		
		/// <summary>
		/// Default region is whole desktop
		/// </summary>
		public ROIArea()
		{
			this.RgnIndex = 0;
			
			this.Left = 0;
			this.Top = 0;
			this.Width = StuffWin32.GetSystemMetrics(StuffWin32.SM_CXSCREEN);
			this.Height = StuffWin32.GetSystemMetrics(StuffWin32.SM_CYSCREEN);
			
			this.DskWidth = StuffWin32.GetSystemMetrics(StuffWin32.SM_CXSCREEN);
			this.DskHeight = StuffWin32.GetSystemMetrics(StuffWin32.SM_CYSCREEN);
		}
		
		/// <summary>
		/// Custom region left, top, width, height (Use -1 for previous value)
		/// </summary>
		/// <param name="_RgnIndex">Index of Region</param>
		/// <param name="_Left"></param>
		/// <param name="_Top"></param>
		/// <param name="_Width"></param>
		/// <param name="_Height"></param>
		public ROIArea(int _RgnIndex, int _Left, int _Top, int _Width, int _Height)
		{
			if(_RgnIndex!=-1) this.RgnIndex = _RgnIndex;
			
			if(_Left!=-1) this.Left = _Left;
			if(_Top!=-1) this.Top = _Top;
			if(_Width!=-1) this.Width = _Width;
			if(_Height!=-1) this.Height = _Height;			
			
			this.DskWidth = StuffWin32.GetSystemMetrics(StuffWin32.SM_CXSCREEN);
			this.DskHeight = StuffWin32.GetSystemMetrics(StuffWin32.SM_CYSCREEN);
		}
		
		public override string ToString()
		{
			return String.Format("RGN# {0} [({1},{2})-({5},{6})], w{3}, h{4}", this.RgnIndex, 
			                     this.Left, 
			                     this.Top, 
			                     this.Width, 
			                     this.Height,
			                     this.Left+this.Width,
			                     this.Top+this.Height);
			//return String.Format("(l{0},t{1}) - (w{2}, h{3})", this.Left, this.Top, this.Width, this.Height);
		}
		
		/// <summary>
		/// Get rectangle of ROI object
		/// </summary>
		/// <returns></returns>
		public Rectangle ConvertToRectangle()
		{
			return new Rectangle(this.Left, this.Top, this.Width, this.Height);
		}
	}
}
