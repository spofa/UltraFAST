/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 29-Mar-16
 * Time: 10:56 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.CodeDom.Compiler;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections.Generic;

using AFFilters = AForge.Imaging.Filters;
using System.IO;

namespace UltraFAST
{
    /// <summary>
    /// Main Code, Does Everything
    /// </summary>
    public class MainApplication : AHandler
    {
        /// <summary>
        /// Number of frame rows
        /// </summary>
        public static int nRows { get; set; }

        /// <summary>
        /// If != -1 tile size will be these pixels
        /// </summary>
        public int nTileTXSzXPixels = -1;

        /// <summary>
        /// If != -1 tile size will be these pixels
        /// </summary>
        public int nTileTXSzYPixels = -1;

        /// <summary>
        /// If != -1 tile count will be these rows
        /// </summary>
        public int nRowsDefault = -1;

        /// <summary>
        /// If != -1 tile count will be these coloums
        /// </summary>
        public int nColoumsDefault = -1;

        /// <summary>
        /// If true reduce all tiles, otherwise only changed ones
        /// </summary>
        public bool ReduceAllTiles = true;

        /// <summary>
        /// Resize after capture
        /// </summary>
        public int def_maxAllowedScrSz4TransmitX = -1;

        /// <summary>
        /// Resize after capture
        /// </summary>
        public int def_maxAllowedScrSz4TransmitY = -1;

        /// <summary>
        /// Number of frame coloums
        /// </summary>
        public static int nCols { get; set; }

        /// <summary>
        /// List of regions of from Desktop
        /// </summary>
        public static List<ROIArea> DSKTopTileRegions { get; set; }

        /// <summary>
        /// Captured image (From Desktop)
        /// </summary>
        public static Bitmap bmpCaptured { get; set; }

        /// <summary>
        /// Reduced image (After Capture)
        /// </summary>
        public static Bitmap bmpReduced { get; set; }

        /// <summary>
        /// Region of whole desktop (capturing area)
        /// </summary>
        public static ROIArea DskAreaToCapture { get; set; }

        /// <summary>
        /// Used for image flipping at various places
        /// </summary>
        public static bool IsSrcNegativeStride { get; set; }

        /// <summary>
        /// If true quantize to 256 colors
        /// </summary>
        public   bool Is8BitQuantize { get; set; }

        /// <summary>
        /// If true convert to JPEG also with JPEGConversionQuality
        /// </summary>
        public bool EnableJPEGConversion   { get; set; }

        /// <summary>
        /// JPEG Compression Quality (1.0 = 100%)
        /// </summary>
        public double JPEGConversionQuality = 1.0;

        /// <summary>
        /// If true compress each tile before sending
        /// </summary>
        public bool EnableCompression { get; set; }

        /// <summary>
        /// Initialize Screen Dimensions To Desktop Size
        /// </summary>
        public void InitScreensSizeToDesktop()
        {
            //Find Desktop Dimensions And Paste It Here
            if (DskAreaToCapture == null) { DskAreaToCapture = new ROIArea(); }
            Logger.WriteLine(String.Format("DESKTOP SIZE# {0} x {1}", DskAreaToCapture.DskWidth, DskAreaToCapture.DskHeight));
        }

