﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SimplePaletteQuantizer.ColorCaches;
using SimplePaletteQuantizer.ColorCaches.Octree;
using SimplePaletteQuantizer.Helpers;

namespace SimplePaletteQuantizer.Quantizers.Popularity
{
    /// <summary>
    /// Popularity algorithms are another form of uniform quantization. However, instead of 
    /// dividing the color space into 256 regions these algorithms break the color space into 
    /// much smaller, and consequently many more, regions. One possible implementation is to 
    /// divide the space into regions 4x4x4 in size (262,144 regions). The original colors are 
    /// again mapped to the region they fall in. The representative color for each region is the 
    /// average of the colors mapped to it. The color map is selected by taking the representative 
    /// colors of the 256 most popular regions (the regions that had the most colors mapped to them). 
    /// If a non-empty region is not selected for the color map its index into the color map (the 
    /// index that will be assigned to colors that map to that region) is then the entry in the color 
    /// map that is closest (Euclidean distance) to its representative color). 
    ///
    /// These algorithms are also easy to implement and yield better results than the uniform 
    /// quantization algorithm. They do, however, take slightly longer to execute and can have a 
    /// significantly larger storage requirement depending on the size of regions. Also depending 
    /// on the image characteristics this may not produce a good result. This can be said about all 
    /// uniform sub-division schemes, because the method for dividing the color space does utilize 
    /// any information about the image.
    /// </summary>
    public class PopularityQuantizer : BaseColorQuantizer
    {
        #region | Fields |

        private List<Color> palette;
        private Dictionary<Int32, PopularityColorSlot> colorMap;

        #endregion

        #region | Methods |

        private static Int32 GetColorIndex(Color color)
        {
            // determines the index by splitting the RGB cube to 4x4x4 (1 >> 2 = 4)
            Int32 redIndex = color.R >> 2;
            Int32 greenIndex = color.G >> 2;
            Int32 blueIndex = color.B >> 2;

            // calculates the whole unique index of the slot: Index = R*4096 + G*64 + B
            return (redIndex << 12) + (greenIndex << 6) + blueIndex;
        }

        #endregion

        #region << IColorQuantizer >>

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public override void Prepare(Image image)
        {
            base.Prepare(image);
            palette = new List<Color>();
            colorMap = new Dictionary<Int32, PopularityColorSlot>();
        }

        /// <summary>
        /// Adds the color to quantizer.
        /// </summary>
        /// <param name="color">The color to be added.</param>
        public override void AddColor(Color color)
        {
            PopularityColorSlot slot;
            color = QuantizationHelper.ConvertAlpha(color);
            Int32 index = GetColorIndex(color);

            if (colorMap.TryGetValue(index, out slot))
            {
                slot.AddValue(color);
            }
            else
            {
                colorMap[index] = new PopularityColorSlot(color);
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
        /// Gets the palette with specified count of the colors.
        /// </summary>
        /// <param name="colorCount">The color count.</param>
        /// <returns></returns>
        protected override List<Color> OnGetPalette(Int32 colorCount)
        {
            Random random = new Random();

            // NOTE: I've added a little randomization here, as it was performing terribly otherwise.
            // sorts out the list by a pixel presence, takes top N slots, and calculates 
            // the average color from them, thus our new palette.
            IEnumerable<Color> colors = colorMap.
                 OrderBy(entry => random.NextDouble()).
                 OrderByDescending(entry => entry.Value.PixelCount).
                 Take(colorCount).
                 Select(entry => entry.Value.GetAverage());

            palette.Clear();
            palette.AddRange(colors);
            return palette;
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
    }
}
