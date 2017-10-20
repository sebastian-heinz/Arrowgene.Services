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

    public class ByteBuffer : IBuffer
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

        public int Size
        {
            get { return (int) _memoryStream.Length; }
        }

        public int Position
        {
            get { return (int) _memoryStream.Position; }
            set { _memoryStream.Position = value; }
        }

        public void SetPositionStart()
        {
            Position = 0;
        }

        public void SetPositionEnd()
        {
            Position = Size - 1;
        }

        public IBuffer Clone(int offset, int length)
        {
            return new BBuffer(GetBytes(offset, length));
        }

        public IBuffer Clone(int length)
        {
            return Clone(0, length);
        }

        public IBuffer Clone()
        {
            return Clone(Size);
        }

        public byte[] GetAllBytes()
        {
            return GetAllBytes(0);
        }

        public byte[] GetAllBytes(int offset)
        {
            return GetBytes(offset, Size - offset);
        }

        public void WriteBytes(byte[] bytes)
        {
            _binaryWriter.Write(bytes);
        }

        public void WriteBytes(byte[] bytes, int offset, int length)
        {
            _binaryWriter.Write(bytes, offset, length);
        }

        public void WriteByte(byte value)
        {
            _binaryWriter.Write(value);
        }

        public void WriteByte(int value)
        {
            _binaryWriter.Write((byte) value);
        }

        public void WriteByte(long value)
        {
            _binaryWriter.Write((byte) value);
        }

        public void WriteInt16(short value)
        {
            _binaryWriter.Write(value);
        }

        public void WriteInt16(int value)
        {
            _binaryWriter.Write((short) value);
        }

        public void WriteInt32(int value)
        {
            _binaryWriter.Write(value);
        }

        public void WriteFloat(float value)
        {
            _binaryWriter.Write(value);
        }

        public void WriteString(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                _binaryWriter.Write((int) value[i]);
            }
        }

        public void WriteFixedString(string value, int length)
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

        public void WriteBuffer(IBuffer value)
        {
            WriteBytes(value.GetAllBytes());
        }

        public void WriteCString(string value)
        {
            WriteString(value);
            WriteByte(0);
        }

        public byte ReadByte()
        {
            return _binaryReader.ReadByte();
        }

        public byte GetByte(int offset)
        {
            int position = Position;
            Position = offset;
            byte b = ReadByte();
            Position = position;
            return b;
        }

        public byte[] ReadBytes(int length)
        {
            return _binaryReader.ReadBytes(length);
        }

        public byte[] GetBytes(int offset, int length)
        {
            int position = Position;
            Position = offset;
            byte[] bytes = ReadBytes(length);
            Position = position;
            return bytes;
        }

        public short GetInt16(int offset)
        {
            int position = Position;
            Position = offset;
            short value = ReadInt16();
            Position = position;
            return value;
        }

        public short ReadInt16()
        {
            return _binaryReader.ReadInt16();
        }

        public int GetInt32(int offset)
        {
            int position = Position;
            Position = offset;
            int value = ReadInt32();
            Position = position;
            return value;
        }

        public int ReadInt32()
        {
            return _binaryReader.ReadInt32();
        }

        public float GetFloat(int offset)
        {
            int position = Position;
            Position = offset;
            float value = ReadFloat();
            Position = position;
            return value;
        }

        public float ReadFloat()
        {
            return _binaryReader.ReadSingle();
        }

        public string GetString(int offset, int length)
        {
            int position = Position;
            Position = offset;
            string value = ReadString(length);
            Position = position;
            return value;
        }

        public string ReadString(int length)
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

        public string ReadCString()
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

        public string GetCString(int offset)
        {
            int position = Position;
            Position = offset;
            string value = ReadCString();
            Position = position;
            return value;
        }

        public string ToHexString()
        {
            byte[] buffer = GetAllBytes();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < buffer.Length; i++)
            {
                sb.Append(buffer[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public string ToAsciiString(bool spaced)
        {
            byte[] buffer = GetAllBytes();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < buffer.Length; i++)
            {
                char c = '.';
                if (buffer[i] >= 'A' && buffer[i] <= 'Z') c = (char) buffer[i];
                if (buffer[i] >= 'a' && buffer[i] <= 'z') c = (char) buffer[i];
                if (buffer[i] >= '0' && buffer[i] <= '9') c = (char) buffer[i];
                if (spaced && i != 0)
                {
                    sb.Append("  ");
                }
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}