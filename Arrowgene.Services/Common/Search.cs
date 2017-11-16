/*
 * MIT License
 * 
 * Copyright (c) 2018 Sebastian Heinz <sebastian.heinz.gt@googlemail.com>
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace Arrowgene.Services.Common
{
    using System;

    public static class Search
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