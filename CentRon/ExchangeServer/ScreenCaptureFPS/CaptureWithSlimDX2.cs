/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 25-Mar-16
 * Time: 3:14 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D9;


namespace ScreenCaptureFPS
{
	/// <summary>
	/// Based On http://spazzarama.com/2009/02/07/screencapture-with-direct3d/.
	/// </summary>
	public class CaptureWithSlimDX2 : ACaptureBase
	{       
		private static SlimDX.Direct3D9.Direct3D _direct3D9 = null;
		private static Dictionary<IntPtr, SlimDX.Direct3D9.Device> _direct3DDeviceCache = new Dictionary<IntPtr, SlimDX.Direct3D9.Device>();
		    
		    
		public static Bitmap CaptureWindow(IntPtr hWnd)
        {
            return CaptureRegionDirect3D(hWnd, NativeMethods.GetAbsoluteClientRect(hWnd));
        }
		        
        public static Bitmap CaptureRegionDirect3D(IntPtr handle, Rectangle region)
        {
            IntPtr hWnd = handle;
            Bitmap bitmap = null;
 
            // We are only supporting the primary display adapter for Direct3D mode
            SlimDX.Direct3D9.AdapterInformation adapterInfo = _direct3D9.Adapters.DefaultAdapter;
            SlimDX.Direct3D9.Device device;
 
            #region Get Direct3D Device
            // Retrieve the existing Direct3D device if we already created one for the given handle
            if (_direct3DDeviceCache.ContainsKey(hWnd))
            {
                device = _direct3DDeviceCache[hWnd];
            }
            // We need to create a new device
            else
            {
                // Setup the device creation parameters
                SlimDX.Direct3D9.PresentParameters parameters = new SlimDX.Direct3D9.PresentParameters();
                parameters.BackBufferFormat = adapterInfo.CurrentDisplayMode.Format;
                Rectangle clientRect = NativeMethods.GetAbsoluteClientRect(hWnd);
                parameters.BackBufferHeight = clientRect.Height;
                parameters.BackBufferWidth = clientRect.Width;
                parameters.Multisample = SlimDX.Direct3D9.MultisampleType.None;
                parameters.SwapEffect = SlimDX.Direct3D9.SwapEffect.Discard;
                parameters.DeviceWindowHandle = hWnd;
                parameters.PresentationInterval = SlimDX.Direct3D9.PresentInterval.Default;
                parameters.FullScreenRefreshRateInHertz = 0;
 
                // Create the Direct3D device
                device = new SlimDX.Direct3D9.Device(_direct3D9, adapterInfo.Adapter, SlimDX.Direct3D9.DeviceType.Hardware, hWnd, SlimDX.Direct3D9.CreateFlags.SoftwareVertexProcessing, parameters);
                _direct3DDeviceCache.Add(hWnd, device);
            }
            #endregion
 
            // Capture the screen and copy the region into a Bitmap
            using (SlimDX.Direct3D9.Surface surface = SlimDX.Direct3D9.Surface.CreateOffscreenPlain(device, adapterInfo.CurrentDisplayMode.Width, adapterInfo.CurrentDisplayMode.Height, SlimDX.Direct3D9.Format.A8R8G8B8, SlimDX.Direct3D9.Pool.SystemMemory))
            {
                device.GetFrontBufferData(0, surface);
 
                // Update: thanks digitalutopia1 for pointing out that SlimDX have fixed a bug
                // where they previously expected a RECT type structure for their Rectangle
                bitmap = new Bitmap(SlimDX.Direct3D9.Surface.ToStream(surface, SlimDX.Direct3D9.ImageFileFormat.Bmp, new Rectangle(region.Left, region.Top, region.Width, region.Height)));
                // Previous SlimDX bug workaround: new Rectangle(region.Left, region.Top, region.Right, region.Bottom)));
 
            }
 
            return bitmap;
        }
        
