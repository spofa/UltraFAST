using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using SimplePaletteQuantizer.ColorCaches;
using SimplePaletteQuantizer.ColorCaches.Common;
using SimplePaletteQuantizer.ColorCaches.EuclideanDistance;
using SimplePaletteQuantizer.ColorCaches.LocalitySensitiveHash;
using SimplePaletteQuantizer.ColorCaches.Octree;
using SimplePaletteQuantizer.Extensions;
using SimplePaletteQuantizer.Helpers;
using SimplePaletteQuantizer.Quantizers;
using SimplePaletteQuantizer.Quantizers.HSL;
using SimplePaletteQuantizer.Quantizers.MedianCut;
using SimplePaletteQuantizer.Quantizers.NeuQuant;
using SimplePaletteQuantizer.Quantizers.Octree;
using SimplePaletteQuantizer.Quantizers.Popularity;
using SimplePaletteQuantizer.Quantizers.Uniform;
using SimplePaletteQuantizer.Quantizers.XiaolinWu;

namespace SimplePaletteQuantizer
{
    public partial class MainForm : Form
    {
        #region | Fields |

        private Image gifImage;
        private Image sourceImage;
        private Boolean turnOnEvents;
        private Int32 projectedGifSize;
        private FileInfo sourceFileInfo;

        private ColorModel activeColorModel;
        private IColorCache activeColorCache;
        private IColorQuantizer activeQuantizer;

        private List<ColorModel> colorModelList;
        private List<IColorCache> colorCacheList;
        private List<IColorQuantizer> quantizerList;
        private Dictionary<Color, Int64> errorCache;

        #endregion

        #region | Constructors |

        /// <summary>
        /// Initializes a new instance of the <see cref="MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        #endregion

        #region | Update methods |

