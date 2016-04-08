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
		
		AForgeNET_StuckiColor = 12
	}
}
