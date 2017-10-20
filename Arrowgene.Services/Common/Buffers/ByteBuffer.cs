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

using System.Text;

namespace Arrowgene.Services.Common.Buffers
{
    using System.IO;

    public class ByteBuffer : Buffer
    {
        private MemoryStream _memoryStream;
        private BinaryWriter _binaryWriter;
        private BinaryReader _binaryReader;

        public ByteBuffer()
        {
            _memoryStream = new MemoryStream();
            _binaryReader = new BinaryReader(_memoryStream);
            _binaryWriter = new BinaryWriter(_memoryStream);
        }

        public ByteBuffer(byte[] buffer) : this()
        {
            _binaryWriter.Write(buffer);
        }

        public ByteBuffer(byte[] buffer, int index, int count) : this()
        {
            _binaryWriter.Write(buffer, index, count);
        }

        public ByteBuffer(string filePath) : this()
        {
            int bufferSize = 1024;
            byte[] buffer = new byte[bufferSize];
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                int read = 0;
                while ((read = fileStream.Read(buffer, 0, bufferSize)) > 0)
                {
                    _binaryWriter.Write(buffer, 0, read);
                }
            }
        }

        public override int Size
        {
            get { return (int) _memoryStream.Length; }
        }

        public override int Position
        {
            get { return (int) _memoryStream.Position; }
            set { _memoryStream.Position = value; }
        }

        public override void SetPositionStart()
        {
            Position = 0;
        }

        public override void SetPositionEnd()
        {
            Position = Size - 1;
        }

        public override IBuffer Clone(int offset, int length)
        {
            return new BBuffer(GetBytes(offset, length));
        }

        public override byte[] GetAllBytes()
        {
            return GetAllBytes(0);
        }

        public override byte[] GetAllBytes(int offset)
        {
            return GetBytes(offset, Size - offset);
        }

        public override void WriteBytes(byte[] bytes)
        {
            _binaryWriter.Write(bytes);
        }

        public override void WriteBytes(byte[] bytes, int offset, int length)
        {
            _binaryWriter.Write(bytes, offset, length);
        }

        public override void WriteByte(byte value)
        {
            _binaryWriter.Write(value);
        }

        public override void WriteByte(int value)
        {
            _binaryWriter.Write((byte) value);
        }

        public override void WriteByte(long value)
        {
            _binaryWriter.Write((byte) value);
        }

        public override void WriteInt16(short value)
        {
            _binaryWriter.Write(value);
        }

        public override void WriteInt16(int value)
        {
            _binaryWriter.Write((short) value);
        }

        public override void WriteInt32(int value)
        {
            _binaryWriter.Write(value);
        }

        public override void WriteFloat(float value)
        {
            _binaryWriter.Write(value);
        }

        public override void WriteString(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                _binaryWriter.Write((int) value[i]);
            }
        }

        public override void WriteFixedString(string value, int length)
        {
            for (int i = 0; i < length; i++)
            {
                _binaryWriter.Write((int) value[i]);
            }
            int diff = length - value.Length;
            if (diff > 0)
            {
                _binaryWriter.Write(new byte[diff]);
            }
        }

        public override void WriteCString(string value)
        {
            WriteString(value);
            WriteByte(0);
        }

        public override byte ReadByte()
        {
            return _binaryReader.ReadByte();
        }

        public override byte GetByte(int offset)
        {
            int position = Position;
            Position = offset;
            byte b = ReadByte();
            Position = position;
            return b;
        }

        public override byte[] ReadBytes(int length)
        {
            return _binaryReader.ReadBytes(length);
        }

        public override byte[] GetBytes(int offset, int length)
        {
            int position = Position;
            Position = offset;
            byte[] bytes = ReadBytes(length);
            Position = position;
            return bytes;
        }

        public override short GetInt16(int offset)
        {
            int position = Position;
            Position = offset;
            short value = ReadInt16();
            Position = position;
            return value;
        }

        public override short ReadInt16()
        {
            return _binaryReader.ReadInt16();
        }

        public override int GetInt32(int offset)
        {
            int position = Position;
            Position = offset;
            int value = ReadInt32();
            Position = position;
            return value;
        }

        public override int ReadInt32()
        {
            return _binaryReader.ReadInt32();
        }

        public override float GetFloat(int offset)
        {
            int position = Position;
            Position = offset;
            float value = ReadFloat();
            Position = position;
            return value;
        }

        public override float ReadFloat()
        {
            return _binaryReader.ReadSingle();
        }

        public override string GetString(int offset, int length)
        {
            int position = Position;
            Position = offset;
            string value = ReadString(length);
            Position = position;
            return value;
        }

        public override string ReadString(int length)
        {
            string s = string.Empty;

            for (int i = 0; i < length; i++)
            {
                if (_memoryStream.Position < _memoryStream.Length)
                {
                    byte b = _binaryReader.ReadByte();
                    if (b > 0)
                    {
                        s = s + ((char) b);
                    }
                }
            }
            return s;
        }

        public override string ReadCString()
        {
            string s = string.Empty;
            bool read = true;
            while (_memoryStream.Position < _memoryStream.Length && read)
            {
                byte b = _binaryReader.ReadByte();
                if (b > 0)
                {
                    s = s + (char) b;
                }
                else
                {
                    read = false;
                }
            }
            return s;
        }

        public override string GetCString(int offset)
        {
            int position = Position;
            Position = offset;
            string value = ReadCString();
            Position = position;
            return value;
        }
      
    }
}