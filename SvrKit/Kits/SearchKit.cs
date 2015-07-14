namespace SvrKit.Kits
{
    using System;

    public static class SearchKit
    {

        /// <summary>
        /// Find subarray in the source array.
        /// </summary>
        /// <param name="array">Source array to search for needle.</param>
        /// <param name="needle">Needle we are searching for.</param>
        /// <param name="startIndex">Start index in source array.</param>
        /// <param name="sourceLength">Number of bytes in source array, where the needle is searched for.</param>
        /// <returns>Returns starting position of the needle if it was found or <b>-1</b> otherwise.</returns>
        public static int Find(byte[] array, byte[] needle, int startIndex)
        {
            int needleLen = needle.Length;
            int index;
            int sourceLength = array.Length;

            while (sourceLength >= needleLen)
            {
                index = Array.IndexOf(array, needle[0], startIndex, sourceLength - needleLen + 1);

                if (index == -1)
                    return -1;

                int i, p;

                for (i = 0, p = index; i < needleLen; i++, p++)
                {
                    if (array[p] != needle[i])
                    {
                        break;
                    }
                }

                if (i == needleLen)
                {
                    return index;
                }

                sourceLength -= (index - startIndex + 1);
                startIndex = index + 1;
            }
            return -1;
        }
    }
}