using System;
using System.Drawing;
using System.IO;

namespace SteganoStrings
{
    //https://github.com/tank130701/Steganography/blob/master/Steganography/Program.cs
    public static class SteganographyHelper
    {
        public static Bitmap EncodeText(Stream imageStream, string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Text to be hidden cannot be null or empty.");

            Bitmap image = new Bitmap(imageStream);
            var textSize = text.Length * 8; // Assuming each character is represented by 16 bits
            var textSizeInKB = textSize / 1024;

            if (textSizeInKB > GetImageSizeInKB(image))
                throw new Exception("Image cannot save text more than " + GetImageSizeInKB(image) + " KB");

            HideTextInImage(image, text);

            return image;
        }

        private static void HideTextInImage(Bitmap image, string text)
        {
            int textLength = text.Length, charIndex = 0;

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color pixel = image.GetPixel(i, j);

                    if (charIndex < textLength)
                    {
                        char letter = text[charIndex];
                        int value = Convert.ToInt32(letter);
                        Color modifiedColor = ModifyPixelColor(value);
                        image.SetPixel(i, j, modifiedColor);
                    }

                    if (i == image.Width - 1 && j == image.Height - 1)
                        image.SetPixel(i, j, Color.FromArgb(pixel.R, pixel.G, textLength));

                    charIndex++;
                }
            }
        }

        private static Color ModifyPixelColor(int value)
        {
            // Extract the individual bytes from the integer value
            byte byte3 = (byte)((value & 0xFF000000) >> 24), byte2 = (byte)((value & 0x00FF0000) >> 16), byte1 = (byte)((value & 0x0000FF00) >> 8), byte0 = (byte)(value & 0x000000FF);

            // Create a new Color object using the extracted bytes
            return Color.FromArgb(byte3, byte2, byte1, byte0);
        }

        private static double GetImageSizeInKB(Bitmap image) => (image.Width * image.Height * 16) / 1024.0; // Assuming each pixel stores 16 bits
    }
}