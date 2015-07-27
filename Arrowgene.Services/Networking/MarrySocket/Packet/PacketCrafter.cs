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
namespace ArrowgeneServices.Networking.MarrySocket.Packet
{
    using System.IO;

    /// <summary>
    /// TODO SUMMARY
    /// </summary>
    public class PacketCrafter : PacketBase
    {
        private BinaryWriter memoryWriter;

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public PacketCrafter()
        {
            base.memoryBuffer = new MemoryStream();
            this.memoryWriter = new BinaryWriter(base.memoryBuffer);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public void Addbyte(byte b)
        {
            this.memoryWriter.Write(b);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public void Addbyte(short s)
        {
            this.Addbyte((byte)s);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public void Addbyte(int i)
        {
            this.Addbyte((byte)i);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public void Addbyte(uint i)
        {
            this.Addbyte((byte)i);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public void Addbytes(params byte[] bytes)
        {
            this.memoryWriter.Write(bytes);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public void Addfloat(double d)
        {
            this.memoryWriter.Write((float)d);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public void Addfloat(float f)
        {
            this.memoryWriter.Write(f);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public void Addint16(short s)
        {
            this.memoryWriter.Write(s);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public void Addint16(int i)
        {
            this.Addint16((short)i);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public void Addint16(long l)
        {
            this.Addint16((short)l);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public void Addint16(uint i)
        {
            this.Addint16((short)i);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public void Addint32(int i)
        {
            this.memoryWriter.Write(i);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public void Addint32(long l)
        {
            this.Addint32((int)l);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public void Addint32(uint i)
        {
            this.Addint32((int)i);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public void Addint64(long l)
        {
            this.memoryWriter.Write(l);
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public void Addstring(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                this.Addbyte((int)str[i]);
            }
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public override byte[] Buffer
        {
            get
            {
                return base.memoryBuffer.ToArray();
            }
        }

    }
}

