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


using System;
using System.Collections.Generic;
using System.Text;

namespace Arrowgene.Services.Buffers
{
    public abstract class Buffer : IBuffer, IBufferProvider, ICloneable
    {
        public static string NoEncoding(byte[] bytes)
        {
            string s = string.Empty;
            foreach (byte b in bytes)
            {
                s += (char) b;
            }

            return s;
        }

        public static byte[] NoEncoding(string str)
        {
            List<byte> bytes = new List<byte>();
            foreach (char c in str)
            {
                bytes.Add((byte) c);
            }

            return bytes.ToArray();
        }

        private readonly IEndiannessSwapper _endiannessSwapper;

        public Buffer()
        {
            _endiannessSwapper =
                new EndiannessSwapper(BitConverter.IsLittleEndian ? Endianness.Little : Endianness.Big);
        }

        public Buffer(IEndiannessSwapper endiannessSwapper)
        {
            _endiannessSwapper = endiannessSwapper;
        }

        public Endianness Endianness => _endiannessSwapper.Endianness;
        public abstract int Size { get; }
        public abstract int Position { get; set; }
        public abstract void SetSize(int size);
        public abstract void SetPositionStart();
        public abstract void SetPositionEnd();
        public abstract IBuffer Clone(int offset, int length);
        public abstract IBuffer Provide();
        public abstract IBuffer Provide(byte[] buffer);
        public abstract byte[] GetAllBytes();
        public abstract byte[] GetAllBytes(int offset);
        public abstract void WriteByte(byte value);
        public abstract void WriteBytes(byte[] bytes);
        public abstract void WriteBytes(byte[] source, int srcOffset, int length);
        public abstract void WriteBytes(byte[] source, int srcOffset, int dstOffset, int count);
        public abstract void WriteDecimal(decimal value);
        public abstract byte ReadByte();
        public abstract byte GetByte(int offset);
        public abstract byte[] ReadBytes(int length);
        public abstract byte[] GetBytes(int offset, int length);
        public abstract decimal GetDecimal(int offset);
        public abstract decimal ReadDecimal();
        public abstract void WriteInt16_Implementation(short value);
        public abstract void WriteUInt16_Implementation(ushort value);
        public abstract void WriteInt32_Implementation(int value);
        public abstract void WriteUInt32_Implementation(uint value);
        public abstract void WriteInt64_Implementation(long value);
        public abstract void WriteUInt64_Implementation(ulong value);
        public abstract void WriteFloat_Implementation(float value);
        public abstract void WriteDouble_Implementation(double value);
        public abstract short GetInt16_Implementation(int offset);
        public abstract ushort GetUInt16_Implementation(int offset);
        public abstract short ReadInt16_Implementation();
        public abstract ushort ReadUInt16_Implementation();
        public abstract int GetInt32_Implementation(int offset);
        public abstract uint GetUInt32_Implementation(int offset);
        public abstract int ReadInt32_Implementation();
        public abstract uint ReadUInt32_Implementation();
        public abstract long GetInt64_Implementation(int offset);
        public abstract ulong GetUInt64_Implementation(int offset);
        public abstract long ReadInt64_Implementation();
        public abstract ulong ReadUInt64_Implementation();
        public abstract float GetFloat_Implementation(int offset);
        public abstract float ReadFloat_Implementation();
        public abstract double GetDouble_Implementation(int offset);
        public abstract double ReadDouble_Implementation();

        public virtual void SetEndianness(Endianness endianness)
        {
            _endiannessSwapper.Endianness = endianness;
        }

