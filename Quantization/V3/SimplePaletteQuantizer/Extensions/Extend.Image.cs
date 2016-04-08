using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using SimplePaletteQuantizer.Helpers;
using SimplePaletteQuantizer.Quantizers;

namespace SimplePaletteQuantizer.Extensions
{
    /// <summary>
    /// The utility extender class.
    /// </summary>
    public static partial class Extend
    {
        /// <summary>
        /// Locks the image data in a given access mode.
        /// </summary>
        /// <param name="image">The source image containing the data.</param>
        /// <param name="lockMode">The lock mode (see <see cref="ImageLockMode"/> for more details).</param>
        /// <returns>The locked image data reference.</returns>
        public static BitmapData LockBits(this Image image, ImageLockMode lockMode)
        {
            // checks whether a source image is valid
            if (image == null)
            {
                const String message = "Cannot lock the bits for a null image.";
                throw new ArgumentNullException(message);
            }

            // determines the bounds of an image, and locks the data in a specified mode
            Bitmap bitmap = (Bitmap)image;
            Rectangle bounds = Rectangle.FromLTRB(0, 0, image.Width, image.Height);
            BitmapData result = bitmap.LockBits(bounds, lockMode, image.PixelFormat);
            return result;
        }

        /// <summary>
        /// Unlocks the data for a given image.
        /// </summary>
        /// <param name="image">The image containing the data.</param>
        /// <param name="data">The data belonging to the image.</param>
        public static void UnlockBits(this Image image, BitmapData data)
        {
            // checks whether a source image is valid
            if (image == null)
            {
                const String message = "Cannot unlock the bits for a null image.";
                throw new ArgumentNullException(message);
            }

            // checks if data to be unlocked are valid
            if (data == null)
            {
                const String message = "Cannot unlock null image data.";
                throw new ArgumentNullException(message);
            }

            // releases a lock
            Bitmap bitmap = (Bitmap)image;
            bitmap.UnlockBits(data);
        }

        /// <summary>
        /// Enumerates the image pixels colors.
        /// </summary>
        /// <param name="image">The source image to be enumerated.</param>
        /// <returns>The traversable enumeration of the image colors.</returns>
        public static IEnumerable<Color> EnumerateImageColors(this Image image)
        {
            // checks whether a source image is valid
            if (image == null)
            {
                const String message = "Cannot enumerate the pixels for a null image.";
                throw new ArgumentNullException(message);
            }
            
            // determines whether the format is indexed
            Boolean isFormatIndexed = image.PixelFormat.IsIndexed();

            // enumerates all the image's pixel colors
            foreach (Pixel pixel in image.EnumerateImagePixels(ImageLockMode.ReadOnly))
            {
                Color color = isFormatIndexed ? image.Palette.Entries[pixel.Index] : pixel.Color;
                yield return color;
            }
        }

