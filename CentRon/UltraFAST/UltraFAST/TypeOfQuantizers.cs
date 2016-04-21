/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 29-Mar-16
 * Time: 11:51 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace UltraFAST
{
	/// <summary>
	/// Types of Quantizers
	/// </summary>
	public enum TypeOfQuantizers
	{
		NONE = -1,
		
		ImageProcessor_OctTree = 1,
		
		/// <summary>
		/// Not working, exception
		/// </summary>
		ImageProcessor_WuTransform = 2,
		
		/// <summary>
		/// Not working, hanged
		/// </summary>
		AForgeNET_Burkes = 3,
		
		AForgeNET_ColorError = 4,
		
		AForgeNET_ColorImage = 5,
		
		AForgeNET_FloydSteinberg = 7,
		
		AForgeNET_JarvisJudiceNinke = 8,
		
		AForgeNET_OrderedColor = 10,
		
		AForgeNET_SierraColor = 11,
		
		AForgeNET_StuckiColor = 12,

        //(https://msdn.microsoft.com/en-in/library/system.drawing.imaging.pixelformat%28v=vs.110%29.aspx)
        /// <summary>
        /// Specifies that the format is 16 bits per pixel; 5 bits are used for the red component, 6 bits are used for the green component, and 5 bits are used for the blue component.
        /// </summary>
        NETFormat16bppRgb565 = 13,
        /// <summary>
        /// Specifies that the format is 16 bits per pixel; 5 bits each are used for the red, green, and blue components. The remaining bit is not used.
        /// </summary>
        NETFormat16bppRgb555 = 14,
        //Specifies that the format is 8 bits per pixel, indexed. The color table therefore has 256 colors in it.
        NETFormat8bppIndexed = 15
    }
}
