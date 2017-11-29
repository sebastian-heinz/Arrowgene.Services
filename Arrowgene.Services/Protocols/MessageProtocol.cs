using System;
using System.Collections.Generic;
using Arrowgene.Services.Messages;

namespace Arrowgene.Services.Protocols
{
    public class MessageProtocol
    {
        private const int HeaderSize = 8;

        private int _currentLength;
        private int _currentId;
        private byte[] _currentBuffer;

        private List<byte[]> Read(byte[] data)
        {
            List<byte[]> results = new List<byte[]>();
            if (_currentBuffer != null)
            {
                byte[] tmp = new byte[data.Length + _currentBuffer.Length];
                Buffer.BlockCopy(_currentBuffer, 0, tmp, 0, _currentBuffer.Length);
                Buffer.BlockCopy(data, 0, tmp, _currentBuffer.Length, data.Length);
                data = tmp;
            }
            else
            {
                _currentLength = GetInt32(data, 0);
                _currentId = GetInt32(data, 4);
            }
            if (_currentLength == data.Length)
            {
                results.Add(data);
                _currentBuffer = null;
            }
            else if (_currentLength > data.Length)
            {
                _currentBuffer = data;
            }
            else if (_currentLength < data.Length)
            {
                bool read = true;
                while (read)
                {
                    byte[] result = new byte[_currentLength];
                    Buffer.BlockCopy(data, 0, result, 0, _currentLength);
                    results.Add(result);
                    int remaining = data.Length - _currentLength;
                    byte[] tmp = new byte[remaining];
                    Buffer.BlockCopy(data, _currentLength, tmp, 0, remaining);
                    data = tmp;
                    if (remaining >= HeaderSize)
                    {
                        _currentLength = GetInt32(data, 0);
                        _currentId = GetInt32(data, 4);
                        if (_currentLength == data.Length)
                        {
                            results.Add(data);
                            _currentBuffer = null;
                            read = false;
                        }
                        else if (_currentLength > data.Length)
                        {
                            _currentBuffer = data;
                            read = false;
                        }
                    }
                    else
                    {
                        _currentBuffer = data;
                        read = false;
                    }
                }
            }
            return results;
        }

        public byte[] Write(byte[] data, Message message)
        {
            int length = data.Length + HeaderSize;
            byte[] result = new byte[length];
            WriteInt32(result, length, 0);
            WriteInt32(result, message.Id, 4);
            Buffer.BlockCopy(data, 0, result, 8, data.Length);
            return result;
        }

        public int GetInt32(byte[] buffer, int offset)
        {
            int value = buffer[offset++] & 0xff;
            value += (buffer[offset++] & 0xff) << 8;
            value += (buffer[offset++] & 0xff) << 16;
            value += (buffer[offset] & 0xff) << 24;
            return value;
        }

        public void WriteInt32(byte[] buffer, int value, int offset)
        {
            buffer[offset++] = (byte) (value & 0xff);
            buffer[offset++] = (byte) ((value & 0xff00) >> 8);
            buffer[offset++] = (byte) ((value & 0xff0000) >> 16);
            buffer[offset] = (byte) ((value & 0xff000000) >> 24);
        }
    }
}