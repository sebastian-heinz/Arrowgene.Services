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

    public class PacketCrafter : PacketBase
    {
        private BinaryWriter memoryWriter;

        public PacketCrafter()
        {
            base.memoryBuffer = new MemoryStream();
            this.memoryWriter = new BinaryWriter(base.memoryBuffer);
        }

        public void Addbyte(byte b)
        {
            this.memoryWriter.Write(b);
        }

        public void Addbyte(short s)
        {
            this.Addbyte((byte) s);
        }

        public void Addbyte(int i)
        {
            this.Addbyte((byte) i);
        }

        public void Addbyte(uint i)
        {
            this.Addbyte((byte) i);
        }

        public void Addbytes(params byte[] bytes)
        {
            this.memoryWriter.Write(bytes);
        }

        public void Addfloat(double d)
        {
            this.memoryWriter.Write((float) d);
        }

        public void Addfloat(float f)
        {
            this.memoryWriter.Write(f);
        }

        public void Addint16(short s)
        {
            this.memoryWriter.Write(s);
        }

        public void Addint16(int i)
        {
            this.Addint16((short) i);
        }

        public void Addint16(long l)
        {
            this.Addint16((short) l);
        }

        public void Addint16(uint i)
        {
            this.Addint16((short) i);
        }

        public void Addint32(int i)
        {
            this.memoryWriter.Write(i);
        }

        public void Addint32(long l)
        {
            this.Addint32((int) l);
        }

        public void Addint32(uint i)
        {
            this.Addint32((int) i);
        }

        public void Addint64(long l)
        {
            this.memoryWriter.Write(l);
        }

        public void Addstring(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                this.Addbyte((int)str[i]);
            }
        }

        public override byte[] Buffer
        {
            get
            {
                return base.memoryBuffer.ToArray();
            }
        }

    }
}

