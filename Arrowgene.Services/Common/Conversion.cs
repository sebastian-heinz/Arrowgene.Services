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
  

    public static class Conversion
    {


        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }


        public static string GetString(byte[] bytes, int srcOffset, int length)
        {
            char[] chars = new char[length / sizeof(char)];
            Buffer.BlockCopy(bytes, srcOffset, chars, 0, length);
            return new string(chars);
        }


        public static string GetString(byte[] bytes)
        {
            return GetString(bytes, 0, bytes.Length);
        }

    }
}