        /// <summary>
        /// Find number of rows and coloums desktop to be broken based on number of threads.
        /// nRows=CORE/2, nCols=CORE-nRows [Min = 1, Max = 4]
        /// </summary>
        public void ComputeRowsAndColoumsBasedOnCPU()
        {
            //#0) If transmit tile size is defined (compute nRows/nCols accordingly)
            if ((nTileTXSzXPixels > 0) && (nTileTXSzYPixels > 0))
            {
                nRows = (int)Math.Round((double)DskAreaToCapture.Height / nTileTXSzYPixels);
                if (nRows <= 1) { nRows = 3; }   //Negative and zero protection
                nCols = (int)Math.Round((double)DskAreaToCapture.Width / nTileTXSzYPixels);
                if (nCols <= 1) { nCols = 3; }	 //Negative and zero protection	

                //Return from UI
                return;
            }

            //#1) If transmit tile count is defined (take nRows/nCols accordingly)
            if ((nRowsDefault > 0) && (nColoumsDefault > 0))
            {
                nRows = nRowsDefault;
                nCols = nColoumsDefault;

                //Return from UI
                return;
            }

            #region "Default Dynamic Logic"
            //#1) Ensure minimum 2X2 tile even for single core
            int nRowsProtMin = 2;
            int nColsProtMin = 2;

            //#2) Max Rows & Cols To Maintain Minimum Tile Size > (256 x 256) for any screen resolution
            int TargetMinTileSize = 256;            //Input (Fixed 256)			
            int nColsTilesMax = (int)Math.Round((double)DskAreaToCapture.Width / (double)TargetMinTileSize);
            if (nColsTilesMax <= 1) { nColsProtMin = 2; }   //Negative and zero protection
            int nRowsTilesMax = (int)Math.Round((double)DskAreaToCapture.Height / (double)TargetMinTileSize);
            if (nRowsTilesMax <= 1) { nRowsProtMin = 2; }   //Negative and zero protection	

            //#3) Max Rows & Cols that can be handled by CPU cores are recommended as
            //12 Core   :4 X 4 [16]
            //8 Core    :4 X 4 [16]
            //6 Core    :4 X 3 [12]
            //4 Core    :4 X 3 [12]
            //2 Core    :3 X 2 [06]
            //1 Core    :3 X 2 [06]
            int nThreads = CPUStuff.ProcessorCount;
            int nRowsThreadMax = nRowsProtMin;  //Deafult
            int nColsThreadMax = nColsProtMin;  //Default
            if (nThreads <= 2) { nRowsThreadMax = 3; nColsThreadMax = 2; }
            else if (nThreads <= 6) { nRowsThreadMax = 4; nColsThreadMax = 3; }
            else if (nThreads <= 12) { nRowsThreadMax = 3; nColsThreadMax = 2; }
            else if (nThreads > 12) { nRowsThreadMax = (nThreads / 2); nColsThreadMax = (nThreads - nRowsThreadMax); }

            //Right Max from #2 and #3
            int nRowsMax = (int)Math.Min(nRowsTilesMax, nRowsThreadMax);
            int nColsMax = (int)Math.Min(nColsTilesMax, nColsThreadMax);

            //Select nRows & nCols = Max but > Safe Value
            nRows = (int)Math.Max(nRowsMax, nRowsProtMin);
            nCols = (int)Math.Min(nColsMax, nColsProtMin);

            //			nRows = nThreads / 2;	//Half processor for Rows
            //			if(nRows == 0) { nRows = 1; }			//Can't be zero Rows 
            //			if(nRows > 4)  { nRows = 4; }			//Can't be more than 4 Rows
            //			nCols = nThreads - nRows;
            //			if(nCols == 0)  { nCols = 1; }			//Can't be zero Cols
            //			if(nCols > 4)  { nCols = 4; }           //Can't be more than 4 Cols

            //FORCE ROWS AND COLOUMS HERE
            //nRows = 2;
            //nCols = 2;
            #endregion

            Logger.WriteLine(String.Format("nRows x nCols# {0} x {1}", nRows.ToString(), nCols.ToString()));
        }

        /// <summary>
        /// Compute bounding rectangles of each region on the desktop (uses parallelism)
        /// </summary>
        public void ComputeRegionsBasedOnImageSize(int RgnFullWidth, int RgnFullHeight)
        {
            //Get Number of Loops to Execute
            //- Desktop is broken into zero based (nRows x nCols) matrix
            //- Where nRows and nCols are derived based on CPU threads			
            int nLoop = nRows * nCols;

            //Clear or Initialize DSKTopRegions
            if (DSKTopTileRegions == null)
            {
                DSKTopTileRegions = new List<ROIArea>();
            }
            else
            {
                DSKTopTileRegions.Clear();
            }

            //Perform Computations In Parallel (Skipping As This Will Make Regions Unordered)
            //Parallel.For(0, nLoop, iThread => { ComputeRegions(iThread); });
            for (int iThread = 0; iThread < nLoop; iThread++) { ComputeRegions(iThread, RgnFullWidth, RgnFullHeight); };
        }

