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

namespace Arrowgene.Services.Common.Buffers
{
    using System;
    using System.Text;

    public class BBuffer : Buffer
    {
        private const int BUFFER_SIZE = 1024;

        private byte[] _buffer;
        private int _size;
        private int _currentPos;

        public BBuffer()
        {
            _buffer = new byte[BUFFER_SIZE];
            _size = 0;
            _currentPos = 0;
        }

        public BBuffer(int len)
        {
            _buffer = new byte[len];
            _size = 0;
            _currentPos = 0;
        }
        
        public BBuffer(byte[] data)
        {
            _buffer = data;
            _size = _buffer.Length;
            _currentPos = 0;
        }

        public override int Size
        {
            get { return _size; }
        }

        public override int Position
        {
            get { return _currentPos; }
            set { SetCurrentPos(value); }
        }

        public override void SetPositionStart()
        {
            SetCurrentPos(0);
        }

        public override void SetPositionEnd()
        {
            SetCurrentPos(_size);
        }

        public override IBuffer Clone(int offset, int length)
        {
            return new BBuffer(GetBytes(offset, length));
        }

        public override byte[] GetAllBytes()
        {
            byte[] bytes = new byte[_size];
            System.Buffer.BlockCopy(_buffer, 0, bytes, 0, _size);
            return bytes;
        }

        public override byte[] GetAllBytes(int offset)
        {
            return GetBytes(offset, _size - offset);
        }

        public override void WriteBytes(byte[] bytes)
        {
            ExtendBufferIfNecessary(bytes.Length);
            foreach (byte b in bytes)
            {
                _buffer[_currentPos++] = b;
            }
            UpdateSizeForPosition(_currentPos);
        }

        public override void WriteBytes(byte[] bytes, int offset, int length)
        {
            ExtendBufferForOffsetIfNecessary(length, offset);
            for (int i = 0; i < length; i++)
            {
                _buffer[offset++] = bytes[i];
            }
            UpdateSizeForPosition(_currentPos);
        }

        public override void WriteByte(byte value)
        {
            WriteBytes(new byte[] {value});
        }

        public override void WriteByte(int value)
        {
            WriteByte((byte) value);
        }

        public override void WriteByte(long value)
        {
            WriteByte((byte) value);
        }

        public override void WriteInt16(short value)
        {
            WriteByte(value & 0xff);
            WriteByte((value & 0xff00) >> 8);
        }

        public override void WriteInt16(int value)
        {
            WriteInt16((short) value);
        }

        public override void WriteInt32(int value)
        {
            WriteByte(value & 0xff);
            WriteByte((value & 0xff00) >> 8);
            WriteByte((value & 0xff0000) >> 16);
            WriteByte((value & 0xff000000) >> 24);
        }

        public override void WriteFloat(float value)
        {
            throw new NotImplementedException();
        }

        public override void WriteString(string value)
        {
            foreach (char c in value)
            {
                WriteByte(c);
            }
        }

        public override void WriteFixedString(string value, int length)
        {
            for (int i = 0; i < length; i++)
            {
                WriteByte(value[i]);
            }
            int diff = length - value.Length;
            if (diff > 0)
            {
                WriteBytes(new byte[diff]);
            }
        }

        public override void WriteCString(string value)
        {
            WriteString(value);
            WriteByte(0);
        }

        public override byte ReadByte()
        {
            byte b = GetByte(_currentPos);
            _currentPos++;
            return b;
        }

        public override byte GetByte(int offset)
        {
            return _buffer[offset];
        }

        public override byte[] ReadBytes(int length)
        {
            byte[] data = GetBytes(_currentPos, length);
            // Advance to skip read bytes
            _currentPos += length;
            return data;
        }

        public override byte[] GetBytes(int offset, int length)
        {
            byte[] data = new byte[length];
            for (int i = 0; i < length; i++)
            {
                data[i] = GetByte(offset++);
            }
            return data;
        }

        public override short GetInt16(int offset)
        {
            short value = (short) (_buffer[offset++] & 0xff);
            value += (short) ((_buffer[offset] & 0xff) << 8);
            return value;
        }

        public override short ReadInt16()
        {
            short value = GetInt16(_currentPos);
            // Advance to skip read bytes
            _currentPos += 2;
            return value;
        }

        public override int GetInt32(int offset)
        {
            int value = _buffer[offset++] & 0xff;
            value += (_buffer[offset++] & 0xff) << 8;
            value += (_buffer[offset++] & 0xff) << 16;
            value += (_buffer[offset] & 0xff) << 24;
            return value;
        }

        public override int ReadInt32()
        {
            int value = GetInt32(_currentPos);
            // Advance to skip read bytes
            _currentPos += 4;
            return value;
        }

        public override float GetFloat(int offset)
        {
            throw new NotImplementedException();
        }

        public override float ReadFloat()
        {
            throw new NotImplementedException();
        }

        public override string GetString(int offset, int length)
        {
            StringBuilder sb = new StringBuilder();
            if (offset + length <= _size)
            {
                for (int i = 0; i < length; i++)
                {
                    sb.Append((char) _buffer[offset++]);
                }
            }
            return sb.ToString();
        }

        public override string ReadString(int length)
        {
            string str = GetString(_currentPos, length);
            // Advance to skip read bytes
            _currentPos += length;
            return str;
        }

        public override string ReadCString()
        {
            string str = GetCString(_currentPos);
            // Advance to skip read bytes
            _currentPos += str.Length;
            // Advance to skip the nul-byte
            _currentPos++;
            return str;
        }

        public override string GetCString(int offset)
        {
            int len = GetLengthTillNulTermination(offset);
            return GetString(offset, len);
        }

        private void SetCurrentPos(int newCurrentPos)
        {
            ExtendBufferForPositionIfNecessary(newCurrentPos);
            _currentPos = newCurrentPos;
        }

        private void ExtendBufferIfNecessary(int length)
        {
            int bLength = _buffer.Length;
            if (length + _currentPos >= bLength)
            {
                int newSize = length + bLength + BUFFER_SIZE;
                ExtendBuffer(newSize);
            }
        }

        private void ExtendBufferForPositionIfNecessary(int position)
        {
            int bLength = _buffer.Length;
            if (position >= bLength)
            {
                int newSize = position + bLength + BUFFER_SIZE;
                ExtendBuffer(newSize);
            }
            UpdateSizeForPosition(position);
        }

        private void ExtendBufferForOffsetIfNecessary(int offset, int length)
        {
            int bLength = _buffer.Length;
            int tLength = offset + length;
            if (tLength >= bLength)
            {
                int newSize = tLength + BUFFER_SIZE;
                ExtendBuffer(newSize);
            }
        }

        private void ExtendBuffer(int newSize)
        {
            byte[] extension = new byte[newSize];
            System.Buffer.BlockCopy(_buffer, 0, extension, 0, _size);
            _buffer = extension;
        }

        private int GetLengthTillNulTermination(int offset)
        {
            int len = 0;
            while (offset + len <= _size && _buffer[offset + len] != 0)
            {
                len++;
            }
            return len;
        }

        private void UpdateSizeForPosition(int position)
        {
            if (position > _size)
            {
                _size = position;
            }
        }
    }
}