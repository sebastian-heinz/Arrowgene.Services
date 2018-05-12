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


using System.IO;

namespace Arrowgene.Services.Buffers
{
    public class StreamBuffer : Buffer
    {
        private readonly MemoryStream _memoryStream;
        private readonly BinaryWriter _binaryWriter;
        private readonly BinaryReader _binaryReader;

        public StreamBuffer()
        {
            _memoryStream = new MemoryStream();
            _binaryReader = new BinaryReader(_memoryStream);
            _binaryWriter = new BinaryWriter(_memoryStream);
        }

        public StreamBuffer(byte[] buffer) : this()
        {
            _binaryWriter.Write(buffer);
        }

        public StreamBuffer(byte[] buffer, int index, int count) : this()
        {
            _binaryWriter.Write(buffer, index, count);
        }

        public StreamBuffer(string filePath) : this()
        {
            const int bufferSize = 1024;
            byte[] buffer = new byte[bufferSize];
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                int read;
                while ((read = fileStream.Read(buffer, 0, bufferSize)) > 0)
                {
                    _binaryWriter.Write(buffer, 0, read);
                }
            }
        }

        public override int Size => (int) _memoryStream.Length;

        public override int Position
        {
            get => (int) _memoryStream.Position;
            set => _memoryStream.Position = value;
        }

        public sealed override void SetPositionStart()
        {
            Position = 0;
        }

        public override void SetPositionEnd()
        {
            Position = Size;
        }

        public override IBuffer Clone(int offset, int length)
        {
            return new StreamBuffer(GetBytes(offset, length));
        }

        public override IBuffer Provide()
        {
            return new StreamBuffer();
        }

        public override IBuffer Provide(byte[] buffer)
        {
            return new StreamBuffer(buffer);
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
            _memoryStream.Write(source, srcOffset, length);
        }

        public override void WriteBytes(byte[] source, int srcOffset, int dstOffset, int count)
        {
            _memoryStream.Position = dstOffset;
            _memoryStream.Write(source, srcOffset, count);
        }

        public override void WriteByte(byte value)
        {
            _binaryWriter.Write(value);
        }

        public override void WriteInt16(short value)
        {
            _binaryWriter.Write(value);
        }

        public override void WriteInt16(ushort value)
        {
            _binaryWriter.Write(value);
        }

        public override void WriteInt32(int value)
        {
            _binaryWriter.Write(value);
        }

        public override void WriteInt32(uint value)
        {
            _binaryWriter.Write(value);
        }

        public override void WriteInt64(long value)
        {
            _binaryWriter.Write(value);
        }

        public override void WriteInt64(ulong value)
        {
            _binaryWriter.Write(value);
        }

        public override void WriteFloat(float value)
        {
            _binaryWriter.Write(value);
        }

        public override void WriteDouble(double value)
        {
            _binaryWriter.Write(value);
        }

        public override void WriteDecimal(decimal value)
        {
            _binaryWriter.Write(value);
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

        public override ushort GetUInt16(int offset)
        {
            int position = Position;
            Position = offset;
            ushort value = ReadUInt16();
            Position = position;
            return value;
        }

        public override short ReadInt16()
        {
            return _binaryReader.ReadInt16();
        }

        public override ushort ReadUInt16()
        {
            return _binaryReader.ReadUInt16();
        }

        public override int GetInt32(int offset)
        {
            int position = Position;
            Position = offset;
            int value = ReadInt32();
            Position = position;
            return value;
        }

        public override uint GetUInt32(int offset)
        {
            int position = Position;
            Position = offset;
            uint value = ReadUInt32();
            Position = position;
            return value;
        }

        public override int ReadInt32()
        {
            return _binaryReader.ReadInt32();
        }

        public override uint ReadUInt32()
        {
            return _binaryReader.ReadUInt32();
        }

        public override long GetInt64(int offset)
        {
            int position = Position;
            Position = offset;
            long value = ReadInt64();
            Position = position;
            return value;
        }

        public override ulong GetUInt64(int offset)
        {
            int position = Position;
            Position = offset;
            ulong value = ReadUInt64();
            Position = position;
            return value;
        }

        public override long ReadInt64()
        {
            return _binaryReader.ReadInt64();
        }

        public override ulong ReadUInt64()
        {
            return _binaryReader.ReadUInt64();
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

        public override double GetDouble(int offset)
        {
            int position = Position;
            Position = offset;
            double value = ReadDouble();
            Position = position;
            return value;
        }

        public override double ReadDouble()
        {
            return _binaryReader.ReadDouble();
        }

        public override decimal GetDecimal(int offset)
        {
            int position = Position;
            Position = offset;
            decimal value = ReadDecimal();
            Position = position;
            return value;
        }

        public override decimal ReadDecimal()
        {
            return _binaryReader.ReadDecimal();
        }
    }
}