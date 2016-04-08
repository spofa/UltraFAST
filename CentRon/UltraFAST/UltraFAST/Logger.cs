/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 31-Mar-16
 * Time: 4:38 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace UltraFAST
{
	/// <summary>
	/// Description of Logger.
	/// </summary>
	public class Logger
	{
		public static bool Enabled = false;
		
		public static void Write(string Message)
		{
			if(Enabled) { Console.Write(Message); }
		}
		
		public static void WriteLine(string Message)
		{
			if(Enabled) { Console.WriteLine(Message); }
		}

	}
}
