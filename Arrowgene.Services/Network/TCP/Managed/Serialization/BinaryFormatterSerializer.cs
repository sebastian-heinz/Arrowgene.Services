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
namespace Arrowgene.Services.Network.TCP.Managed.Serialization
{
    using Arrowgene.Services.Logging;
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;


    public class BinaryFormatterSerializer : ISerializer
    {

        public byte[] Serialize(object myClass, Logger logger)
        {
            byte[] serialized = null;
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, myClass);
                    if (stream.Length < Int32.MaxValue)
                    {
                        serialized = new byte[stream.Length];
                        Buffer.BlockCopy(stream.GetBuffer(), 0, serialized, 0, (int)stream.Length);
                    }
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                logger.Write("Failed to serialize. Reason: {0}", ex.Message, LogType.ERROR);
            }
            return serialized;
        }

        public object Deserialize(byte[] data, Logger logger)
        {
            object myClass = null;
            try
            {
                using (MemoryStream stream = new MemoryStream(data))
                {
                    IFormatter formatter = new BinaryFormatter();
                    myClass = formatter.Deserialize(stream);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                logger.Write("Failed to deserialize. Reason: {0}", ex.Message, LogType.ERROR);
            }
            return myClass;
        }

    }
}
