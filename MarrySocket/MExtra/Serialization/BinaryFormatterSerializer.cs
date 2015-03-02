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
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MarrySocket.MExtra.Serialization
{
    public class BinaryFormatterSerializer : ISerialization
    {
        public byte[] Serialize<T>(T myClass)
        {
            byte[] serialized = null;

            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(stream, myClass);
                    if (stream.Length < Int32.MaxValue)
                    {
                        serialized = new byte[stream.Length];
                        Buffer.BlockCopy(stream.GetBuffer(), 0, serialized, 0, (int)stream.Length);
                    }
                }
                catch (Exception e)
                {

                }
                stream.Close();
            }
            return serialized;
        }

        public T Deserialize<T>(byte[] data)
        {
            T myObject = default(T);
            using (MemoryStream stream = new MemoryStream(data))
            {
                IFormatter formatter = new BinaryFormatter();
                try
                {
                    myObject = (T)formatter.Deserialize(stream);
                }
                catch (Exception e)
                {

                }
                stream.Close();
            }
            return myObject;
        }

    }
}
