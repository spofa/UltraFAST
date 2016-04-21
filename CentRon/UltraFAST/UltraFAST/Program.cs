/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 28-Mar-16
 * Time: 5:16 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace UltraFAST
{
	/// <summary>
	/// Image Processing Libraries Are:
	/// - OpenCV NET Wrapper (http://www.emgu.com/wiki/index.php/Main_Page)
	/// - AForge.NET (
	/// </summary>
	class Program
	{
		/// <summary>
		/// Save all images to disk
		/// </summary>
		public static bool SaveAllOutputsToDiskInTEMPFolder = false;

		/// <summary>
		/// Here bitmaps will stored
		/// </summary>
		public const string SaveOutputToDiskTEMPFolder = @"C:\TEMP\";
		
		//http://stackoverflow.25lm.com/questions/32022849/capture-desktop-make-it-256-color-and-send-it-over-internet
		//http://bobcravens.com/2009/04/create-a-remote-desktop-viewer-using-c-and-wcf/
		public static void Main(string[] args)
		{
			//Logger.WriteLine("Hello World!");


   //         UDPP2PServer s = new UDPP2PServer();
   //         UDPP2PClient c1 = new UDPP2PClient("SACHIN", "200.168.1.7");
   //         Thread.Sleep(5000);
            
   //         Logger.WriteLine(c1.ReadFromServer());
   //         UDPP2PClient c2 = new UDPP2PClient("SUMIT", "200.168.1.7");
   //         Thread.Sleep(5000);
            
			//Logger.WriteLine(c2.ReadFromServer());

            MainApplication Appliation = new MainApplication();
            Appliation.ReduceAllTiles = true;
            Appliation.def_maxAllowedScrSz4TransmitX = 1366;
            Appliation.def_maxAllowedScrSz4TransmitY = 768;
            Appliation.nColoumsDefault = 2;
            Appliation.nRowsDefault = 2;
            Appliation.ReInitializeAndReadSettings();

            Logger.WriteLine("Press any key to continue . . .");
			Console.ReadKey(true);
			
			double elapsed = 0.0;
			int numAttempts = 1;
			
			for(int i = 0; i<numAttempts; i++)
			{
				Object Dummy = null;
				((AHandler)Appliation).TExecute(ref Dummy);
				if(i!=0)
				{
					elapsed += ((AHandler)Appliation).TimeTakenSec;
				}
			}
			
			double FPS = numAttempts / elapsed;
			
			Logger.WriteLine(string.Format("\r\n\r\nFPS: {0:0.00}", FPS));
			
			Logger.WriteLine("Press any key to exit . . . ");
			Console.ReadKey(true);
		}
	}
}