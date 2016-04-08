using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using SimplePaletteQuantizer.ColorCaches;
using SimplePaletteQuantizer.ColorCaches.Octree;
using SimplePaletteQuantizer.Extensions;
using SimplePaletteQuantizer.Helpers;

namespace SimplePaletteQuantizer.Quantizers.HSL
{
    /// <summary>
    /// This is my baby. Read more in the article on the Code Project:
    /// http://www.codeproject.com/KB/recipes/SimplePaletteQuantizer.aspx
    /// </summary>
    public class CompetitionQuantizer : BaseColorQuantizer
    {
        #region | Constants |

        private const float Epsilon = 1E-05f;

        #endregion

        #region | Fields |

        private List<Color> palette;
        private Dictionary<Int32, ColorInfo> colorMap;

        #endregion

        #region << IColorQuantizer >>

        /// <summary>
        /// Clears the color map.
        /// </summary>
        public override void Prepare(Image image)
        {
            base.Prepare(image);
            palette = new List<Color>();
            colorMap = new Dictionary<Int32, ColorInfo>();
        }

        /// <summary>
        /// Adds the color to quantizer, only unique colors are added.
        /// </summary>
        /// <param name="color">The color to be added.</param>
        public override void AddColor(Color color)
        {
            // if alpha is higher then fully transparent, convert it to a RGB value for more precise processing
            color = QuantizationHelper.ConvertAlpha(color);
            Int32 key = color.R << 16 | color.G << 8 | color.B;
            ColorInfo colorInfo;

            if (colorMap.TryGetValue(key, out colorInfo))
            {
                colorInfo.IncreaseCount();
            }
            else
            {
                colorInfo = new ColorInfo(color);
                colorMap.Add(key, colorInfo);
            }
        }

        /// <summary>
        /// Called when it is needed to create default cache (no cache is supplied from outside).
        /// </summary>
        /// <returns></returns>
        protected override IColorCache OnCreateDefaultCache()
        {
            // use OctreeColorCache best performance/quality
            return new OctreeColorCache();
        }

        /// <summary>
        /// Gets the palette with a specified count of the colors.
        /// </summary>
        /// <param name="colorCount">The color count.</param>
        /// <returns></returns>
        protected override List<Color> OnGetPalette(Int32 colorCount)
        {
            palette.Clear();

            // lucky seed :)
            Random random = new Random(13);

            // shuffles the colormap
            List<ColorInfo> colorInfoList = colorMap.Values.
                OrderBy(entry => random.NextDouble()).
                ToList();

            // workaround for backgrounds, the most prevalent color
            ColorInfo background = colorInfoList.MaxBy(info => info.Count);
            colorCount--;

            ColorHueComparer hueComparer = new ColorHueComparer();
            ColorSaturationComparer saturationComparer = new ColorSaturationComparer();
            ColorBrightnessComparer brightnessComparer = new ColorBrightnessComparer();

            // generates catalogue
            List<IEqualityComparer<ColorInfo>> comparers = new List<IEqualityComparer<ColorInfo>> { hueComparer, saturationComparer, brightnessComparer };

            // take adequate number from each slot
            while (ProcessList(colorCount, colorInfoList, comparers, out colorInfoList)) { }

            Int32 listColorCount = colorInfoList.Count();

            if (listColorCount > 0)
            {
                Int32 allowedTake = Math.Min(colorCount, listColorCount);
                colorInfoList = colorInfoList.Take(allowedTake).ToList();
            }

            // adds the selected colors to a final palette
            palette.Add(background.Color);
            palette.AddRange(colorInfoList.Select(colorInfo => colorInfo.Color));

            // returns our new palette
            return palette;
        }

        private static Boolean ProcessList(Int32 colorCount, List<ColorInfo> list, List<IEqualityComparer<ColorInfo>> comparers, out List<ColorInfo> outputList)
        {
            IEqualityComparer<ColorInfo> bestComparer = null;
            Int32 maximalCount = 0;
            outputList = list;

            foreach (IEqualityComparer<ColorInfo> comparer in comparers)
            {
                List<ColorInfo> filteredList = list.
                    Distinct(comparer).
                    GroupBy(entry => entry, comparer).
                    First().
                    ToList();

                Int32 filteredListCount = filteredList.Count;

                if (filteredListCount > colorCount && filteredListCount > maximalCount)
                {
                    maximalCount = filteredListCount;
                    bestComparer = comparer;
                    outputList = filteredList;
                }
            }

            comparers.Remove(bestComparer);
            return comparers.Count > 0 && maximalCount > colorCount;
        }

        /// <summary>
        /// Gets the color count.
        /// </summary>
        /// <returns></returns>
        public override Int32 GetColorCount()
        {
            return colorMap.Count;
        }

        #endregion

        #region | Helper classes (comparers) |

        /// <summary>
        /// Compares a hue components of a color info.
        /// </summary>
        private class ColorHueComparer : IEqualityComparer<ColorInfo>
        {
            public Boolean Equals(ColorInfo x, ColorInfo y)
            {
                return Math.Abs(x.Hue - y.Hue) < Epsilon;
            }

            public Int32 GetHashCode(ColorInfo color)
            {
                return color.HueHashCode;
            }
        }

        /// <summary>
        /// Compares a saturation components of a color info.
        /// </summary>
        private class ColorSaturationComparer : IEqualityComparer<ColorInfo>
        {
            public Boolean Equals(ColorInfo x, ColorInfo y)
            {
                return Math.Abs(x.Saturation - y.Saturation) < Epsilon;
            }

            public Int32 GetHashCode(ColorInfo color)
            {
                return color.SaturationHashCode;
            }
        }

        /// <summary>
        /// Compares a brightness components of a color info.
        /// </summary>
        private class ColorBrightnessComparer : IEqualityComparer<ColorInfo>
        {
            public Boolean Equals(ColorInfo x, ColorInfo y)
            {
                return Math.Abs(x.Brightness - y.Brightness) < Epsilon;
            }

            public Int32 GetHashCode(ColorInfo color)
            {
                return color.BrightnessHashCode;
            }
        }

        #endregion
    }
}


