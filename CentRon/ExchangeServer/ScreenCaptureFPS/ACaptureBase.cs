/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 25-Mar-16
 * Time: 1:25 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace ScreenCaptureFPS
{
	/// <summary>
	/// Description of ACaptureBase.
	/// </summary>
	public abstract class ACaptureBase
	{
		public abstract double CaptureFrames(int numAttempts);
	}

    //This structure shall be used to keep the size of the screen.
    public struct SIZE
    {
        public int cx;
        public int cy;
    }
}