﻿/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 28-Mar-16
 * Time: 5:32 PM
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
	/// StuffWin32: Wrapper for "user32.dll" functions for Graphics
	/// </summary>
	public class StuffWin32
	{
        #region Class Variables
        
        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;

        public const Int32 CURSOR_SHOWING = 0x00000001;

        [StructLayout(LayoutKind.Sequential)]
        public struct ICONINFO
        {
            public bool fIcon;         // Specifies whether this structure defines an icon or a cursor. A value of TRUE specifies 
            public Int32 xHotspot;     // Specifies the x-coordinate of a cursor's hot spot. If this structure defines an icon, the hot 
            public Int32 yHotspot;     // Specifies the y-coordinate of the cursor's hot spot. If this structure defines an icon, the hot 
            public IntPtr hbmMask;     // (HBITMAP) Specifies the icon bitmask bitmap. If this structure defines a black and white icon, 
            public IntPtr hbmColor;    // (HBITMAP) Handle to the icon color bitmap. This member can be optional if this 
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public Int32 x;
            public Int32 y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO
        {
            public Int32 cbSize;        // Specifies the size, in bytes, of the structure. 
            public Int32 flags;         // Specifies the cursor state. This parameter can be one of the following values:
            public IntPtr hCursor;          // Handle to the cursor. 
            public POINT ptScreenPos;       // A POINT structure that receives the screen coordinates of the cursor. 
        }

        #endregion


        #region Class Functions

        /// <summary>
        /// The GetDesktopWindow function returns a handle to the desktop window. The desktop window covers the entire screen. The desktop window 
        /// is the area on top of which other windows are painted.
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        public static extern IntPtr GetDesktopWindow();

        /// <summary>
        /// The GetDC function retrieves a handle to a device context (DC) for the client area of a specified window or for the entire screen. You 
        /// can use the returned handle in subsequent GDI functions to draw in the DC. The device context is an opaque data structure, whose values 
        /// are used internally by GDI. The GetDCEx function is an extension to GetDC, which gives an application more control over how and whether 
        /// clipping occurs in the client area.
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetDC")]
        public static extern IntPtr GetDC(IntPtr ptr);

        [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
        public static extern int GetSystemMetrics(int abc);

        [DllImport("user32.dll", EntryPoint = "GetWindowDC")]
        public static extern IntPtr GetWindowDC(Int32 ptr);

        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);


        [DllImport("user32.dll", EntryPoint = "GetCursorInfo")]
        public static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll", EntryPoint = "CopyIcon")]
        public static extern IntPtr CopyIcon(IntPtr hIcon);

        [DllImport("user32.dll", EntryPoint = "GetIconInfo")]
        public static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);


        #endregion
	}
}
