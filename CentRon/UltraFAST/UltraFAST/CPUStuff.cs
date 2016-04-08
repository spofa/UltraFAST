/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 29-Mar-16
 * Time: 11:05 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace UltraFAST
{
	/// <summary>
	/// CPUStuff: Find Core/Threads of CPU, BASED on http://stackoverflow.com/questions/13015794/how-to-get-number-of-cpus-logical-cores-threads-in-c
	/// </summary>
	public class CPUStuff
	{
		
		private static int _CoreCount = -1;
		
		/// <summary>
		/// Count of cores in CPU
		/// </summary>
		public static int CoreCount
		{
			get
			{
				if(_CoreCount == -1)
				{
					int tmpTotal = 0;
					foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
					{
					    tmpTotal += int.Parse(item["NumberOfCores"].ToString());
					}
					_CoreCount = tmpTotal;
				}
				return _CoreCount;
			}
		}
		
		private static int _LogicalProcessors = -1;
		
		/// <summary>
		/// Logical processors in system
		/// </summary>
		public static int LogicalProcessors
		{
			get
			{
				if(_LogicalProcessors == -1)
				{
					foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get())
					{
						_LogicalProcessors = int.Parse(item["NumberOfLogicalProcessors"].ToString());
					}
				}
				return _LogicalProcessors;
			}
		}
		
		private static int _ProcessorCount = -1;
		
		/// <summary>
		/// Count of processors in a system
		/// </summary>
		public static int ProcessorCount
		{
			get
			{
				if(_ProcessorCount == -1)
				{
					_ProcessorCount = Environment.ProcessorCount;
				}
				
				return _ProcessorCount;
			}
		}
		
		/// <summary>
		/// If OS is 64 Bit
		/// </summary>
		/// <returns></returns>
		public static bool Is64BitSystem()
		{
			return Environment.Is64BitOperatingSystem;
		}
		
		/// <summary>
		/// If APP is 64 Bit
		/// </summary>
		/// <returns></returns>
		public static bool Is64BitProcess()
		{
			return Environment.Is64BitProcess;
		}
		
		public override string ToString()
		{
			return String.Format("CORE# {0}, LOGICAL# {1}, PROCESSOR# {2}", CoreCount, LogicalProcessors, ProcessorCount);
		}
	}
}