        private void UpdateImages()
        {
            // prepares quantizer
            errorCache.Clear();

            // updates source image
            UpdateSourceImage();

            // tries to retrieve an image based on HSB quantization

            try
            {
                DateTime before = DateTime.Now;
                Image targetImage = GetQuantizedImage(sourceImage);
                TimeSpan duration = DateTime.Now - before;
                TimeSpan perPixel = new TimeSpan(duration.Ticks/(sourceImage.Width*sourceImage.Height));
                pictureTarget.Image = targetImage;

                // new GIF and PNG sizes
                Int32 newGifSize, newPngSize;

                // retrieves a GIF image based on our HSB-quantized one
                GetConvertedImage(targetImage, ImageFormat.Gif, out newGifSize);

                // retrieves a PNG image based on our HSB-quantized one
                GetConvertedImage(targetImage, ImageFormat.Png, out newPngSize);

                // spits out the statistics
                Text = string.Format("Simple palette quantizer (duration 0:{0:00}.{1:0000000}, per pixel 0.{2:0000000})", duration.Seconds, duration.Ticks, perPixel.Ticks);
                editProjectedGifSize.Text = projectedGifSize.ToString();
                editProjectedPngSize.Text = sourceFileInfo.Length.ToString();
                editNewGifSize.Text = newGifSize.ToString();
                editNewPngSize.Text = newPngSize.ToString();
            }
            catch (NotSupportedException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateSourceImage()
        {
            switch (listSource.SelectedIndex)
            {
                case 0:
                    pictureSource.Image = sourceImage;
                    break;

                case 1:
                    pictureSource.Image = gifImage;
                    break;

                default:
                    throw new NotSupportedException("Not expected!");
            }
        }

        #endregion

        #region | Functions |

        private static PixelFormat GetFormatByColorCount(Int32 colorCount)
        {
            if (colorCount <= 0 || colorCount > 256)
            {
                String message = string.Format("A color count '{0}' not supported!", colorCount);
                throw new NotSupportedException(message);
            }

            PixelFormat result = PixelFormat.Format1bppIndexed;

            if (colorCount > 16)
            {
                result = PixelFormat.Format8bppIndexed;
            }
            else if (colorCount > 2)
            {
                result = PixelFormat.Format4bppIndexed;
            }

            return result;
        }

        private Int32 GetColorCount()
        {
            switch (listColors.SelectedIndex)
            {
                case 0: return 2;
                case 1: return 4;
                case 2: return 8;
                case 3: return 16;
                case 4: return 32;
                case 5: return 64;
                case 6: return 128;
                case 7: return 256;

                default:
                    throw new NotSupportedException("Only 2, 4, 8, 16, 32, 64, 128 and 256 colors are supported.");
            }
        }

        #endregion

        #region | Methods |

        private void ChangeQuantizer()
        {
            activeQuantizer = quantizerList[listMethod.SelectedIndex];

            // turns off the color option for the uniform quantizer, as it doesn't make sense
            listColors.Enabled = turnOnEvents && listMethod.SelectedIndex != 1 && listMethod.SelectedIndex != 6;

            // enables the color cache option; where available
            listColorCache.Enabled = turnOnEvents && activeQuantizer is BaseColorQuantizer;
            listColorModel.Enabled = listColorCache.Enabled && turnOnEvents && activeColorCache is BaseColorCache && ((BaseColorCache)activeColorCache).IsColorModelSupported;

            // applies current UI selection
            if (activeQuantizer is BaseColorQuantizer)
            {
                BaseColorQuantizer quantizer = (BaseColorQuantizer)activeQuantizer;
                quantizer.ChangeCacheProvider(activeColorCache);
            }

            if (listMethod.SelectedIndex == 1 ||
                listMethod.SelectedIndex == 6)
            {
                turnOnEvents = false;
                listColors.SelectedIndex = 7;
                turnOnEvents = true;
            }
        }

        private void ChangeColorCache()
        {
            activeColorCache = colorCacheList[listColorCache.SelectedIndex];

            // enables the color model option; where available
            listColorModel.Enabled = turnOnEvents && activeColorCache is BaseColorCache && ((BaseColorCache)activeColorCache).IsColorModelSupported;

            // applies current UI selection
            if (activeQuantizer is BaseColorQuantizer)
            {
                BaseColorQuantizer quantizer = (BaseColorQuantizer) activeQuantizer;
                quantizer.ChangeCacheProvider(activeColorCache);
            }

            // applies current UI selection
            if (activeColorCache is BaseColorCache)
            {
                BaseColorCache colorCache = (BaseColorCache)activeColorCache;
                colorCache.ChangeColorModel(activeColorModel);
            }
        }

        private void ChangeColorModel()
        {
            activeColorModel = colorModelList[listColorModel.SelectedIndex];

            // applies current UI selection
            if (activeColorCache is BaseColorCache)
            {
                BaseColorCache  colorCache = (BaseColorCache) activeColorCache;
                colorCache.ChangeColorModel(activeColorModel);
            }
        }

        private void EnableChoices()
        {
            Boolean allowColors = listMethod.SelectedIndex != 1 && listMethod.SelectedIndex != 6;

            checkShowError.Enabled = true;
            listSource.Enabled = true;
            listMethod.Enabled = true;
            listColorCache.Enabled = activeQuantizer is BaseColorQuantizer;
            listColorModel.Enabled = activeColorCache is BaseColorCache && allowColors;
            listColors.Enabled = allowColors;
        }

        private void GenerateProjectedGif()
        {
            // retrieves a projected GIF image (automatic C# conversion)
            Int32 projectedSize;
            gifImage = GetConvertedImage(sourceImage, ImageFormat.Gif, out projectedSize);
            projectedGifSize = projectedSize;
        }

        private static Image GetConvertedImage(Image image, ImageFormat newFormat, out Int32 imageSize)
        {
            Image result;

            // saves the image to the stream, and then reloads it as a new image format; thus conversion.. kind of
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, newFormat);
                stream.Seek(0, SeekOrigin.Begin);
                imageSize = (Int32)stream.Length;
                result = Image.FromStream(stream);
            }

            return result;
        }

