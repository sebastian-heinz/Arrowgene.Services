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
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class Packet
    {
        public const int HEADER_SIZE = 6;
        public const int PACKET_LENGTH_SIZE = 4;

        private MemoryStream memoryStream;
        private BinaryWriter memoryWriter;

        public Packet()
        {
            this.memoryStream = new MemoryStream();
            this.memoryWriter = new BinaryWriter(this.memoryStream);
        }

        public Int16 PacketId { get; set; }
        public Int32 PacketSize { get; private set; }

        public void SetPacketData(byte[] data)
        {
            this.memoryStream.SetLength(0);
            this.memoryStream.Position = Packet.HEADER_SIZE;
            this.memoryWriter.Write(data);
            this.PacketSize = data.Length + Packet.HEADER_SIZE;
        }

        public byte[] GetPacketForSending()
        {
            this.memoryStream.Position = 0;
            memoryWriter.Write(this.PacketSize);
            memoryWriter.Write(this.PacketId);

            return this.memoryStream.ToArray();
        }

        public MemoryStream GetPacketForReading()
        {
            this.memoryStream.Position = Packet.HEADER_SIZE;

            return this.memoryStream;
        }
    }
}
