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


using System.Collections.Generic;
using Arrowgene.Services.Buffers;

namespace Arrowgene.Services.Protocols.Messages
{
    public class MessageProtocol : IProtocol<List<Message>>
    {
        private const int HeaderSize = 4;

        private BinaryFormatterSerializer<Message> _serializer;

        private int _currentLength;
        private IBuffer _currentBuffer;
        private bool _readLength;

        public MessageProtocol()
        {
            _serializer = new BinaryFormatterSerializer<Message>();
        }

        public byte[] Serialize(List<Message> messages)
        {
            IBuffer buffer = new StreamBuffer();
            foreach (var message in messages)
            {
                byte[] data = _serializer.Serialize(message);
                buffer.WriteInt32(data.Length);
                buffer.WriteBytes(data);
            }
            return buffer.GetAllBytes();
        }

        public List<Message> Deserialize(byte[] data)
        {
            List<Message> messages = new List<Message>();
            if (_currentBuffer == null)
            {
                _currentBuffer = new StreamBuffer(data);
                _currentBuffer.SetPositionStart();
            }
            else
            {
                _currentBuffer.SetPositionEnd();
                _currentBuffer.WriteBytes(data);
                _currentBuffer.SetPositionStart();
            }
            while (_currentBuffer.Position < _currentBuffer.Size)
            {
                if (_currentBuffer.Position + HeaderSize > _currentBuffer.Size)
                {
                    break;
                }
                int length;
                if (_readLength)
                {
                    length = _currentLength;
                }
                else
                {
                    length = _currentBuffer.ReadInt32();
                }
                if (length > _currentBuffer.Size)
                {
                    _currentLength = length;
                    _readLength = true;
                    break;
                }
                _readLength = false;
                byte[] messageBytes = _currentBuffer.ReadBytes(length);
                Message message = _serializer.Deserialize(messageBytes);
                messages.Add(message);
            }
            return messages;
        }
    }
}