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
namespace MarrySocket.MExtra.Packet
{
    using System.IO;

    public class PacketReader : PacketBase
    {
        private BinaryReader memoryReader;

        public PacketReader(int bufferSize)
        {
            base.memoryBuffer = new MemoryStream(bufferSize);
            this.memoryReader = new BinaryReader(base.memoryBuffer);
        }

        public bool Readbool()
        {
            return this.memoryReader.ReadBoolean();
        }

        public byte Readbyte()
        {
            return this.memoryReader.ReadByte();
        }

        public float Readfloat()
        {
            return this.memoryReader.ReadSingle();
        }

        public short Readint16()
        {
            return this.memoryReader.ReadInt16();
        }

        public int Readint32()
        {
            return this.memoryReader.ReadInt32();
        }

        public long Readint64()
        {
            return this.memoryReader.ReadInt64();
        }

        public string Readstring(int dwLength)
        {
            byte[] buffer = this.memoryReader.ReadBytes(dwLength);
            string str = "";
            for (int i = 0; i < dwLength; i++)
            {
                str = str + ((char) buffer[i]);
            }
            return str;
        }

        public string ReadZeroString()
        {
            string str = "";
            while (this.memoryReader.ReadByte() > 0)
            {
                this.memoryReader.BaseStream.Position -= 1;
                str = str + ((char) this.memoryReader.ReadByte());
            }
            return str;
        }

        public override byte[] Buffer
        {
            get
            {
                return base.Buffer;
            }
            set
            {
                base.memoryBuffer.Seek(0L, SeekOrigin.Begin);
                base.memoryBuffer.Write(value, 0, value.Length);
            }
        }

    }
}