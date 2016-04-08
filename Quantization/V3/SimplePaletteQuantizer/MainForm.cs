using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using SimplePaletteQuantizer.Extensions;
using SimplePaletteQuantizer.Helpers;
using SimplePaletteQuantizer.Quantizers;
using SimplePaletteQuantizer.Quantizers.HSB;
using SimplePaletteQuantizer.Quantizers.Median;
using SimplePaletteQuantizer.Quantizers.Octree;
using SimplePaletteQuantizer.Quantizers.Popularity;
using SimplePaletteQuantizer.Quantizers.Uniform;

namespace SimplePaletteQuantizer
{
    public partial class MainForm : Form
    {
        private Image gifImage;
        private Image sourceImage;
        private Boolean turnOnEvents;
        private Int32 projectedGifSize;
        private FileInfo sourceFileInfo;
        private IColorQuantizer activeQuantizer;
        private List<IColorQuantizer> quantizerList;
        private Dictionary<Color, Single> errorCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        #region | Update methods |

        private void UpdateImages()
        {
            // prepares quantizer
            activeQuantizer.Clear();
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
            listColors.Enabled = turnOnEvents && listMethod.SelectedIndex != 1;

            if (listMethod.SelectedIndex == 1)
            {
                turnOnEvents = false;
                listColors.SelectedIndex = 7;
                turnOnEvents = true;
            }
        }

        private void EnableChoices()
        {
            listSource.Enabled = true;
            listMethod.Enabled = true;
            listColors.Enabled = listMethod.SelectedIndex != 1;
        }

        private void GenerateProjectedGif()
        {
            // retrieves a projected GIF image (automatic C# conversion)
            gifImage = GetConvertedImage(sourceImage, ImageFormat.Gif, out projectedGifSize);
        }

        private static Image GetConvertedImage(Image image, ImageFormat newFormat, out Int32 imageSize)
        {
            Image result;

            // saves the image to the stream, and then reloads it as a new image format; thus conversion.. kind of
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, newFormat);
                stream.Flush();
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
            image.AddColorsToQuantizer(activeQuantizer);
            Int32 originalColorCount = activeQuantizer.GetColorCount();

            // creates a target bitmap in 8-bit format
            Boolean isSourceIndexed = image.PixelFormat.IsIndexed();
            PixelFormat targetPixelFormat = GetFormatByColorCount(colorCount);
            Bitmap result = new Bitmap(image.Width, image.Height, targetPixelFormat);

            //Single imageColorError = 0;
            List<Color> sourcePalette = isSourceIndexed ? image.GetPalette() : null;
            List<Color> palette = activeQuantizer.GetPalette(colorCount);
            result.SetPalette(palette);

            // initializes both source and target image enumerators
            IEnumerable<Pixel> sourceEnum = image.EnumerateImagePixels(ImageLockMode.ReadOnly);
            IEnumerable<Pixel> targetEnum = result.EnumerateImagePixels(ImageLockMode.WriteOnly);

            // ensures that both enumerators are released from memory afterwards
            using (IEnumerator<Pixel> source = sourceEnum.GetEnumerator())
            using (IEnumerator<Pixel> target = targetEnum.GetEnumerator())
            {
                Boolean isSourceAvailable = source.MoveNext();
                Boolean isTargetAvailable = target.MoveNext();

                // moves to next pixel for both images
                // while (source.MoveNext() || target.MoveNext())
                while (isSourceAvailable || isTargetAvailable)
                {
                    Color color;

                    if (isSourceIndexed)
                    {
                        Byte colorIndex = source.Current.GetIndex();
                        color = sourcePalette[colorIndex];
                    }
                    else
                    {
                        color = source.Current.Color;
                    }

                    color = QuantizationHelper.ConvertAlpha(color);
                    Int32 paletteIndex = activeQuantizer.GetPaletteIndex(color);
                    target.Current.SetIndex((Byte) paletteIndex);

                    //Single pixelError;

                    //if (!errorCache.TryGetValue(color, out pixelError))
                    //{
                    //    pixelError = QuantizationHelper.GetColorEuclideanDistanceInRGB(color, palette[paletteIndex]);
                    //    errorCache[color] = pixelError;
                    //}

                    //imageColorError += pixelError;

                    isSourceAvailable = source.MoveNext();
                    isTargetAvailable = target.MoveNext();
                }
            }

            // calculates root mean square deviation
            Double nrmsd = 0.0; // Math.Sqrt(imageColorError / (3.0 * result.Width * result.Height)) / 255.0;

            // spits some duration statistics (those actually slow the processing quite a bit, turn them off to make it quicker)
            editSourceInfo.Text = string.Format("Original: {0} colors ({1} x {2})", originalColorCount, image.Width, image.Height);
            editTargetInfo.Text = string.Format("Quantized: {0} colors (NRMSD = {1:0.#####})", palette.Count, nrmsd);

            // returns the quantized image
            return result;
        }

        #endregion

        #region << Events >>

        private void MainFormLoad(object sender, EventArgs e)
        {
            errorCache = new Dictionary<Color, Single>();

            quantizerList = new List<IColorQuantizer>
            {
                new PaletteQuantizer(),
                new UniformQuantizer(),
                new PopularityQuantizer(),
                new MedianCutQuantizer(),
                new OctreeQuantizer()
            };

            turnOnEvents = false;
            
            listSource.SelectedIndex = 0;
            listMethod.SelectedIndex = 0;
            listColors.SelectedIndex = 7;

            ChangeQuantizer();

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

        #endregion
    }
}
