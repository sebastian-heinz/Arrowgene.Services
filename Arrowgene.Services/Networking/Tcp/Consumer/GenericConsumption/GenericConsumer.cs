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

namespace Arrowgene.Services.Networking.Tcp.Consumer.GenericConsumption
{
    public class GenericConsumer<T> : IConsumer
    {
        private const int HeaderSize = 4;

        private readonly BinaryFormatterSerializer<T> _serializer;
        private readonly Dictionary<ITcpSocket, GenericState> _serverStates;

        public GenericConsumer()
        {
            _serializer = new BinaryFormatterSerializer<T>();
            _serverStates = new Dictionary<ITcpSocket, GenericState>();
        }

        public event EventHandler<ReceivedGenericEventArgs<T>> ReceivedGeneric;

        public byte[] Serialize(T generic)
        {
            IBuffer buffer = new StreamBuffer();
            byte[] data = _serializer.Serialize(generic);
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
            GenericState state;
            if (_serverStates.ContainsKey(socket))
            {
                state = _serverStates[socket];
            }
            else
            {
                state = new GenericState();
            }

            state.Data = data;
            List<T> generics = Deserialize(state);
            foreach (T generic in generics)
            {
                OnReceivedGeneric(socket, generic);
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

        protected virtual void OnReceivedGeneric(ITcpSocket socket, T generic)
        {
            EventHandler<ReceivedGenericEventArgs<T>> receivedGeneric = ReceivedGeneric;
            if (receivedGeneric != null)
            {
                ReceivedGenericEventArgs<T> receivedGenericEventArgs = new ReceivedGenericEventArgs<T>(socket, generic);
                receivedGeneric(this, receivedGenericEventArgs);
            }
        }

        private List<T> Deserialize(GenericState state)
        {
            List<T> generics = new List<T>();
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
                T generic = _serializer.Deserialize(messageBytes);
                generics.Add(generic);
            }

            return generics;
        }
    }
}