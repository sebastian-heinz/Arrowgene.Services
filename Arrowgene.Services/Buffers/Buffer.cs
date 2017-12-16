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
using System.Text;

namespace Arrowgene.Services.Buffers
{
    public abstract class Buffer : IBuffer, IBufferProvider, ICloneable
    {
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
        public abstract void WriteByte(int value);
        public abstract void WriteByte(long value);
        public abstract void WriteBytes(byte[] bytes);
        public abstract void WriteBytes(byte[] source, int srcOffset, int length);
        public abstract void WriteBytes(byte[] source, int srcOffset, int dstOffset, int count);
        public abstract void WriteInt16(short value);
        public abstract void WriteInt16(int value);
        public abstract void WriteInt32(int value);
        public abstract void WriteFloat(float value);
        public abstract void WriteString(string value);
        public abstract void WriteFixedString(string value, int length);
        public abstract void WriteCString(string value);
        public abstract byte ReadByte();
        public abstract byte GetByte(int offset);
        public abstract byte[] ReadBytes(int length);
        public abstract byte[] GetBytes(int offset, int length);
        public abstract short GetInt16(int offset);
        public abstract short ReadInt16();
        public abstract int GetInt32(int offset);
        public abstract int ReadInt32();
        public abstract float GetFloat(int offset);
        public abstract float ReadFloat();
        public abstract string GetString(int offset, int length);
        public abstract string ReadString(int length);
        public abstract string ReadCString();
        public abstract string GetCString(int offset);

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

        public string Dump()
        {
            return ToAsciiString(true) +
                   Environment.NewLine +
                   ToHexString();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public override string ToString()
        {
            return string.Format("Size:{0} Position:{1}", Size, Position);
        }
    }
}