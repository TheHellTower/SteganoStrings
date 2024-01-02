using System;

namespace SteganoStrings
{
    public static class ArrayExtensions
    {
        private static Random Random = new Random();
        public static void Shuffle<T>(this T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = Random.Next(0, i + 1);

                // Swap array[i] and array[j]
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }

        public static string GetRandomString<T>(this T[] array) => array[Random.Next(0, array.Length)].ToString();
    }
}