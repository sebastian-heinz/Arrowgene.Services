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
namespace ArrowgeneServices.Networking.MarrySocket.MBase
{
    using ArrowgeneServices.Logging;
    using ArrowgeneServices.Networking.MarrySocket.Packet;
    using ArrowgeneServices.Networking.MarrySocket.Serialization;
    using System;
    using System.Net.Sockets;

    /// <summary>
    /// TODO SUMMARY
    /// </summary>
    public abstract class BaseSocket
    {
         internal Logger logger;
         internal ISerialization serializer;

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        protected BaseSocket(Socket socket, Logger logger, ISerialization serializer)
        {
            this.Socket = socket;
            this.logger = logger;
            this.serializer = serializer;
            this.OutTraffic = 0;
            this.InTraffic = 0;
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public Int64 InTraffic { get; internal set; }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public Int64 OutTraffic { get; private set; }

        internal Socket Socket { get; private set; }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public virtual void SendObject(Int32 packetId, object myClass)
        {
            byte[] serialized = this.serializer.Serialize(myClass, this.logger);
            if (serialized != null)
            {
                CraftPacket craftPacket = new CraftPacket(packetId, myClass.GetType(), serialized);
                this.Socket.Send(craftPacket.Buffer);
                this.OutTraffic += craftPacket.BufferSize;
            }
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        protected virtual void Disconnect()
        {
            if (this.Socket.Connected)
            {
                this.Socket.Shutdown(SocketShutdown.Both);
            }
            this.Socket.Close();
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        protected abstract void Error(string error);
    }
}