		public override double CaptureFrames(int numAttempts)
		{
	    
		    int Width = Screen.PrimaryScreen.Bounds.Width;
			int Height = Screen.PrimaryScreen.Bounds.Height;
			
			//Input Parameters & Create Device & Surface
			PresentParameters PPrams = new PresentParameters();
			PPrams.Windowed = true;
			PPrams.SwapEffect = SwapEffect.Discard;	
				
			_direct3D9 = new SlimDX.Direct3D9.Direct3D();
			SlimDX.Direct3D9.AdapterInformation adapterInfo = _direct3D9.Adapters.DefaultAdapter;
			SlimDX.Direct3D9.Device Dvc = new SlimDX.Direct3D9.Device(_direct3D9, 
			                        adapterInfo.Adapter, 
			                        DeviceType.Hardware, 
			                        IntPtr.Zero, 
			                        CreateFlags.SoftwareVertexProcessing, 
			                        PPrams);
			
			SlimDX.Direct3D9.Surface Surf = SlimDX.Direct3D9.Surface.CreateOffscreenPlain(Dvc, 
			                                                                             adapterInfo.CurrentDisplayMode.Width, 
			                                                                             adapterInfo.CurrentDisplayMode.Height, 
			                                                                             SlimDX.Direct3D9.Format.A8R8G8B8, 
			                                                                             SlimDX.Direct3D9.Pool.SystemMemory);				
			Stopwatch stopWatch = new Stopwatch();
		    stopWatch.Start();
		    
		    for (int i = 0; i < numAttempts; i++)
		    {
		    		Dvc.GetFrontBufferData(0, Surf);
		    		Bitmap bmp = new Bitmap(SlimDX.Direct3D9.Surface.ToStream(Surf, 
		    		                                                          SlimDX.Direct3D9.ImageFileFormat.Bmp));

		    }
		    
		    stopWatch.Stop();
		    
		    //Disposal Of Things
		    Surf.Dispose();
		    Dvc.Dispose();
		    
		   
		    double elapsed = (double)stopWatch.ElapsedMilliseconds / 1000.0;
		    double fps = (numAttempts / elapsed);
		    return fps;			
		}
	}
	
    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
 
        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }
 
        public Rectangle AsRectangle
        {
            get
            {
                return new Rectangle(this.Left, this.Top, this.Right - this.Left, this.Bottom - this.Top);
            }
        }
 
        public static RECT FromXYWH(int x, int y, int width, int height)
        {
            return new RECT(x, y, x + width, y + height);
        }
 
        public static RECT FromRectangle(Rectangle rect)
        {
            return new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
    }
    
	[System.Security.SuppressUnmanagedCodeSecurity()]
    internal sealed class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
 
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
 
        /// <summary>
        /// Get a windows client rectangle in a .NET structure
        /// </summary>
        /// <param name="hwnd">The window handle to look up</param>
        /// <returns>The rectangle</returns>
        internal static Rectangle GetClientRect(IntPtr hwnd)
        {
            RECT rect = new RECT();
            GetClientRect(hwnd, out rect);
            return rect.AsRectangle;
        }
 
        /// <summary>
        /// Get a windows rectangle in a .NET structure
        /// </summary>
        /// <param name="hwnd">The window handle to look up</param>
        /// <returns>The rectangle</returns>
        internal static Rectangle GetWindowRect(IntPtr hwnd)
        {
            RECT rect = new RECT();
            GetWindowRect(hwnd, out rect);
            return rect.AsRectangle;
        }
 
        internal static Rectangle GetAbsoluteClientRect(IntPtr hWnd)
        {
            Rectangle windowRect = NativeMethods.GetWindowRect(hWnd);
            Rectangle clientRect = NativeMethods.GetClientRect(hWnd);
 
            // This gives us the width of the left, right and bottom chrome - we can then determine the top height
            int chromeWidth = (int)((windowRect.Width - clientRect.Width) / 2);
 
            return new Rectangle(new Point(windowRect.X + chromeWidth, windowRect.Y + (windowRect.Height - clientRect.Height - chromeWidth)), clientRect.Size);
        }
    }
}
