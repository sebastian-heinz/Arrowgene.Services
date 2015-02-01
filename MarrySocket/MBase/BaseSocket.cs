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
namespace MarrySocket.MBase
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    public abstract class BaseSocket
    {
        internal BaseSocket(Socket socket)
        {
            this.Socket = socket;
        }

        internal BaseSocket()
        {
            this.Socket = null;
        }

        internal Socket Socket { get; set; }

        public virtual void SendObject(Int16 id, object myClass)
        {
            Packet packet = new Packet();
            packet.PacketId = id;
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
                    this.Error("Failed to serialize. Reason: " + e.ToString());
                }
                stream.Close();
            }

            if (serialized != null)
            {
                packet.SetPacketData(serialized);
                this.Socket.Send(packet.GetPacketForSending());
            }
        }

        internal virtual void Disconnect()
        {
            if (this.Socket.Connected)
            {
                this.Socket.Shutdown(SocketShutdown.Both);
            }
            this.Socket.Close();
        }

        internal abstract void Error(string error);
    }
}
