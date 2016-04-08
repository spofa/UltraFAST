using System;
using System.Collections.Generic;
using System.Drawing;

namespace SimplePaletteQuantizer.Helpers
{
    public class QuantizationHelper
    {
        private static readonly Color BackgroundColor;
        private static readonly Double[] Factors;

        static QuantizationHelper()
        {
            BackgroundColor = SystemColors.Control;
            Factors = PrecalculateFactors();
        }

        /// <summary>
        /// Precalculates the alpha-fix values for all the possible alpha values (0-255).
        /// </summary>
        private static Double[] PrecalculateFactors()
        {
            Double[] result = new Double[256];

            for (Int32 value = 0; value < 256; value++)
            {
                result[value] = value / 255.0;
            }

            return result;
        }

        /// <summary>
        /// Converts the alpha blended color to a non-alpha blended color.
        /// </summary>
        /// <param name="color">The alpha blended color (ARGB).</param>
        /// <returns>The non-alpha blended color (RGB).</returns>
        internal static Color ConvertAlpha(Color color)
        {
            Color result = color;

            if (color.A < 255)
            {
                // performs a alpha blending (second color is BackgroundColor, by default a Control color)
                Double colorFactor = Factors[color.A];
                Double backgroundFactor = Factors[255 - color.A];
                Int32 red = (Int32) (color.R*colorFactor + BackgroundColor.R*backgroundFactor);
                Int32 green = (Int32) (color.G*colorFactor + BackgroundColor.G*backgroundFactor);
                Int32 blue = (Int32) (color.B*colorFactor + BackgroundColor.B*backgroundFactor);
                result = Color.FromArgb(255, red, green, blue);
            }

            return result;
        }

        /// <summary>
        /// Finds the closest color match in a given palette using Euclidean distance.
        /// </summary>
        /// <param name="color">The color to be matched.</param>
        /// <param name="palette">The palette to search in.</param>
        /// <returns>The palette index of the closest match.</returns>
        internal static Int32 GetNearestColor(Color color, IList<Color> palette)
        {
            // initializes the best difference, set it for worst possible, it can only get better
            Int32 bestIndex = 0;
            Int32 leastDistance = Int32.MaxValue;

            // goes thru all the colors in the palette, looking for the best match
            for (Int32 index = 0; index < palette.Count; index++)
            {
                Color targetColor = palette[index];
                Int32 distance = GetColorEuclideanDistanceInRGB(palette.Count, color, targetColor);

                // if a difference is zero, we're good because it won't get better
                if (distance == 0)
                {
                    bestIndex = index;
                    break;
                }

                // if a difference is the best so far, stores it as our best candidate
                if (distance < leastDistance)
                {
                    leastDistance = distance;
                    bestIndex = index;
                }
            }

            // returns the palette index of the most similar color
            return bestIndex;
        }

        ///// <summary>
        ///// Gets the color euclidean distance.
        ///// </summary>
        ///// <param name="requestedColor">Color of the requested.</param>
        ///// <param name="realColor">Color of the real.</param>
        ///// <returns></returns>
        public static Int32 GetColorEuclideanDistanceInRGB(Int32 count, Color requestedColor, Color realColor)
        {
            // calculates a difference for all the color components
            Int32 redDelta = Math.Abs(requestedColor.R - realColor.R);
            Int32 greenDelta = Math.Abs(requestedColor.G - realColor.G);
            Int32 blueDelta = Math.Abs(requestedColor.B - realColor.B);

            Int32 redFactor = redDelta * redDelta;
            Int32 greenFactor = greenDelta * greenDelta;
            Int32 blueFactor = blueDelta * blueDelta;

            // calculates the Euclidean distance, a square-root is not need 
            // as we're only comparing distance, not measuring it
            return redFactor + greenFactor + blueFactor;
        }

        ///// <summary>
        ///// Gets the color euclidean distance.
        ///// </summary>
        ///// <param name="count"></param>
        ///// <param name="requestedColor">Color of the requested.</param>
        ///// <param name="realColor">Color of the real.</param>
        ///// <returns></returns>
        //public static Int32 GetColorEuclideanDistanceInRGB(Int32 count, Color requestedColor, Color realColor)
        //{
        //    Int32 result;

        //    // calculates a difference for all the color components
        //    Int32 redDelta = requestedColor.R - realColor.R;
        //    Int32 greenDelta = requestedColor.G - realColor.G;
        //    Int32 blueDelta = requestedColor.B - realColor.B;

        //    if (redDelta < 0) redDelta = -redDelta;
        //    if (greenDelta < 0) greenDelta = -greenDelta;
        //    if (blueDelta < 0) blueDelta = -blueDelta;

        //    // calculates a power of two)
        //    if (count <= 8)
        //    {
        //        Int32 redFactor = redDelta * redDelta;
        //        Int32 greenFactor = greenDelta * greenDelta;
        //        Int32 blueFactor = blueDelta * blueDelta;
        //        result = redFactor + greenFactor + blueFactor;
        //    }
        //    else if (count <= 32)
        //    {
        //        Int32 maxRedOrGreen = redDelta < greenDelta ? greenDelta : redDelta;
        //        Int32 maxBlueOrRest = blueDelta < maxRedOrGreen ? maxRedOrGreen : blueDelta;
        //        result = redDelta + greenDelta + blueDelta + maxBlueOrRest;
        //    }
        //    else
        //    {
        //        result = redDelta + greenDelta + blueDelta;
        //    }

        //    // calculates the Euclidean distance, a square-root is not need 
        //    // as we're only comparing distance, not measuring it
        //    return result;
        //}

        //public static Single GetColorEuclideanDistanceInHSB(Color requestedColor, Color realColor)
        //{
        //    // calculates a difference for all the color components
        //    Single hDelta = requestedColor.GetHue() - realColor.GetHue();
        //    Single sDelta = requestedColor.GetSaturation() - realColor.GetSaturation();
        //    Single bDelta = requestedColor.GetBrightness() - realColor.GetBrightness();

        //    // calculates a power of two
        //    Single hFactor = hDelta * hDelta;
        //    Single sFactor = sDelta * sDelta;
        //    Single bFactor = bDelta * bDelta;

        //    // calculates the Euclidean distance, a square-root is not need 
        //    // as we're only comparing distance, not measuring it
        //    return hFactor + sFactor + bFactor;
        //}
    }
}
