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

namespace Arrowgene.Services.Network.TCP.Client
{
    using Common.Buffers;
    using Logging;
    using System;
    using Server;
    using System.Net;

    public abstract class TCPClient : ITCPClient
    {
        public const string Name = "TCP Client";

        public IPAddress RemoteIpAddress { get; }
        public int Port { get; }
        public ILogger Logger { get; }
        public ITCPSocket Socket { get; }
        public event EventHandler<DisconnectedEventArgs> Disconnected;
        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<ReceivedPacketEventArgs> ReceivedPacket;
        public event EventHandler<ConnectErrorEventArgs> ConnectError;

        public abstract void Connect(IPAddress serverIPAddress, int serverPort, TimeSpan timeout);

        public abstract void Disconnect();

        public abstract void Send(byte[] payload);

        protected virtual void OnClientReceivedPacket(ITCPClient clientSocket, ByteBuffer payload)
        {
            EventHandler<ReceivedPacketEventArgs> receivedPacket = this.ReceivedPacket;
            if (receivedPacket != null)
            {
                ReceivedPacketEventArgs ReceivedPacketEventArgs = new ReceivedPacketEventArgs(this, payload);
                receivedPacket(this, ReceivedPacketEventArgs);
            }
        }

        protected virtual void OnDisconnected()
        {
            EventHandler<DisconnectedEventArgs> clientDisconnected = this.Disconnected;
            if (clientDisconnected != null)
            {
                DisconnectedEventArgs clientDisconnectedEventArgs = new DisconnectedEventArgs(this);
                clientDisconnected(this, clientDisconnectedEventArgs);
            }
        }

        protected virtual void OnConnected()
        {
            EventHandler<ConnectedEventArgs> clientConnected = this.Connected;
            if (clientConnected != null)
            {
                ConnectedEventArgs clientConnectedEventArgs = new ConnectedEventArgs(this);
                clientConnected(this, clientConnectedEventArgs);
            }
        }

        protected virtual void OnConnectError(string reason, IPAddress serverIPAddress, int serverPort, TimeSpan timeout)
        {
            EventHandler<ConnectErrorEventArgs> connectError = this.ConnectError;
            if (connectError != null)
            {
                ConnectErrorEventArgs connectErrorEventArgs = new ConnectErrorEventArgs(reason, serverIPAddress, serverPort, timeout);
                connectError(this, connectErrorEventArgs);
            }
        }
    }
}