        /// <summary>
        /// Compute row, col, bouning rectangle based on regions
        /// </summary>
        /// <param name="indexCTR"></param>
        /// <param name="CaptureWidth"></param>
        /// <param name="CaptureHeight"></param>
        private void ComputeRegions(int indexCTR, int RgnFullWidth, int RgnFullHeight)
        {
            //Setup Current Thread To High Priority
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            //Convert iThread to (row, col) of Desktop Image Matrix
            int row = (indexCTR / nCols);
            int col = (indexCTR % nCols);
            //String uID = String.Format("{0}: r{1}, c{2}", iThread.ToString(), row.ToString(), col.ToString());
            //Logger.WriteLine(uID);

            //Find Bounding Box (Region) from a specific (row, col) on Desktop
            //- Size of each row and coloum in pixels
            int szRow = (RgnFullHeight / nRows);
            int szCol = (RgnFullWidth / nCols);
            //Logger.WriteLine(szRow.ToString() + ", " + szCol.ToString());
            //- Coordinates of bounding box (left, top, width, height)
            int boundLeft = (col * szCol);
            int boundTop = (row * szRow);
            int boundWidth = szCol;
            int boundHeight = szRow;

            //- Adjustment of coordinates for last row or coloum
            if (row == (nRows - 1)) { boundHeight = (RgnFullHeight - boundTop); }
            if (col == (nCols - 1)) { boundWidth = (RgnFullWidth - boundLeft); }

            //Compute region and add it to the list
            ROIArea iRegion = new ROIArea(indexCTR, boundLeft, boundTop, boundWidth, boundHeight);

            //Logger.WriteLine(uID + ":: " + Frame.ToString());
            lock (DSKTopTileRegions)
            {
                DSKTopTileRegions.Add(iRegion);
            }
        }

        /// <summary>
        /// OTF
        /// </summary>
        public bool IsInitialized = false;

        /// <summary>
        /// Call to read settings
        /// </summary>
        public void ReInitializeAndReadSettings()
        {
            this.IsInitialized = false;
            DSKTopTileRegions.Clear();
            DSKTopTileRegions = null;
        }

        private void InitalizaeEverything()
        {
            if (!IsInitialized)
            {
                //Screen Sizes
                InitScreensSizeToDesktop();

                //Rows/Coloums
                ComputeRowsAndColoumsBasedOnCPU();

                //Configure tile regions based on dimension of desktop
                //(if resized then tiles based on new size)
                if ((def_maxAllowedScrSz4TransmitX > 0) && (def_maxAllowedScrSz4TransmitY > 0))
                {
                    ComputeRegionsBasedOnImageSize(this.def_maxAllowedScrSz4TransmitX,
                                                   this.def_maxAllowedScrSz4TransmitY);
                }
                else
                {
                    ComputeRegionsBasedOnImageSize(MainApplication.DskAreaToCapture.DskWidth,
                                                   MainApplication.DskAreaToCapture.DskHeight);
                }

                //Initialize Modules
                InitializeModules();

                //Initialization Complete
                IsInitialized = true;
            }
        }


        //Filters for project
        public static GDICapture filtGDIDesktopCapture { get; set; }
        public static IMGResizing filtEnsureManageableCaptureSz { get; set; }
        public static IMGCropper filtScreenToTiles { get; set; }
        public static IMGQuantizn filtQuantizeTileToSend { get; set; }
        public static IMGResizing filtResizingTileToSend { get; set; }

        //Initialize project filters
        public void InitializeModules()
        {
            if (filtGDIDesktopCapture == null) { filtGDIDesktopCapture = new GDICapture(); }
            if (filtScreenToTiles == null) { filtScreenToTiles = new IMGCropper(); }
            if (filtEnsureManageableCaptureSz == null) { filtEnsureManageableCaptureSz = new IMGResizing(); }
            if (filtQuantizeTileToSend == null) { filtQuantizeTileToSend = new IMGQuantizn(); }
            if (filtResizingTileToSend == null) { filtResizingTileToSend = new IMGResizing(); }
        }

        /// <summary>
        /// Define nRows and nCols based on CPU 
        /// </summary>
        public MainApplication()
        {
            //Initialize Everything
            InitalizaeEverything();
        }

        /// <summary>
        /// If true use TExecute calls
        /// </summary>
        private bool UseTimedMethods = false;

