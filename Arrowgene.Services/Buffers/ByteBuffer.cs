/*
 * MIT License
 * 
 * Copyright (c) 2018 Sebastian Heinz <sebastian.heinz.gt@googlemail.com>
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace Arrowgene.Services.Buffers
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
            SetPositionStart();
        }

        public ByteBuffer(byte[] buffer, int index, int count) : this()
        {
            _binaryWriter.Write(buffer, index, count);
            SetPositionStart();
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
            SetPositionStart();
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

        public override void WriteBytes(byte[] source, int srcOffset, int length)
        {
            throw new System.NotImplementedException();
        }

        public override void WriteBytes(byte[] source, int srcOffset, int dstOffset, int count)
        {
            throw new System.NotImplementedException();
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
                _binaryWriter.Write((byte) value[i]);
            }
        }

        public override void WriteFixedString(string value, int length)
        {
            for (int i = 0; i < length; i++)
            {
                _binaryWriter.Write((byte) value[i]);
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