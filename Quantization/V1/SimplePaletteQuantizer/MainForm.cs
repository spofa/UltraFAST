﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SimplePaletteQuantizer.Quantizers;

namespace SimplePaletteQuantizer
{
    public partial class MainForm : Form
    {
        private Image sourceImage;
        private IColorQuantizer quantizer;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            quantizer = new PaletteQuantizer();
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            if (dialogOpenFile.ShowDialog() == DialogResult.OK)
            {
                sourceImage = Image.FromFile(dialogOpenFile.FileName);
                UpdateImages();
            }
        }

        private void UpdateImages()
        {
            quantizer.Clear();
            pictureSource.Image = sourceImage;
            pictureTarget.Image = GetQuantizedImage(sourceImage);
        }

        private Image GetQuantizedImage(Image image)
        {
            // checks whether a source image is valid
            if (image == null)
            {
                const String message = "Cannot quantize a null image.";
                throw new ArgumentNullException(message);
            }

            // locks the source image data
            Bitmap bitmap = (Bitmap) image;
            Rectangle bounds = Rectangle.FromLTRB(0, 0, bitmap.Width, bitmap.Height);
            BitmapData sourceData = bitmap.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                Int32[] sourceBuffer = new Int32[image.Width];
                Int64 sourceOffset = sourceData.Scan0.ToInt64();

                for (Int32 row = 0; row < image.Height; row++)
                {
                    Marshal.Copy(new IntPtr(sourceOffset), sourceBuffer, 0, image.Width);

                    foreach (Color color in sourceBuffer.Select(argb => Color.FromArgb(argb)))
                    {
                        quantizer.AddColor(color);
                    }

                    // increases a source offset by a row
                    sourceOffset += sourceData.Stride;
                }
            }
            catch
            {
                bitmap.UnlockBits(sourceData);
                throw;
            }

            // calculates the palette
            Bitmap result = new Bitmap(image.Width, image.Height, PixelFormat.Format8bppIndexed);
            List<Color> palette = quantizer.GetPalette(256);
            ColorPalette imagePalette = result.Palette;

            textBox1.Text = string.Format("Original picture: {0} colors", quantizer.GetColorCount());
            textBox2.Text = string.Format("Quantized picture: {0} colors", palette.Count);

            for (Int32 index = 0; index < palette.Count; index++)
            {
                imagePalette.Entries[index] = palette[index];
            }

            result.Palette = imagePalette;

            // locks the target image data
            BitmapData targetData = result.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            try
            {
                Byte[] targetBuffer = new Byte[result.Width];
                Int32[] sourceBuffer = new Int32[image.Width];
                Int64 sourceOffset = sourceData.Scan0.ToInt64();
                Int64 targetOffset = targetData.Scan0.ToInt64();

                for (Int32 row = 0; row < image.Height; row++)
                {
                    Marshal.Copy(new IntPtr(sourceOffset), sourceBuffer, 0, image.Width);

                    for (Int32 index = 0; index < image.Width; index++)
                    {
                        Color color = Color.FromArgb(sourceBuffer[index]);
                        targetBuffer[index] = quantizer.GetPaletteIndex(color);
                    }

                    Marshal.Copy(targetBuffer, 0, new IntPtr(targetOffset), result.Width);

                    // increases the offsets by a row
                    sourceOffset += sourceData.Stride;
                    targetOffset += targetData.Stride;
                }
            }
            finally
            {
                // releases the locks on both images
                bitmap.UnlockBits(sourceData);
                result.UnlockBits(targetData);
            }

            return result;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            panelRight.Width = panelMain.Width/2;
        }
    }
}
