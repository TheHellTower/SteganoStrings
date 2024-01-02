using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace SteganoStrings
{
    public static class SteganographyHelper
    {
        public static Bitmap EncodeText(Stream imageStream, string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Text to be hidden cannot be null or empty.");

            Bitmap image = new Bitmap(imageStream);
            var textSize = text.Length * 8; // Assuming each character is represented by 8 bits
            var textSizeInKB = textSize / 1024;

            if (textSizeInKB > GetImageSizeInKB(image))
                throw new Exception("Image cannot save text more than " + GetImageSizeInKB(image) + " KB");

            HideTextInImage(image, text);

            return image;
        }

        private static void HideTextInImage(Bitmap image, string text)
        {
            int textLength = text.Length, charIndex = 0, width = image.Width, height = image.Height;
            BitmapData bmpData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, image.PixelFormat);

            try
            {
                IntPtr ptr = bmpData.Scan0;
                int bytes = Math.Abs(bmpData.Stride) * height;
                byte[] rgbValues = new byte[bytes];
                Marshal.Copy(ptr, rgbValues, 0, bytes);

                int pixelSize = Image.GetPixelFormatSize(image.PixelFormat) / 8; // Bytes per pixel

                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        int offset = (j * bmpData.Stride) + (i * pixelSize);
                        if (charIndex < textLength)
                        {
                            // Modify the blue component with the text data
                            char letter = text[charIndex];
                            int value = Convert.ToInt32(letter);
                            rgbValues[offset] = (byte)value;
                        }

                        if (i == width - 1 && j == height - 1)
                        {
                            // Set the last pixel to store the text length
                            rgbValues[offset] = (byte)textLength;
                        }

                        charIndex++;
                    }

                Marshal.Copy(rgbValues, 0, ptr, bytes);
            }
            finally
            {
                image.UnlockBits(bmpData);
            }
        }

        private static double GetImageSizeInKB(Bitmap image) => (image.Width * image.Height * 8) / 1024.0; // Assuming each pixel stores 8 bits
    }
}