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
namespace Arrowgene.Services.Common
{
    using System;
    using System.IO;

    //TODO MaxBufferSize = Int32.Max

    public class ByteBuffer
    {
        public static byte[] BlockCopy(byte[] source)
        {
            return ByteBuffer.BlockCopy(source, source.Length);
        }

        public static byte[] BlockCopy(byte[] source, int size)
        {
            byte[] destination = new byte[size];
            Buffer.BlockCopy(source, 0, destination, 0, size);
            return destination;
        }

        private MemoryStream memoryStream;
        private BinaryWriter binaryWriter;
        private BinaryReader binaryReader;

        public ByteBuffer()
        {
            this.memoryStream = new MemoryStream();
            this.binaryReader = new BinaryReader(this.memoryStream);
            this.binaryWriter = new BinaryWriter(this.memoryStream);
        }

        public ByteBuffer(byte[] buffer)
            : this()
        {
            this.binaryWriter.Write(buffer);
        }

        public ByteBuffer(byte[] buffer, int index, int count)
     : this()
        {
            this.binaryWriter.Write(buffer, index, count);
        }

        public ByteBuffer(string filePath)
            : this()
        {
            int bufferSize = 1024;
            byte[] buffer = new byte[bufferSize];
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                int read = 0;
                while ((read = fileStream.Read(buffer, 0, bufferSize)) > 0)
                {
                    this.binaryWriter.Write(buffer, 0, read);
                }
            }
        }

        public long Size { get { return this.memoryStream.Length; } }

        public long Position { get { return this.memoryStream.Position; } private set { this.memoryStream.Position = value; } }

        public void ResetPosition()
        {
            this.Position = 0;
        }

        public bool SetPosition(long position)
        {
            if(position < this.Size)
            {
                this.Position = position;
                return true;
            }
            return false;
        }

        public byte[] ReadBytes()
        {
            this.Position = 0;
            return this.ReadBytes((int)this.Size);
        }

        #region read
        public byte ReadByte()
        {
            return this.binaryReader.ReadByte();
        }

        public byte[] ReadBytes(int length)
        {
            return this.binaryReader.ReadBytes(length);
        }

        public int ReadInt16()
        {
            return this.binaryReader.ReadInt16();
        }

        public int ReadInt32()
        {
            return this.binaryReader.ReadInt32();
        }

        public float ReadFloat()
        {
            return this.binaryReader.ReadSingle();
        }

        public string ReadZeroString()
        {
            string s = string.Empty;
            bool read = true;
            while (this.memoryStream.Position < this.memoryStream.Length && read)
            {
                byte b = this.binaryReader.ReadByte();
                if (b > 0)
                {
                    s = s + ((char)b);
                }
                else
                {
                    read = false;
                }
            }
            return s;
        }

        public string ReadString(int length)
        {
            string s = string.Empty;

            for (int i = 0; i < length; i++)
            {
                if (this.memoryStream.Position < this.memoryStream.Length)
                {
                    byte b = this.binaryReader.ReadByte();
                    if (b > 0)
                    {
                        s = s + ((char)b);
                    }
                }
            }
            return s;
        }
        #endregion read

        #region write
        public void WriteByte(byte b)
        {
            this.binaryWriter.Write(b);
        }

        public void WriteBuffer(ByteBuffer buffer)
        {
            this.WriteBytes(buffer.ReadBytes());
        }

        public void WriteByte(int i)
        {
            this.binaryWriter.Write((byte)i);
        }

        public void WriteBytes(byte[] b)
        {
            this.binaryWriter.Write(b);
        }

        public void WriteBytes(byte[] b, int index, int count)
        {
            this.binaryWriter.Write(b, index, count);
        }

        public void WriteInt16(short i)
        {
            this.binaryWriter.Write(i);
        }

        public void WriteInt16(int i)
        {
            this.binaryWriter.Write((short)i);
        }

        public void WriteInt32(int i)
        {
            this.binaryWriter.Write(i);
        }

        public void WriteString(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                this.binaryWriter.Write((int)s[i]);
            }
        }

        public void WriteFixedString(string s, int length)
        {
            int sLength = s.Length;
            int diff = length - sLength;
            if (diff >= 0)
            {
                for (int i = 0; i < sLength; i++)
                {
                    this.binaryWriter.Write((int)s[i]);
                }
                if (diff > 0)
                {
                    this.binaryWriter.Write(new byte[diff]);
                }
            }
        }

        #endregion write

    }
}