using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Runtime
{
    public static class Runtime
    {
        public static void Initialize(string String)
        {
            using (Stream manifestResourceStream = Assembly.GetCallingAssembly().GetManifestResourceStream(String))
            {
                Bitmap image = new Bitmap(manifestResourceStream);
                string extractedText = "";
                Color lastPixel = image.GetPixel(image.Width - 1, image.Height - 1);
                int textLength = lastPixel.B;

                int charIndex = 0;
                for (int i = 0; i < image.Width; i++)
                    for (int j = 0; j < image.Height; j++)
                    {
                        if (charIndex < textLength)
                        {
                            Color pixel = image.GetPixel(i, j);
                            int value = pixel.B;
                            char c = Convert.ToChar(value);
                            extractedText += c;
                        }

                        charIndex++;
                    }
                List = extractedText.Split(new string[] { @"\_THT_/" }, StringSplitOptions.None).ToList<string>();
            }
        }

        public static string GetString(int index) => List[index];

        public static List<string> List = new List<string>();
    }
}