        /// <summary>
        /// Enumerates the image pixels.
        /// </summary>
        /// <param name="image">The source image to be enumerated.</param>
        /// <param name="accessMode">The lock mode (see <see cref="ImageLockMode"/> for more details).</param>
        /// <returns>The traversable enumeration of the image pixels.</returns>
        public static IEnumerable<Pixel> EnumerateImagePixels(this Image image, ImageLockMode accessMode)
        {
            // checks whether a source image is valid
            if (image == null)
            {
                const String message = "Cannot enumerate the pixels for a null image.";
                throw new ArgumentNullException(message);
            }

            // locks the image data
            BitmapData data = image.LockBits(accessMode);
            PixelFormat pixelFormat = image.PixelFormat;

            try
            {
                // calculates all the values necessary for enumeration
                Byte bitDepth = pixelFormat.GetBitDepth();
                Int32 bitLength = image.Width*bitDepth;
                Int32 byteLength = data.Stride < 0 ? -data.Stride : data.Stride;
                Int32 byteCount = Math.Max(1, bitDepth >> 3);

                // initializes the transfer buffers, and current pixel offset
                Byte[] buffer = new Byte[byteLength];
                Byte[] value = new Byte[byteCount];
                Int64 offset = data.Scan0.ToInt64();

                // enumerates the pixels row by row
                for (Int32 row = 0; row < image.Height; row++)
                {
                    // aquires the pointer to the first row pixel
                    IntPtr offsetPointer = new IntPtr(offset);
                    Int32 column = 0;

                    // if a read operation is possible, reads the row buffer from the image at current offset
                    if (accessMode == ImageLockMode.ReadOnly || accessMode == ImageLockMode.ReadWrite)
                    {
                        // copies the row image data to the transfer buffer 
                        Marshal.Copy(offsetPointer, buffer, 0, byteLength);
                    }

                    // enumerates the buffer per pixel
                    for (Int32 index = 0; index < bitLength; index += bitDepth)
                    {
                        // when read is allowed, retrieves current value (in bytes)
                        if (accessMode == ImageLockMode.ReadOnly || accessMode == ImageLockMode.ReadWrite)
                        {
                            Array.Copy(buffer, index >> 3, value, 0, byteCount);
                        }

                        // enumerates the pixel, and returns the control to the outside
                        Pixel pixel = new Pixel(value, column++, row, index % 8, pixelFormat);
                        yield return pixel;

                        // when write is allowed, copies the value back to the row buffer
                        if (accessMode == ImageLockMode.WriteOnly || accessMode == ImageLockMode.ReadWrite)
                        {
                            Array.Copy(value, 0, buffer, index >> 3, byteCount);
                        }
                    }

                    // if a write operation is possible, writes the row buffer back to current offset
                    if (accessMode == ImageLockMode.WriteOnly || accessMode == ImageLockMode.ReadWrite)
                    {
                        // copies the row image data from the buffer back to the image
                        Marshal.Copy(buffer, 0, offsetPointer, byteLength);
                    }

                    // increases offset by a row
                    offset += data.Stride;
                }
            }
            finally
            {
                // releases the lock on the image data
                image.UnlockBits(data);
            }
        }

        /// <summary>
        /// Adds all the colors from a source image to a given color quantizer.
        /// </summary>
        /// <param name="image">The image to be processed.</param>
        /// <param name="quantizer">The target color quantizer.</param>
        public static void AddColorsToQuantizer(this Image image, IColorQuantizer quantizer)
        {
            // checks whether a source image is valid
            if (image == null)
            {
                const String message = "Cannot add colors from a null image.";
                throw new ArgumentNullException(message);
            }

            // checks whether the quantizer is valid
            if (quantizer == null)
            {
                const String message = "Cannot add colors to a null quantizer.";
                throw new ArgumentNullException(message);
            }

            // determines which method of color retrieval to use
            Boolean isImageIndexed = image.PixelFormat.IsIndexed();

            // retrieves all the colors from image into a given quantizer
            foreach (Pixel pixel in image.EnumerateImagePixels(ImageLockMode.ReadOnly))
            {
                // determines a pixel color
                Color color = isImageIndexed ? image.Palette.Entries[pixel.Index] : pixel.Color;

                // adds the color to the quantizer
                quantizer.AddColor(color);
            }
        }

        /// <summary>
        /// Changes the pixel format.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="targetFormat">The target format.</param>
        /// <param name="quantizer">The color quantizer.</param>
        /// <returns>The converted image in a target format.</returns>
        public static Image ChangePixelFormat(this Image image, PixelFormat targetFormat, IColorQuantizer quantizer)
        {
            // checks for image validity
            if (image == null)
            {
                const String message = "Cannot change a pixel format for a null image.";
                throw new ArgumentNullException(message);
            }

            // checks whether a target format is supported
            if (!targetFormat.IsSupported())
            {
                String message = string.Format("A pixel format '{0}' is not supported.", targetFormat);
                throw new NotSupportedException(message);
            }

            // checks whether there is a quantizer for a indexed format
            if (targetFormat.IsIndexed() && quantizer == null)
            {
                String message = string.Format("A quantizer is cannot be null for indexed pixel format '{0}'.", targetFormat);
                throw new NotSupportedException(message);
            }

            // creates an image with the target format
            Bitmap result = new Bitmap(image.Width, image.Height, targetFormat);

            // gathers some information about the target format
            Boolean hasSourceAlpha = image.PixelFormat.HasAlpha();
            Boolean hasTargetAlpha = targetFormat.HasAlpha();
            Boolean isSourceIndexed = image.PixelFormat.IsIndexed();
            Boolean isTargetIndexed = targetFormat.IsIndexed();
            Boolean isSourceDeepColor = image.PixelFormat.IsDeepColor();
            Boolean isTargetDeepColor = targetFormat.IsDeepColor();

            // if palette is needed create one first
            if (isTargetIndexed)
            {
                Int32 targetColorCount = result.GetPaletteColorCount();
                List<Color> palette = quantizer.GetPalette(targetColorCount);
                result.SetPalette(palette);
            }

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


                    // if both source and target formats are deep color formats, copies a value directly
                    if (isSourceDeepColor && isTargetDeepColor)
                    {
                        UInt64 value = source.Current.Value;
                        target.Current.SetValue(value);
                    }
                    else
                    {
                        // retrieves a source image color
                        if (isSourceIndexed)
                        {
                            // for the indexed images, retrieves a color from their palette
                            color = image.Palette.Entries[source.Current.Index];
                        }
                        else
                        {
                            // for the non-indexed image, retrieves a color directly
                            color = source.Current.Color;
                        }

                        // if alpha is not present in the source image, but is present in the target, make one up
                        if (!hasSourceAlpha && hasTargetAlpha)
                        {
                            color = Color.FromArgb(255, color.R, color.G, color.B);
                        }

                        // sets the color to a target pixel
                        if (isTargetIndexed)
                        {
                            // for the indexed images, determines a color from the octree
                            Byte paletteIndex = (Byte) quantizer.GetPaletteIndex(color);
                            target.Current.SetIndex(paletteIndex);
                        }
                        else
                        {
                            // for the non-indexed images, sets the color directly
                            target.Current.SetColor(color);
                        }
                    }

                    isSourceAvailable = source.MoveNext();
                    isTargetAvailable = target.MoveNext();
                }
            }

