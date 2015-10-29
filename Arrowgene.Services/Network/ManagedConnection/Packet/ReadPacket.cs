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
    using Arrowgene.Services.Common;
    using System;

    /// <summary>
    /// TODO SUMMARY
    /// </summary>
    public class ReadPacket
    {
        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public ReadPacket(PacketHeader packetHeader, byte[] data)
        {
            this.PacketHeader = packetHeader;

            string typeName = Conversion.GetString(data, 0, this.PacketHeader.TypeNameSize);
            this.Type = Type.GetType(typeName);

            this.SerializedClass = new byte[this.PacketHeader.SerializedClassSize];
            Array.Copy(data, this.PacketHeader.TypeNameSize, this.SerializedClass, 0, this.PacketHeader.SerializedClassSize);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public PacketHeader PacketHeader { get; private set; }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public byte[] SerializedClass { get; private set; }
    }
}
