/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 29-Mar-16
 * Time: 2:41 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace UltraFAST
{
	/// <summary>
	/// StuffMem: Wrapper for "msvcrt.dll" functions for memory
	/// </summary>
	public class StuffMem
	{
		//Copy byte array using byte pointers to array
		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint="memcpy")] 
		public unsafe static extern int MemCopy(byte * Destination, byte * Source, long Counts);

		//Compare byte array using IntPtr
		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcmp")]		
 	    public unsafe static extern int MemCmp(IntPtr left, IntPtr right, long Counts);
		
		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint="memcmp")] 
		public unsafe static extern int MemCmp(byte[] left, byte[] right, UIntPtr Counts);
	}
}
