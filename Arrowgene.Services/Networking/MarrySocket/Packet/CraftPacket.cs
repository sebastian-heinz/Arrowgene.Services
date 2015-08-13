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
namespace Arrowgene.Services.Networking.MarrySocket.Packet
{
    using Arrowgene.Services.Common;
    using System;

    /// <summary>
    /// TODO SUMMARY
    /// </summary>
    public class CraftPacket : PacketCrafter
    {
        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public CraftPacket(Int32 packetId, Type type, byte[] serializedClass)
        {
            byte[] typeName = Conversion.GetBytes(type.AssemblyQualifiedName);
            Int32 packetSize = typeName.Length + serializedClass.Length + PacketHeader.HEADER_SIZE;

            this.Addint32(packetSize);
            this.Addint32(packetId);
            this.Addint32(typeName.Length);
            this.Addint32(serializedClass.Length);
            this.Addbytes(typeName);
            this.Addbytes(serializedClass);
        }


    }
}