        #region implemented abstract members of AHandler
        public override bool Execute(ref object Input)
        {
            //Speed Up This Thread To High Priority
            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;

            //Initialize Everything
            InitalizaeEverything();

            //Application Object
            Object MySelf = this;

            //Full Desktop Capture (GDIWin32Method: Fastest) Into gdiDesktopCapture.bmpCaptured
            Object RgnToCapture = MainApplication.DskAreaToCapture;
            if (UseTimedMethods) { ((AHandler)filtGDIDesktopCapture).TExecute(ref RgnToCapture); } else { filtGDIDesktopCapture.Execute(ref RgnToCapture); }
            MainApplication.bmpCaptured = filtGDIDesktopCapture.bmpCaptured;

            //ReSize Image To Manageable Size (Like 1080p). When Desktop Is Huge Say UHD
            if (def_maxAllowedScrSz4TransmitX > 0) { IMGResizing.def_maxAllowedScrSz4TransmitX = def_maxAllowedScrSz4TransmitX; }
            if (def_maxAllowedScrSz4TransmitY > 0) { IMGResizing.def_maxAllowedScrSz4TransmitY = def_maxAllowedScrSz4TransmitY; }
            if (UseTimedMethods) { ((AHandler)filtEnsureManageableCaptureSz).TExecute(ref MySelf); } else { filtEnsureManageableCaptureSz.Execute(ref MySelf); }
            MainApplication.bmpReduced = filtEnsureManageableCaptureSz.bmpReduced;

            //After reduction, no need to keep high resolution capture in RAM				
            MainApplication.bmpCaptured.Dispose();
            MainApplication.bmpCaptured = null;
            filtGDIDesktopCapture.bmpCaptured = null;

            //Crop ReSized (Managable Sized) Image in (MainApplication.bmpReduced) 
            //into regions in (MainApplication.DSKTopRegions.CroppedImage)            
            if (UseTimedMethods) { ((AHandler)filtScreenToTiles).TExecute(ref MySelf); } else { filtScreenToTiles.Execute(ref MySelf); }

            //Release Unnecessary Pointer to 'bmpReduced' 
            //(each roi already pointing to MainApplication.bmpReduced)
            MainApplication.bmpReduced = null;
            filtEnsureManageableCaptureSz.bmpReduced = null;

            Logger.Write(string.Format("\r\n[CAPT: {0:0.00}, CHG: ", filtGDIDesktopCapture.TimeTakenSec +
                                       filtEnsureManageableCaptureSz.TimeTakenSec +
                                       filtScreenToTiles.TimeTakenSec));

            //Capture[Full Desktop] -> ReduceToLimits -> CropTilesOfImage -> If[TileChanged] -> Optional[Reduce/CompressTile] -> Send
            //Optional STEP: Check whether each roiArea is changed during capture;
            //               Reduce Tiles To Be Sent [Not All To Save Time]
            //(ORIGINAL MEANS DON'T QUANTIZE)
            //1. Reduce Tile Size (H x W): Disabled
            //2. Quantize (bpp)          : trying 16bpp565 or 16bpp555
            //3. Convert (JPEG)          : Enabled with Quality Control
            //4. ZIPCompress             : Did not helped 
            TypeOfQuantizers QuantizerToUse = TypeOfQuantizers.NONE;
            if (Is8BitQuantize) {

                QuantizerToUse = TypeOfQuantizers.ImageProcessor_OctTree;
            }

            TypesOfResizing ResizerToUse = TypesOfResizing.NONE;
            double ResizerToUseReduceBy = 1.0;
            TypesOfImageConversion ImageConverterToUse = TypesOfImageConversion.NONE;
            if (EnableJPEGConversion)
            {
                ImageConverterToUse = TypesOfImageConversion.JPEG; 
            }
            double CompressionQuality = JPEGConversionQuality;
            TypesOfTileCompression TileCompressorToUse = TypesOfTileCompression.NONE;
            if(EnableCompression)
            {
                TileCompressorToUse = TypesOfTileCompression.Lz4Net;
            }

            //Parallel.ForEach<ROIArea>(MainApplication.DSKTopTileRegions, roiAreaTile => UpdateROIChangedFlag(roiAreaTile));	
            foreach (ROIArea roiAreaTile in MainApplication.DSKTopTileRegions)
            {
                #region "Detect If Tile Is Changed" 
                //(false, -0.2) ==> whole image array compare 
                //(false, 0.n)  ==> line compare, differ if 0.1*height lines changed [[if n is high does not compare]]
                //(true, *, K)     ==> use internal variable for algorithm
                //       K: 1 - gray, 2 - 50%, 3 - 256x356

                //-------------------------timings (logging turned on) ----------------
                //(false, 0.1)        : worst, in FPS is 0.2 (reject)
                //(false, -0.2)       : 8.66FPS  
                //                      -> (10.25FPS with Logging Off) 
                //                      -> 8.5 (With Parallel Difference)
                //(true, -0.2, 1/2/3) : 6.53/6.75/7.04 FPS
                roiAreaTile.IsROITileChanged = StuffImaging.IsBitmapsDiffer(roiAreaTile.CroppedTileBitmap,
                                                                            roiAreaTile.prevCroppedTileBitmap,
                                                                             false, -0.2);

                //roiAreaTile.IsROITileChanged = StuffImaging.IsBitmapsDifferNew(roiAreaTile.CroppedTileBitmap, roiAreaTile.prevCroppedTileBitmap);

                #endregion

                #region "Reduce TileBitmap Size By Series Of Filters"
                if (ReduceAllTiles == true)
                {
                    ReduceTileBytes(QuantizerToUse, ResizerToUse, ResizerToUseReduceBy, ImageConverterToUse, CompressionQuality, roiAreaTile, TileCompressorToUse);
                }
                else
                {
                    if (roiAreaTile.IsROITileChanged)
                    {
                        ReduceTileBytes(QuantizerToUse, ResizerToUse, ResizerToUseReduceBy, ImageConverterToUse, CompressionQuality, roiAreaTile, TileCompressorToUse);
                    }
                }
                #endregion

                #region "Move Previous Image"
                roiAreaTile.prevCroppedTileBitmap = roiAreaTile.CroppedTileBitmap;
                roiAreaTile.CroppedTileBitmap = null;
                #endregion
            }

            //Logger.WriteLine("");
            return true;
        }

