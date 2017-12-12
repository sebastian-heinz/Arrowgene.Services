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


using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Arrowgene.Services.Protocols
{
    public class BinaryFormatterSerializer<T>
    {
        public byte[] Serialize(T message)
        {
            byte[] serialized;
            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, message);
                if (stream.Length < int.MaxValue)
                {
                    serialized = new byte[stream.Length];
                    Buffer.BlockCopy(stream.GetBuffer(), 0, serialized, 0, (int) stream.Length);
                }
                else
                {
                    serialized = null;
                }
                stream.Close();
            }
            return serialized;
        }

        public T Deserialize(byte[] data)
        {
            T message;
            using (MemoryStream stream = new MemoryStream(data))
            {
                IFormatter formatter = new BinaryFormatter();
                message = (T) formatter.Deserialize(stream);
                stream.Close();
            }
            return message;
        }
    }
}