        private Image GetQuantizedImage(Image image)
        {
            // checks whether a source image is valid
            if (image == null)
            {
                const String message = "Cannot quantize a null image.";
                throw new ArgumentNullException(message);
            }

            // scans the image for pixel colors, and adds them to a quantizer
            Int32 colorCount = GetColorCount();
            activeQuantizer.Prepare(sourceImage);
            image.AddColorsToQuantizer(activeQuantizer);
            Int32 originalColorCount = activeQuantizer.GetColorCount();

            // creates a target bitmap in 8-bit format
            Boolean isSourceIndexed = image.PixelFormat.IsIndexed();
            PixelFormat targetPixelFormat = GetFormatByColorCount(colorCount);
            Bitmap result = new Bitmap(image.Width, image.Height, targetPixelFormat);

            Int64 imageColorError = 0;
            List<Color> sourcePalette = isSourceIndexed ? image.GetPalette() : null;
            List<Color> palette = activeQuantizer.GetPalette(colorCount);
            result.SetPalette(palette);

            // moves to next pixel for both images
            Action<Pixel, Pixel> quantization = (sourcePixel, targetPixel) =>
            {
                Color color = isSourceIndexed ? sourcePalette[sourcePixel.GetIndex()] : sourcePixel.Color;
                color = QuantizationHelper.ConvertAlpha(color);
                Int32 paletteIndex = activeQuantizer.GetPaletteIndex(color);
                targetPixel.SetIndex((Byte)paletteIndex);

                // if turned on, it calculates root mean square deviation
                if (checkShowError.Checked)
                {
                    Int64 pixelError;

                    if (!errorCache.TryGetValue(color, out pixelError))
                    {
                        pixelError = ColorModelHelper.GetColorEuclideanDistance(ColorModel.RedGreenBlue, color, palette[paletteIndex]);
                        errorCache[color] = pixelError;
                    }

                    imageColorError += pixelError;
                }
            };

            // processes the quantization
            image.ProcessImagePixels(result, quantization);

            // calculates root mean square deviation
            Double nrmsd = !checkShowError.Checked ? 0.0 : Math.Sqrt(imageColorError / (3.0 * result.Width * result.Height)) / 255.0;
            String nrmsdString = checkShowError.Checked ? string.Format(" (NRMSD = {0:0.#####})", nrmsd) : string.Empty;

            // spits some duration statistics (those actually slow the processing quite a bit, turn them off to make it quicker)
            editSourceInfo.Text = string.Format("Original: {0} colors ({1} x {2})", originalColorCount, image.Width, image.Height);
            editTargetInfo.Text = string.Format("Quantized: {0} colors{1}", palette.Count, nrmsdString);

            // returns the quantized image
            return result;
        }

        #endregion

        #region << Events >>

        private void MainFormLoad(object sender, EventArgs e)
        {
            errorCache = new Dictionary<Color, Int64>();

            quantizerList = new List<IColorQuantizer>
            {
                new CompetitionQuantizer(),
                new UniformQuantizer(),
                new PopularityQuantizer(),
                new MedianCutQuantizer(),
                new OctreeQuantizer(),
                new WuColorQuantizer(),
                new NeuralColorQuantizer()
            };

            colorCacheList = new List<IColorCache>
            {
                new EuclideanDistanceColorCache(),
                new LshColorCache(),
                new OctreeColorCache()
            };

            colorModelList = new List<ColorModel>
            {
                ColorModel.RedGreenBlue,
                ColorModel.LabColorSpace
            };

            turnOnEvents = false;
            
            listSource.SelectedIndex = 0;
            listMethod.SelectedIndex = 0;
            listColors.SelectedIndex = 7;
            listColorCache.SelectedIndex = 0;
            listColorModel.SelectedIndex = 0;

            ChangeQuantizer();
            ChangeColorCache();
            ChangeColorModel();

            turnOnEvents = true;
        }

        private void MainFormResize(object sender, EventArgs e)
        {
            panelRight.Width = panelMain.Width / 2;
        }

        private void ButtonBrowseClick(object sender, EventArgs e)
        {
            if (dialogOpenFile.ShowDialog() == DialogResult.OK)
            {
                editFilename.Text = Path.GetFileName(dialogOpenFile.FileName);
                sourceFileInfo = new FileInfo(dialogOpenFile.FileName);
                sourceImage = Image.FromFile(dialogOpenFile.FileName);
                GenerateProjectedGif();
                EnableChoices();
                UpdateImages();
            }
        }

        private void ListSourceSelectedIndexChanged(object sender, EventArgs e)
        {
            if (turnOnEvents) UpdateSourceImage();
        }

        private void ListMethodSelectedIndexChanged(object sender, EventArgs e)
        {
            if (turnOnEvents)
            {
                ChangeQuantizer();
                UpdateImages();
            }
        }

        private void ListColorsSelectedIndexChanged(object sender, EventArgs e)
        {
            if (turnOnEvents) UpdateImages();
        }

        private void CheckShowErrorCheckedChanged(object sender, EventArgs e)
        {
            UpdateImages();
        }

        private void ListColorCacheSelectedIndexChanged(object sender, EventArgs e)
        {
            if (turnOnEvents)
            {
                ChangeColorCache();
                UpdateImages();
            }
        }

        private void ListColorModelSelectedIndexChanged(object sender, EventArgs e)
        {
            if (turnOnEvents)
            {
                ChangeColorModel();
                UpdateImages();
            }
        }

        #endregion
    }
}
