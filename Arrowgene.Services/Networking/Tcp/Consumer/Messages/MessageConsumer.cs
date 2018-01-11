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
using Arrowgene.Services.Buffers;
using Arrowgene.Services.Serialization;

namespace Arrowgene.Services.Networking.Tcp.Consumer.Messages
{
    public class MessageConsumer : IConsumer, IMessageSerializer
    {
        private const int HeaderSize = 4;

        private readonly BinaryFormatterSerializer<Message> _serializer;
        private readonly Dictionary<int, IMessageHandle> _handles;
        private readonly Dictionary<ITcpSocket, MessageState> _serverStates;

        public MessageConsumer()
        {
            _handles = new Dictionary<int, IMessageHandle>();
            _serializer = new BinaryFormatterSerializer<Message>();
            _serverStates = new Dictionary<ITcpSocket, MessageState>();
        }

        public void AddHandle(IMessageHandle handle)
        {
            if (_handles.ContainsKey(handle.Id))
            {
                throw new Exception(string.Format("Handle for id: {0} already defined.", handle.Id));
            }
            handle.SetMessageSerializer(this);
            _handles.Add(handle.Id, handle);
        }

        public byte[] Serialize(Message message)
        {
            IBuffer buffer = new StreamBuffer();
            byte[] data = _serializer.Serialize(message);
            buffer.WriteInt32(data.Length);
            buffer.WriteBytes(data);
            return buffer.GetAllBytes();
        }

        public virtual void OnStart()
        {
            _serverStates.Clear();
        }

        public virtual void OnReceivedData(ITcpSocket socket, byte[] data)
        {
            MessageState state;
            if (_serverStates.ContainsKey(socket))
            {
                state = _serverStates[socket];
            }
            else
            {
                state = new MessageState();
            }
            state.Data = data;
            List<Message> messages = Deserialize(state);
            foreach (Message message in messages)
            {
                if (_handles.ContainsKey(message.Id))
                {
                    _handles[message.Id].Process(message, socket);
                }
            }
        }

        public virtual void OnClientDisconnected(ITcpSocket socket)
        {
            if (socket != null)
            {
                _serverStates.Remove(socket);
            }
        }

        public virtual void OnClientConnected(ITcpSocket socket)
        {
        }

        public virtual void OnStop()
        {
        }

        private List<Message> Deserialize(MessageState state)
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