            // returns the image in the target format
            return result;
        }

        public static Int32 GetPaletteColorCount(this Image image)
        {
            // checks whether a source image is valid
            if (image == null)
            {
                const String message = "Cannot assign a palette to a null image.";
                throw new ArgumentNullException(message);
            }

            // checks if the image has an indexed format
            if (!image.PixelFormat.IsIndexed())
            {
                String message = string.Format("Cannot retrieve a color count from a non-indexed image with pixel format '{0}'.", image.PixelFormat);
                throw new InvalidOperationException(message);
            }

            // returns the color count
            return image.Palette.Entries.Length;
        }

        /// <summary>
        /// Gets the palette of an indexed image.
        /// </summary>
        /// <param name="image">The source image.</param>
        public static List<Color> GetPalette(this Image image)
        {
            // checks whether a source image is valid
            if (image == null)
            {
                const String message = "Cannot assign a palette to a null image.";
                throw new ArgumentNullException(message);
            }

            // checks if the image has an indexed format
            if (!image.PixelFormat.IsIndexed())
            {
                String message = string.Format("Cannot retrieve a palette from a non-indexed image with pixel format '{0}'.", image.PixelFormat);
                throw new InvalidOperationException(message);
            }

            // retrieves and returns the palette
            return image.Palette.Entries.ToList();
        }

        /// <summary>
        /// Sets the palette of an indexed image.
        /// </summary>
        /// <param name="image">The target image.</param>
        /// <param name="palette">The palette.</param>
        public static void SetPalette(this Image image, List<Color> palette)
        {
            // checks whether a palette is valid
            if (palette == null)
            {
                const String message = "Cannot assign a null palette.";
                throw new ArgumentNullException(message);
            }

            // checks whether a target image is valid
            if (image == null)
            {
                const String message = "Cannot assign a palette to a null image.";
                throw new ArgumentNullException(message);
            }

            // checks if the image has indexed format
            if (!image.PixelFormat.IsIndexed())
            {
                String message = string.Format("Cannot store a palette to a non-indexed image with pixel format '{0}'.", image.PixelFormat);
                throw new InvalidOperationException(message);
            }
            
            // checks if the palette can fit into the image palette
            if (palette.Count > image.Palette.Entries.Length)
            {
                String message = string.Format("Cannot store a palette with '{0}' colors intto an image palette where only '{1}' colors are allowed.", palette.Count, image.Palette.Entries.Length);
                throw new ArgumentOutOfRangeException(message);
            }

            // retrieves a target image palette
            ColorPalette imagePalette = image.Palette;

            // copies all color entries
            for (Int32 index = 0; index < palette.Count; index++)
            {
                imagePalette.Entries[index] = palette[index];
            }

            // assigns the palette to the target image
            image.Palette = imagePalette;
        }
    }
}