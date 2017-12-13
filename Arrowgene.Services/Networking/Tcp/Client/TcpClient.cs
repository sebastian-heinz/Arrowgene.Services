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
using System.Net;
using Arrowgene.Services.Networking.Tcp.Consumer;

namespace Arrowgene.Services.Networking.Tcp.Client
{
    public abstract class TcpClient : ITcpClient
    {
        private readonly IConsumer _consumer;

        protected TcpClient(IConsumer consumer)
        {
            _consumer = consumer;
        }

        public event EventHandler<ConnectErrorEventArgs> ConnectError;

        public IPAddress RemoteIpAddress { get; }
        public int Port { get; }

        public abstract void Connect(IPAddress serverIpAddress, int serverPort, TimeSpan timeout);
        public abstract void Close();
        public abstract void Send(byte[] payload);


        protected void OnReceivedData(ITcpSocket socket, byte[] data)
        {
            _consumer.OnReceivedData(socket, data);
        }

        protected void OnClientDisconnected(ITcpSocket socket)
        {
            _consumer.OnClientDisconnected(socket);
        }

        protected void OnClientConnected(ITcpSocket socket)
        {
            _consumer.OnClientConnected(socket);
        }

        protected void OnConnectError(ITcpClient client, string reason, IPAddress serverIpAddress, int serverPort, TimeSpan timeout)
        {
            EventHandler<ConnectErrorEventArgs> connectError = ConnectError;
            if (connectError != null)
            {
                ConnectErrorEventArgs connectErrorEventArgsEventArgs = new ConnectErrorEventArgs(client, reason, serverIpAddress, serverPort, timeout);
                connectError(this, connectErrorEventArgsEventArgs);
            }
        }
    }
}