using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Runtime
{
    public static class Runtime
    {
        public static void Initialize(string resourceName)
        {
            using (Stream manifestResourceStream = Assembly.GetCallingAssembly().GetManifestResourceStream(resourceName))
            {
                Bitmap image = new Bitmap(manifestResourceStream);
                string extractedText = "";

                Color lastPixel = GetLastPixelColor(image);
                int textLength = lastPixel.B;

                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                BitmapData bmpData = image.LockBits(rect, ImageLockMode.ReadWrite, image.PixelFormat);

                try
                {
                    IntPtr ptr = bmpData.Scan0;
                    int bytes = Math.Abs(bmpData.Stride) * image.Height, charIndex = 0;
                    byte[] rgbValues = new byte[bytes];
                    Marshal.Copy(ptr, rgbValues, 0, bytes);

                    for (int i = 0; i < image.Width; i++)
                        for (int j = 0; j < image.Height; j++)
                        {
                            if (charIndex < textLength)
                            {
                                int offset = (j * bmpData.Stride) + (i * 3); // Assuming 24bpp image
                                byte blue = rgbValues[offset + 0];
                                char c = Convert.ToChar(blue);
                                extractedText += c;
                            }

                            charIndex++;
                        }
                }
                finally
                {
                    image.UnlockBits(bmpData);
                }

                List = extractedText.Split(new string[] { @"\_THT_/" }, StringSplitOptions.None).ToList();
            }
        }

        static Color GetLastPixelColor(Bitmap image)
        {
            int width = image.Width, height = image.Height;
            BitmapData bmpData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, image.PixelFormat);

            try
            {
                // Calculate the index of the last pixel
                int lastPixelIndex = ((height - 1) * bmpData.Stride) + ((width - 1) * Image.GetPixelFormatSize(image.PixelFormat) / 8);

                // Extract color components from the last pixel
                byte blue = Marshal.ReadByte(bmpData.Scan0, lastPixelIndex), green = Marshal.ReadByte(bmpData.Scan0, lastPixelIndex + 1), red = Marshal.ReadByte(bmpData.Scan0, lastPixelIndex + 2);

                return Color.FromArgb(red, green, blue);
            }
            finally
            {
                image.UnlockBits(bmpData);
            }
        }

        public static string GetString(int index) => List[index];

        public static List<string> List = new List<string>();
    }
}