        /// <summary>
        /// On Each Tile Performs [Resize + Quantize + Convert(JPEG) + Compress (LZ4) ]
        /// </summary>
        /// <param name="QuantizerToUse"></param>
        /// <param name="ResizerToUse"></param>
        /// <param name="ResizerToUseReduceBy"></param>
        /// <param name="ImageConverterToUse"></param>
        /// <param name="CompressionQuality"></param>
        /// <param name="roiAreaTile"></param>
        private void ReduceTileBytes(TypeOfQuantizers QuantizerToUse, TypesOfResizing ResizerToUse, double ResizerToUseReduceBy, 
            TypesOfImageConversion ImageConverterToUse, double CompressionQuality, ROIArea roiAreaTile,
            TypesOfTileCompression TileCompressorToUse)
        {
            //Quantize Tiles To Be Sent [Not All To Save Time]
            if (roiAreaTile.IsROITileChanged)
            {
                //Read The Cropped Tile From ROITile In Cyclic Bitmap
                Bitmap CyclicBmp = roiAreaTile.CroppedTileBitmap;

                #region "OPTIONAL: Resize Tile (H x W - ResizerToUseReduceBy%)"
                if (ResizerToUse != TypesOfResizing.NONE)
                {
                    //Read The Previous Output
                    //CyclicBmp = CyclicBmp; (Nothing To Do Here)

                    //Settings for Resizing Routine
                    filtResizingTileToSend.ReduceToSizeX = (int)(ResizerToUseReduceBy * CyclicBmp.Width);
                    filtResizingTileToSend.ReduceToSizeY = (int)(ResizerToUseReduceBy * CyclicBmp.Height);
                    filtResizingTileToSend.SaveFileName = String.Format("TILE_{0}_ReSized", roiAreaTile.RgnIndex);

                    //Input Image To Filter As Object & Execute Filter
                    Object filtInpImage = CyclicBmp;
                    if (UseTimedMethods)
                    {
                        ((AHandler)filtResizingTileToSend).TExecute(ref filtInpImage);
                    }
                    else
                    {
                        filtResizingTileToSend.Execute(ref filtInpImage);
                    }

                    //Read Output From The Filter & Wipe (Save RAM)
                    CyclicBmp = filtResizingTileToSend.bmpReduced;
                    filtResizingTileToSend.bmpReduced = null;
                }
                #endregion

                #region "OPTIONAL: Quantize Tile (BPP - 255/8)"
                if (QuantizerToUse != TypeOfQuantizers.NONE)
                {
                    //Settings for Quantization Filter
                    filtQuantizeTileToSend.maxColorsPellet = 255;
                    filtQuantizeTileToSend.maxColorBits = 8;
                    filtQuantizeTileToSend.QuantizerToUse = QuantizerToUse;
                    filtQuantizeTileToSend.def_saveFileNameDefault = String.Format("TILE_{0}_Quantized", roiAreaTile.RgnIndex);

                    //Input Image To Filter As Object & Execute Filter
                    Object filtInpImage = CyclicBmp;
                    if (UseTimedMethods)
                    {
                        ((AHandler)filtQuantizeTileToSend).TExecute(ref filtInpImage);
                    }
                    else
                    {
                        filtQuantizeTileToSend.Execute(ref filtInpImage);
                    }

                    //Reset Filter Settings to Default
                    filtQuantizeTileToSend.maxColorsPellet = IMGQuantizn.def_maxColorsPellet;
                    filtQuantizeTileToSend.maxColorBits = IMGQuantizn.def_maxColorBits;
                    filtQuantizeTileToSend.QuantizerToUse = IMGQuantizn.def_QuantizerToUse;

                    //Read Output From The Filter & Wipe (Save RAM)
                    CyclicBmp = filtQuantizeTileToSend.bmpQuantized;
                    filtQuantizeTileToSend.bmpQuantized = null;
                };
                #endregion

                #region "OPTIONAL: Convert Tile (BMP/JPEG/...)"
                if (ImageConverterToUse != TypesOfImageConversion.NONE)
                {
                    String fName = String.Format("TILE_{0}_Converted", roiAreaTile.RgnIndex);
                    //fName = ""; //Don't Save Output
                    //Stopwatch SWatch = new Stopwatch();
                    //SWatch.Start();
                    Bitmap outBitmap = StuffImaging.BitmapConvert(CyclicBmp,
                                                                  TypesOfImageConversion.JPEG,
                                                                  CompressionQuality,
                                                                  TypeOfIMGAlgos.NETLanguage,
                                                                  fName);
                    //SWatch.Stop();
                    //double elp =SWatch.ElapsedMilliseconds/1000.0;
                    //Logger.WriteLine("CONVERSION# {0:0.00}",  elp);

                    CyclicBmp = null;
                    CyclicBmp = outBitmap;
                }
                #endregion

                #region "OPTIONAL: ZIP Compression (Not Implemented)"
                if(TileCompressorToUse != TypesOfTileCompression.NONE)
                {
                    switch(TileCompressorToUse)
                    {
                        case TypesOfTileCompression.Lz4Net:
                            using (MemoryStream ms = new MemoryStream())
                            {
                                //Save Image to Stream
                                if (EnableJPEGConversion)
                                {
                                    CyclicBmp.Save(ms, ImageFormat.Jpeg);
                                }
                                else
                                {
                                    CyclicBmp.Save(ms, ImageFormat.Bmp);
                                }
                                //Compress to ZIP
                                 roiAreaTile.byteArrayToTransmit = Lz4Net.Lz4.CompressBytes(ms.ToArray(), Lz4Net.Lz4Mode.HighCompression);
                            }
                            break;
                        default:
                            using (MemoryStream ms = new MemoryStream())
                            {
                                //Save Image to Stream
                                if (EnableJPEGConversion)
                                {
                                    CyclicBmp.Save(ms, ImageFormat.Jpeg);
                                }
                                else
                                {
                                    CyclicBmp.Save(ms, ImageFormat.Bmp);
                                }
                                //Compress to ZIP
                                roiAreaTile.byteArrayToTransmit = Lz4Net.Lz4.CompressBytes(ms.ToArray(), Lz4Net.Lz4Mode.HighCompression);
                            }
                            break;
                    }
                }
                #endregion

                //Finally Send Tile to UDP (Reduced One)
                roiAreaTile.bmpToTransmit = CyclicBmp;
            }
            else
            {
                //Finally Send Tile to UDP (Captured One)
                roiAreaTile.bmpToTransmit = roiAreaTile.CroppedTileBitmap;
            }
        }
        #endregion
    }
}
