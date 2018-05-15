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

        public static bool SwapNeeded(Endianness endianness)
        {
            return (BitConverter.IsLittleEndian && endianness == Endianness.Big)
                   || (!BitConverter.IsLittleEndian && endianness == Endianness.Little);
        }

        public static ushort SwapBytes(ushort x)
        {
            return (ushort) ((x >> 8) | (x << 8));
        }

        public static uint SwapBytes(uint x)
        {
            x = (x >> 16) | (x << 16);
            return ((x & 0xFF00FF00) >> 8) | ((x & 0x00FF00FF) << 8);
        }

        public static ulong SwapBytes(ulong x)
        {
            x = (x >> 32) | (x << 32);
            x = ((x & 0xFFFF0000FFFF0000) >> 16) | ((x & 0x0000FFFF0000FFFF) << 16);
            return ((x & 0xFF00FF00FF00FF00) >> 8) | ((x & 0x00FF00FF00FF00FF) << 8);
        }

        public static float SwapBytes(float input)
        {
            byte[] tmpIn = BitConverter.GetBytes(input);
            byte[] tmpOut = new byte[4];
            tmpOut[0] = tmpIn[3];
            tmpOut[1] = tmpIn[2];
            tmpOut[2] = tmpIn[1];
            tmpOut[3] = tmpIn[0];
            return BitConverter.ToSingle(tmpOut, 0);
        }

        public static double SwapBytes(double input)
        {
            byte[] tmpIn = BitConverter.GetBytes(input);
            byte[] tmpOut = new byte[8];
            tmpOut[0] = tmpIn[7];
            tmpOut[1] = tmpIn[6];
            tmpOut[2] = tmpIn[5];
            tmpOut[3] = tmpIn[4];
            tmpOut[4] = tmpIn[3];
            tmpOut[5] = tmpIn[2];
            tmpOut[6] = tmpIn[1];
            tmpOut[7] = tmpIn[0];
            return BitConverter.ToSingle(tmpOut, 0);
        }

        public static short SwapBytes(short value)
        {
            return (short) SwapBytes((ushort) value);
        }

        public static int SwapBytes(int value)
        {
            return (int) SwapBytes((uint) value);
        }

        public static long SwapBytes(long value)
        {
            return (long) SwapBytes((ulong) value);
        }

        public abstract int Size { get; }
        public abstract int Position { get; set; }
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
        public abstract void WriteInt16(short value);
        public abstract void WriteInt16(ushort value);
        public abstract void WriteInt32(int value);
        public abstract void WriteInt32(uint value);
        public abstract void WriteInt64(long value);
        public abstract void WriteInt64(ulong value);
        public abstract void WriteFloat(float value);
        public abstract void WriteDouble(double value);
        public abstract void WriteDecimal(decimal value);
        public abstract byte ReadByte();
        public abstract byte GetByte(int offset);
        public abstract byte[] ReadBytes(int length);
        public abstract byte[] GetBytes(int offset, int length);
        public abstract short GetInt16(int offset);
        public abstract ushort GetUInt16(int offset);
        public abstract short ReadInt16();
        public abstract ushort ReadUInt16();
        public abstract int GetInt32(int offset);
        public abstract uint GetUInt32(int offset);
        public abstract int ReadInt32();
        public abstract uint ReadUInt32();
        public abstract long GetInt64(int offset);
        public abstract ulong GetUInt64(int offset);
        public abstract long ReadInt64();
        public abstract ulong ReadUInt64();
        public abstract float GetFloat(int offset);
        public abstract float ReadFloat();
        public abstract double GetDouble(int offset);
        public abstract double ReadDouble();
        public abstract decimal GetDecimal(int offset);
        public abstract decimal ReadDecimal();

        public virtual short GetInt16(int offset, Endianness endianness)
        {
            return GetSwap(offset, GetInt16, SwapBytes, endianness);
        }

        public virtual ushort GetUInt16(int offset, Endianness endianness)
        {
            return GetSwap(offset, GetUInt16, SwapBytes, endianness);
        }

        public virtual short ReadInt16(Endianness endianness)
        {
            return ReadSwap(ReadInt16, SwapBytes, endianness);
        }

        public virtual ushort ReadUInt16(Endianness endianness)
        {
            return ReadSwap(ReadUInt16, SwapBytes, endianness);
        }

        public virtual int GetInt32(int offset, Endianness endianness)
        {
            return GetSwap(offset, GetInt32, SwapBytes, endianness);
        }

        public virtual uint GetUInt32(int offset, Endianness endianness)
        {
            return GetSwap(offset, GetUInt32, SwapBytes, endianness);
        }

        public virtual int ReadInt32(Endianness endianness)
        {
            return ReadSwap(ReadInt32, SwapBytes, endianness);
        }

        public virtual uint ReadUInt32(Endianness endianness)
        {
            return ReadSwap(ReadUInt32, SwapBytes, endianness);
        }

        public virtual long GetInt64(int offset, Endianness endianness)
        {
            return GetSwap(offset, GetInt64, SwapBytes, endianness);
        }

        public virtual ulong GetUInt64(int offset, Endianness endianness)
        {
            return GetSwap(offset, GetUInt64, SwapBytes, endianness);
        }

        public virtual long ReadInt64(Endianness endianness)
        {
            return ReadSwap(ReadInt64, SwapBytes, endianness);
        }

        public virtual ulong ReadUInt64(Endianness endianness)
        {
            return ReadSwap(ReadUInt64, SwapBytes, endianness);
        }

        public virtual float GetFloat(int offset, Endianness endianness)
        {
            return GetSwap(offset, GetFloat, SwapBytes, endianness);
        }

        public virtual float ReadFloat(Endianness endianness)
        {
            return ReadSwap(ReadFloat, SwapBytes, endianness);
        }

        public virtual double GetDouble(int offset, Endianness endianness)
        {
            return GetSwap(offset, GetDouble, SwapBytes, endianness);
        }

        public virtual double ReadDouble(Endianness endianness)
        {
            return ReadSwap(ReadDouble, SwapBytes, endianness);
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
            WriteString(value, encoding.GetBytes);
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

        public virtual string ToHexString(char? seperator = null)
        {
            byte[] buffer = GetAllBytes();
            StringBuilder sb = new StringBuilder();
            int len = buffer.Length;
            for (int i = 0; i < len; i++)
            {
                sb.Append(buffer[i].ToString("X2"));
                if (seperator != null && i < len - 1)
                {
                    sb.Append(seperator);
                }
            }

            return sb.ToString();
        }

        public virtual string ToAsciiString(bool spaced)
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

        public virtual void WriteInt16(short value, Endianness endianness)
        {
            WriteSwap(value, WriteInt16, SwapBytes, endianness);
        }

        public virtual void WriteInt32(int value, Endianness endianness)
        {
            WriteSwap(value, WriteInt32, SwapBytes, endianness);
        }

        public virtual void WriteInt64(long value, Endianness endianness)
        {
            WriteSwap(value, WriteInt64, SwapBytes, endianness);
        }

        public virtual void WriteInt16(ushort value, Endianness endianness)
        {
            WriteSwap(value, WriteInt16, SwapBytes, endianness);
        }

        public virtual void WriteInt32(uint value, Endianness endianness)
        {
            WriteSwap(value, WriteInt32, SwapBytes, endianness);
        }

        public virtual void WriteInt64(ulong value, Endianness endianness)
        {
            WriteSwap(value, WriteInt64, SwapBytes, endianness);
        }

        public virtual void WriteFloat(float value, Endianness endianness)
        {
            WriteSwap(value, WriteFloat, SwapBytes, endianness);
        }

        public virtual void WriteDouble(double value, Endianness endianness)
        {
            WriteSwap(value, WriteDouble, SwapBytes, endianness);
        }

        public string Dump()
        {
            return ToAsciiString(true) +
                   Environment.NewLine +
                   ToHexString();
        }

        public override string ToString()
        {
            return string.Format("Size:{0} Position:{1}", Size, Position);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        protected T GetSwap<T>(int offset, Func<int, T> getFunction, Func<T, T> swapFunction, Endianness endianness)
        {
            T value = getFunction(offset);
            if (SwapNeeded(endianness))
            {
                value = swapFunction(value);
            }

            return value;
        }

        protected T ReadSwap<T>(Func<T> readFunction, Func<T, T> swapFunction, Endianness endianness)
        {
            T value = readFunction();
            if (SwapNeeded(endianness))
            {
                value = swapFunction(value);
            }

            return value;
        }

        protected void WriteSwap<T>(T value, Action<T> writeFunction, Func<T, T> swapFunction, Endianness endianness)
        {
            if (SwapNeeded(endianness))
            {
                value = swapFunction(value);
            }

            writeFunction(value);
        }
    }
}