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
namespace Arrowgene.Services.Network.MarrySocket.Packet
{
    using System;

    /// <summary>
    /// TODO SUMMARY
    /// </summary>
    public class PacketHeader
    {
        /// <summary>TODO SUMMARY</summary>
        public const Int32 HEADER_SIZE = 16;
        /// <summary>TODO SUMMARY</summary>
        public const Int32 HEADER_PACKET_LENGTH = 4;
        /// <summary>TODO SUMMARY</summary>
        public const Int32 HEADER_ID_LENGTH = 4;
        /// <summary>TODO SUMMARY</summary>
        public const Int32 HEADER_TYPE_LENGTH = 4;
        /// <summary>TODO SUMMARY</summary>
        public const Int32 HEADER_SERIALIZED_CLASS_LENGTH = 4;

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public static PacketHeader CreateInstance(byte[] headerBuffer)
        {
            Int32 packetSize = BitConverter.ToInt32(headerBuffer, 0);
            Int32 packetId = BitConverter.ToInt32(headerBuffer, PacketHeader.HEADER_PACKET_LENGTH);
            Int32 typeNameSize = BitConverter.ToInt32(headerBuffer, PacketHeader.HEADER_PACKET_LENGTH + PacketHeader.HEADER_ID_LENGTH);
            Int32 serializedClassSize = BitConverter.ToInt32(headerBuffer, PacketHeader.HEADER_PACKET_LENGTH + PacketHeader.HEADER_ID_LENGTH + PacketHeader.HEADER_SERIALIZED_CLASS_LENGTH);
            return new PacketHeader(packetSize, packetId, typeNameSize, serializedClassSize);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public PacketHeader(Int32 packetSize, Int32 packetId, Int32 typeNameSize, Int32 serializedClassSize)
        {
            this.PacketSize = packetSize;
            this.PacketId = packetId;
            this.TypeNameSize = typeNameSize;
            this.SerializedClassSize = serializedClassSize;
        }

        /// <summary>TODO SUMMARY</summary>
        public Int32 PacketId { get; private set; }
        /// <summary>TODO SUMMARY</summary>
        public Int32 PacketSize { get; private set; }
        /// <summary>TODO SUMMARY</summary>
        public Int32 TypeNameSize { get; private set; }
        /// <summary>TODO SUMMARY</summary>
        public Int32 SerializedClassSize { get; private set; }
        /// <summary>TODO SUMMARY</summary>
        public Int32 DataSize { get { return this.PacketSize - HEADER_SIZE; } }
    }
}
