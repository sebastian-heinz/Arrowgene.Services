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
using Arrowgene.Services.Networking.Tcp.Protocols;
using Arrowgene.Services.Protocols.Messages;

namespace Arrowgene.Services.Protocols.Messages
{
    public class MessageProtocol<T> : IProtocol
    {
        private const int HeaderSize = 4;

        private BinaryFormatterSerializer<Message> _serializer;
        private Dictionary<T, MessageState<T>> _states;

        public MessageProtocol()
        {
            _serializer = new BinaryFormatterSerializer<Message>();
            _states = new Dictionary<T, MessageState<T>>();
        }

        public byte[] Serialize(Message message)
        {
            IBuffer buffer = new StreamBuffer();
            byte[] data = _serializer.Serialize(message);
            buffer.WriteInt32(data.Length);
            buffer.WriteBytes(data);
            return buffer.GetAllBytes();
        }
        
        public List<Message> Deserialize(byte[] data, T user)
        {
            MessageState<T> state;
            if (_states.ContainsKey(user))
            {
                state = _states[user];
            }
            else
            {
                state = new MessageState<T>();
            }
            state.Data = data;
            return Deserialize(state);
        }

        public List<Message> Deserialize(MessageState<T> state)
        {
            List<Message> messages = new List<Message>();
            if (state.CurrentBuffer == null)
            {
                state.CurrentBuffer = new StreamBuffer(state.Data);
                state.CurrentBuffer.SetPositionStart();
            }
            else
            {
                state.CurrentBuffer.SetPositionEnd();
                state.CurrentBuffer.WriteBytes(state.Data);
                state.CurrentBuffer.SetPositionStart();
            }
            while (state.CurrentBuffer.Position < state.CurrentBuffer.Size)
            {
                if (state.CurrentBuffer.Position + HeaderSize > state.CurrentBuffer.Size)
                {
                    break;
                }
                int length;
                if (state.ReadLength)
                {
                    length = state.CurrentLength;
                }
                else
                {
                    length = state.CurrentBuffer.ReadInt32();
                    state.CurrentLength = length;
                    state.ReadLength = true;
                }
                if (length > state.CurrentBuffer.Size)
                {
                    break;
                }
                state.ReadLength = false;
                byte[] messageBytes = state.CurrentBuffer.ReadBytes(length);
                Message message = _serializer.Deserialize(messageBytes);
                messages.Add(message);
            }
            return messages;
        }
    }
}