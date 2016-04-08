/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 28-Mar-16
 * Time: 6:53 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Drawing;

namespace UltraFAST
{
	/// <summary>
	/// Description of MeasureTiming.
	/// </summary>
	public abstract class AHandler
	{
		private Stopwatch SWatch {get; set;}
		
		private long ElapsedMilliseconds = 0;
		
		public double TimeTakenSec
		{
			get
			{
				return (ElapsedMilliseconds/1000.0);
			}
		}
		
		public double FPSSingle
		{
			get
			{
				return (1.0 / TimeTakenSec);
			}
		}
		
		public bool TExecute(ref Object Input)
		{
//			try
//			{
				//Create watch object
				if(SWatch == null) { SWatch = new Stopwatch();}
				
				//Start our watch
				SWatch.Start();
				
				//Execute code for timing
				bool Result = Execute(ref Input);
				
				//Stop our watch and reset
				SWatch.Stop();
				ElapsedMilliseconds = SWatch.ElapsedMilliseconds;
				SWatch.Reset();
				
				//Return the result
				return Result;
//			}
//			catch(Exception Ex)
//			{
//				return false;
//			}
		}
		
		public abstract bool Execute(ref Object Input);
	}
}
