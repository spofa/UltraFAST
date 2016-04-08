/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 29-Mar-16
 * Time: 2:38 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace UltraFAST
{
	/// <summary>
	/// AForge.NET (http://www.aforgenet.com/)
	/// ImageProcessor (http://imageprocessor.org)
	/// EmguCV (http://www.emgu.com/wiki/index.php/Main_Page) [[NET Wrapper To OpenCV]]
	///        (Samples: http://www.emgu.com/wiki/index.php/Tutorial#Examples)
	/// DotImaging (https://github.com/dajuric/dot-imaging)
	/// CUDAFy (http://www.codeproject.com/Articles/572583/CUDA-Programming-Model-on-AMD-GPUs-and-Intel-CPUs)
	/// BitBlt (http://www.codeproject.com/Articles/6710/Using-BitBlt-to-Copy-and-Paste-Graphics)
	/// Accord.NET (http://accord-framework.net/)
	/// </summary>
	public enum TypeOfIMGAlgos
	{
		Win32Native = 0,
		ImageProcessor = 1, //(Uses memory stream, not good n slow)
		EmguCV = 2,   	    //(Not using as opencv v2.4.8 native dll's not found on web)
		DotImaging = 3,     //(methods not found for image processing)
		AForgeNET = 4,
		AccordNET = 5, 	    //(seems statistics library for imaging only)
		CUDAFy = 6, 		//(only nvidia CPU and not for image processing, for parallel tasks)
		NETLanguage = 7,
	}	
}