        public virtual void WriteInt16(short value)
        {
            _endiannessSwapper.WriteSwap(value, WriteInt16_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual void WriteUInt16(ushort value)
        {
            _endiannessSwapper.WriteSwap(value, WriteUInt16_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual void WriteInt32(int value)
        {
            _endiannessSwapper.WriteSwap(value, WriteInt32_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual void WriteUInt32(uint value)
        {
            _endiannessSwapper.WriteSwap(value, WriteUInt32_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual void WriteInt64(long value)
        {
            _endiannessSwapper.WriteSwap(value, WriteInt64_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual void WriteUInt64(ulong value)
        {
            _endiannessSwapper.WriteSwap(value, WriteUInt64_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual void WriteFloat(float value)
        {
            _endiannessSwapper.WriteSwap(value, WriteFloat_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual void WriteDouble(double value)
        {
            _endiannessSwapper.WriteSwap(value, WriteDouble_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual short GetInt16(int offset)
        {
            return _endiannessSwapper.GetSwap(offset, GetInt16_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual ushort GetUInt16(int offset)
        {
            return _endiannessSwapper.GetSwap(offset, GetUInt16_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual short ReadInt16()
        {
            return _endiannessSwapper.ReadSwap(ReadInt16_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual ushort ReadUInt16()
        {
            return _endiannessSwapper.ReadSwap(ReadUInt16_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual int GetInt32(int offset)
        {
            return _endiannessSwapper.GetSwap(offset, GetInt32_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual uint GetUInt32(int offset)
        {
            return _endiannessSwapper.GetSwap(offset, GetUInt32_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual int ReadInt32()
        {
            return _endiannessSwapper.ReadSwap(ReadInt32_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual uint ReadUInt32()
        {
            return _endiannessSwapper.ReadSwap(ReadUInt32_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual long GetInt64(int offset)
        {
            return _endiannessSwapper.GetSwap(offset, GetInt64_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual ulong GetUInt64(int offset)
        {
            return _endiannessSwapper.GetSwap(offset, GetUInt64_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual long ReadInt64()
        {
            return _endiannessSwapper.ReadSwap(ReadInt64_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual ulong ReadUInt64()
        {
            return _endiannessSwapper.ReadSwap(ReadUInt64_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual float GetFloat(int offset)
        {
            return _endiannessSwapper.GetSwap(offset, GetFloat_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual float ReadFloat()
        {
            return _endiannessSwapper.ReadSwap(ReadFloat_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual double GetDouble(int offset)
        {
            return _endiannessSwapper.GetSwap(offset, GetDouble_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual double ReadDouble()
        {
            return _endiannessSwapper.ReadSwap(ReadDouble_Implementation, _endiannessSwapper.SwapBytes);
        }

        public virtual short GetInt16(int offset, Endianness endianness)
        {
            return _endiannessSwapper.GetSwap(offset, GetInt16, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual ushort GetUInt16(int offset, Endianness endianness)
        {
            return _endiannessSwapper.GetSwap(offset, GetUInt16, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual short ReadInt16(Endianness endianness)
        {
            return _endiannessSwapper.ReadSwap(ReadInt16, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual ushort ReadUInt16(Endianness endianness)
        {
            return _endiannessSwapper.ReadSwap(ReadUInt16, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual int GetInt32(int offset, Endianness endianness)
        {
            return _endiannessSwapper.GetSwap(offset, GetInt32, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual uint GetUInt32(int offset, Endianness endianness)
        {
            return _endiannessSwapper.GetSwap(offset, GetUInt32, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual int ReadInt32(Endianness endianness)
        {
            return _endiannessSwapper.ReadSwap(ReadInt32, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual uint ReadUInt32(Endianness endianness)
        {
            return _endiannessSwapper.ReadSwap(ReadUInt32, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual long GetInt64(int offset, Endianness endianness)
        {
            return _endiannessSwapper.GetSwap(offset, GetInt64, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual ulong GetUInt64(int offset, Endianness endianness)
        {
            return _endiannessSwapper.GetSwap(offset, GetUInt64, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual long ReadInt64(Endianness endianness)
        {
            return _endiannessSwapper.ReadSwap(ReadInt64, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual ulong ReadUInt64(Endianness endianness)
        {
            return _endiannessSwapper.ReadSwap(ReadUInt64, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual float GetFloat(int offset, Endianness endianness)
        {
            return _endiannessSwapper.GetSwap(offset, GetFloat, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual float ReadFloat(Endianness endianness)
        {
            return _endiannessSwapper.ReadSwap(ReadFloat, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual double GetDouble(int offset, Endianness endianness)
        {
            return _endiannessSwapper.GetSwap(offset, GetDouble, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual double ReadDouble(Endianness endianness)
        {
            return _endiannessSwapper.ReadSwap(ReadDouble, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual void WriteString(string value)
        {
            WriteString(value, NoEncoding);
        }

        public virtual void WriteString(string value, Encoding encoding)
        {
            WriteString(value, encoding.GetBytes);
        }

        public virtual void WriteString(string value, Func<string, byte[]> converter)
        {
            byte[] bytes = converter(value);
            WriteBytes(bytes);
        }

        public virtual void WriteFixedString(string value, int length, Func<string, byte[]> converter)
        {
            byte[] bytes = converter(value);
            for (int i = 0; i < bytes.Length && i < length; i++)
            {
                WriteByte(bytes[i]);
            }

            int diff = length - bytes.Length;
            if (diff > 0)
            {
                WriteBytes(new byte[diff]);
            }
        }

        public virtual void WriteFixedString(string value, int length, Encoding encoding)
        {
            WriteFixedString(value, length, encoding.GetBytes);
        }

        public virtual void WriteFixedString(string value, int length)
        {
            WriteFixedString(value, length, NoEncoding);
        }

        public virtual void WriteCString(string value)
        {
            WriteString(value);
            WriteByte(0);
        }

        public virtual void WriteCString(string value, Encoding encoding)
        {
            WriteString(value, encoding);
            WriteByte(0);
        }

        public virtual void WriteCString(string value, Func<string, byte[]> converter)
        {
            WriteString(value, converter);
            WriteByte(0);
        }

        public virtual string ToHexString(string separator = null)
        {
            return Service.ToHexString(GetAllBytes(), separator);
        }

        public virtual string ToAsciiString(string separator = "  ")
        {
            return Service.ToAsciiString(GetAllBytes(), separator);
        }

        public virtual string GetString(int offset, int length)
        {
            return GetString(offset, length, NoEncoding);
        }

        public virtual string GetString(int offset, int length, Encoding encoding)
        {
            return GetString(offset, length, encoding.GetString);
        }

        public virtual string GetString(int offset, int length, Func<byte[], string> converter)
        {
            int position = Position;
            Position = offset;
            string value = ReadString(length, converter);
            Position = position;
            return value;
        }

        public virtual string ReadString(int length)
        {
            return ReadString(length, NoEncoding);
        }

        public virtual string ReadString(int length, Encoding encoding)
        {
            return ReadString(length, encoding.GetString);
        }

        public virtual string ReadString(int length, Func<byte[], string> converter)
        {
            byte[] bytes = ReadBytes(length);
            return converter(bytes);
        }

        public virtual string ReadFixedString(int length)
        {
            return ReadFixedString(length, NoEncoding);
        }

        public virtual string ReadFixedString(int length, Encoding encoding)
        {
            return ReadFixedString(length, encoding.GetString);
        }

        public virtual string ReadFixedString(int length, Func<byte[], string> converter)
        {
            byte[] bytes = ReadBytesFixedZeroTerminated(length);
            return converter(bytes);
        }

        public virtual string GetCString(int offset)
        {
            return GetCString(offset, NoEncoding);
        }

        public virtual string GetCString(int offset, Encoding encoding)
        {
            return GetCString(offset, encoding.GetString);
        }

        public virtual string GetCString(int offset, Func<byte[], string> converter)
        {
            int position = Position;
            Position = offset;
            string value = ReadCString(converter);
            Position = position;
            return value;
        }

        public virtual byte[] ReadBytesZeroTerminated()
        {
            return ReadBytesTerminated(0);
        }

        public virtual byte[] ReadBytesFixedZeroTerminated(int length)
        {
            return ReadBytesFixedTerminated(length, 0);
        }

        public virtual byte[] ReadBytesTerminated(byte termination)
        {
            List<byte> readBytes = new List<byte>();
            while (Position < Size)
            {
                byte b = ReadByte();
                if (b != termination)
                {
                    readBytes.Add(b);
                }
                else
                {
                    break;
                }
            }

            return readBytes.ToArray();
        }

        public virtual byte[] ReadBytesFixedTerminated(int length, byte termination)
        {
            byte[] bytes = ReadBytes(length);
            List<byte> readBytes = new List<byte>();
            foreach (byte b in bytes)
            {
                if (b != termination)
                {
                    readBytes.Add(b);
                }
                else
                {
                    break;
                }
            }

            return readBytes.ToArray();
        }

        public virtual string ReadCString()
        {
            return ReadCString(NoEncoding);
        }

        public virtual string ReadCString(Encoding encoding)
        {
            return ReadCString(encoding.GetString);
        }

        public virtual string ReadCString(Func<byte[], string> converter)
        {
            byte[] bytes = ReadBytesZeroTerminated();
            return converter(bytes);
        }

        public virtual void WriteBuffer(IBuffer value, int offset, int length)
        {
            WriteBytes(value.GetBytes(offset, length));
        }

        public virtual void WriteBuffer(IBuffer value)
        {
            WriteBytes(value.GetAllBytes());
        }

        public virtual IBuffer Clone(int length)
        {
            return Clone(0, length);
        }

        public virtual IBuffer Clone()
        {
            return Clone(Size);
        }

        public virtual void WriteInt16(short value, Endianness endianness)
        {
            _endiannessSwapper.WriteSwap(value, WriteInt16, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual void WriteInt32(int value, Endianness endianness)
        {
            _endiannessSwapper.WriteSwap(value, WriteInt32, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual void WriteInt64(long value, Endianness endianness)
        {
            _endiannessSwapper.WriteSwap(value, WriteInt64, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual void WriteUInt16(ushort value, Endianness endianness)
        {
            _endiannessSwapper.WriteSwap(value, WriteUInt16, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual void WriteUInt32(uint value, Endianness endianness)
        {
            _endiannessSwapper.WriteSwap(value, WriteUInt32, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual void WriteUInt64(ulong value, Endianness endianness)
        {
            _endiannessSwapper.WriteSwap(value, WriteUInt64, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual void WriteFloat(float value, Endianness endianness)
        {
            _endiannessSwapper.WriteSwap(value, WriteFloat, _endiannessSwapper.SwapBytes, endianness);
        }

        public virtual void WriteDouble(double value, Endianness endianness)
        {
            _endiannessSwapper.WriteSwap(value, WriteDouble, _endiannessSwapper.SwapBytes, endianness);
        }

        public string Dump()
        {
            return ToAsciiString() +
                   Environment.NewLine +
                   ToHexString();
        }

        public override string ToString()
        {
            return $"Size:{Size} Position:{Position}";
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}