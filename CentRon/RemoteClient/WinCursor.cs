using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

public class WinCursor
{

    [StructLayout(LayoutKind.Sequential)]
    struct CURSORINFO
    {
        public Int32 cbSize;
        public Int32 flags;
        public IntPtr hCursor;
        public POINTAPI ptScreenPos;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct POINTAPI
    {
        public int x;
        public int y;
    }

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool DrawIconEx(IntPtr hdc, int xLeft, int yTop, IntPtr hIcon, int cxWidth, int cyHeight, int istepIfAniCur, IntPtr hbrFlickerFreeDraw, int diFlags);

    private const Int32 CURSOR_SHOWING = 0x0001;
    private const Int32 DI_NORMAL = 0x0003;
    //With all these hacks i keep needing to do I need to learn C++ and go back to COM that microdoft kicked us all off with .NET about 15 years ago and still uses at its core

    [DllImport("user32.dll")]
    static extern bool GetCursorInfo(out CURSORINFO pci);
    public static Color CaptureCursor(ref int X, ref int Y, Graphics theShot, int ScreenServerX, int ScreenServerY)
    {//We return a color so that it can be embeded in a bitmap to be returned to the client program
        IntPtr C = Cursors.Arrow.Handle;
        CURSORINFO pci;
        pci.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CURSORINFO));
        if (GetCursorInfo(out pci))
        {
            X = pci.ptScreenPos.x;
            Y = pci.ptScreenPos.y;
            if (pci.hCursor == Cursors.Default.Handle) return Color.Red;
            else if (pci.hCursor == Cursors.WaitCursor.Handle) return Color.Green;
            else if (pci.hCursor == Cursors.Arrow.Handle) return Color.Blue;
            else if (pci.hCursor == Cursors.IBeam.Handle) return Color.White;
            else if (pci.hCursor == Cursors.Hand.Handle) return Color.Violet;
            else if (pci.hCursor == Cursors.SizeNS.Handle) return Color.Yellow;
            else if (pci.hCursor == Cursors.SizeWE.Handle) return Color.Orange;
            else if (pci.hCursor == Cursors.SizeNESW.Handle) return Color.Aqua;
            else if (pci.hCursor == Cursors.SizeNWSE.Handle) return Color.Pink;
            else if (pci.hCursor == Cursors.PanEast.Handle) return Color.BlueViolet;
            else if (pci.hCursor == Cursors.HSplit.Handle) return Color.Cyan;
            else if (pci.hCursor == Cursors.VSplit.Handle) return Color.DarkGray;
            else if (pci.hCursor == Cursors.Help.Handle) return Color.DarkGreen;
            else if (pci.hCursor == Cursors.AppStarting.Handle) return Color.SlateGray;
            if (pci.flags == CURSOR_SHOWING)
            {//We need to caputer the mouse image and embed tha in the screen shot of the desktop because it's a custom mouse cursor
                float XReal = pci.ptScreenPos.x * (float)ScreenServerX / (float)Screen.PrimaryScreen.Bounds.Width - 11;
                float YReal = pci.ptScreenPos.y * (float)ScreenServerY / (float)Screen.PrimaryScreen.Bounds.Height - 11;
                int x = Screen.PrimaryScreen.Bounds.X;
                var hdc = theShot.GetHdc();
                DrawIconEx(hdc, (int)XReal, (int)YReal, pci.hCursor, 0, 0, 0, IntPtr.Zero, DI_NORMAL);
                theShot.ReleaseHdc();
            }
            return Color.Black;
        }
        X = 0;
        Y = 0;
        return Color.Black;
    }
    public static Cursor ColorToCursor(Color C)
    {//Code for the client that pulls a pixel from the picture and converts it to a cursor
        if (C.ToArgb() == Color.Red.ToArgb()) return Cursors.Default;
        if (C.ToArgb() == Color.Green.ToArgb()) return Cursors.WaitCursor;
        if (C.ToArgb() == Color.Blue.ToArgb()) return Cursors.Arrow;
        if (C.ToArgb() == Color.White.ToArgb()) return Cursors.IBeam;
        if (C.ToArgb() == Color.Violet.ToArgb()) return Cursors.Hand;
        if (C.ToArgb() == Color.Yellow.ToArgb()) return Cursors.SizeNS;
        if (C.ToArgb() == Color.Orange.ToArgb()) return Cursors.SizeWE;
        if (C.ToArgb() == Color.Aqua.ToArgb()) return Cursors.SizeNESW;
        if (C.ToArgb() == Color.Pink.ToArgb() || C.B == 206) return Cursors.SizeNWSE;
        if (C.ToArgb() == Color.BlueViolet.ToArgb()) return Cursors.PanEast;
        if (C.ToArgb() == Color.Cyan.ToArgb()) return Cursors.HSplit;
        if (C.ToArgb() == Color.DarkGray.ToArgb()) return Cursors.VSplit;
        if (C.ToArgb() == Color.DarkGreen.ToArgb()) return Cursors.Help;
        if (C.ToArgb() == Color.SlateGray.ToArgb()) return Cursors.AppStarting;
        if (C.ToArgb() == Color.Fuchsia.ToArgb()) return Cursors.No;
        return Cursors.Default;
    }
}
