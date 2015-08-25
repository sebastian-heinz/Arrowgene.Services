/*
 *  Copyright 2015 Sebastian Heinz <sebastian.heinz.gt@googlemail.com>
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
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