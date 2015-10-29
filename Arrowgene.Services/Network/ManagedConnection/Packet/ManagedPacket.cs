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
namespace Arrowgene.Services.Network.ManagedConnection.Packet
{
    using System;

    /// <summary>
    /// Class to manage packet content
    /// </summary>
    public class ManagedPacket
    {
        /// <summary>
        /// Size of header
        /// </summary>
        public const Int32 HEADER_SIZE = 16;

        /// <summary>
        /// Size of payload header part
        /// </summary>
        public const Int32 HEADER_PAYLOAD_SIZE = 4;

        /// <summary>
        /// size of id header part
        /// </summary>
        public const Int32 HEADER_ID_SIZE = 4;

        /// <summary>
        /// Creates a new <see cref="ManagedPacket"/> from packetId and payload
        /// </summary>
        /// <param name="packetId"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static ManagedPacket CreatePacket(Int32 packetId, byte[] payload)
        {
            Int32 payloadSize = payload.Length;

            byte[] payloadSizeBytes = BitConverter.GetBytes(payloadSize);
            byte[] packetIdBytes = BitConverter.GetBytes(packetId);
            byte[] header = new byte[HEADER_SIZE];

            Buffer.BlockCopy(payloadSizeBytes, 0, header, 0, payloadSizeBytes.Length);
            Buffer.BlockCopy(packetIdBytes, 0, header, payloadSizeBytes.Length, packetIdBytes.Length);

            ManagedPacket managedPacket = new ManagedPacket(packetId, header, payload);
            return managedPacket;
        }

        /// <summary>
        /// Creates a new packet
        /// </summary>
        /// <param name="id"></param>
        /// <param name="header"></param>
        /// <param name="payload"></param>
        public ManagedPacket(int id, byte[] header, byte[] payload)
        {
            this.Id = id;
            this.Header = header;
            this.Payload = payload;
            this.Object = null;
        }

        /// <summary>
        /// Header
        /// </summary>
        public byte[] Header { get; set; }

        /// <summary>
        /// Payload
        /// </summary>
        public byte[] Payload { get; set; }

        /// <summary>
        /// Size of the total bytes header + payload
        /// </summary>
        public int PacketSize { get { return this.Payload.Length + HEADER_SIZE; } }

        /// <summary>
        /// Id to identify the packet
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Transfered Object
        /// </summary>
        public object Object { get; internal set; }

        /// <summary>
        /// Returns concrete class or value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetObject<T>()
        {
            T myObject = (T)this.Object;
            return myObject;
        }

        /// <summary>
        /// Returns the packet as byte array
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            byte[] bytes = new byte[this.PacketSize];

            Buffer.BlockCopy(this.Header, 0, bytes, 0, HEADER_SIZE);
            Buffer.BlockCopy(this.Payload, 0, bytes, HEADER_SIZE, this.Payload.Length);

            return bytes;
        }
    }
}