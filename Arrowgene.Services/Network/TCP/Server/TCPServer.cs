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

namespace Arrowgene.Services.Network.TCP.Server
{
    using Common.Buffers;
    using Logging;
    using System;
    using System.Net;

    public abstract class TCPServer : ITCPServer
    {
        private const String TCP_SERVER = "TCP Server";

        public TCPServer(IPAddress ipAddress, int port, ILogger logger)
        {
            IPAddress = ipAddress;
            Port = port;
            Logger = logger;
        }

        public TCPServer(int port, ILogger logger) : this(IPAddress.Any, port, logger)
        {
        }

        public TCPServer(int port) : this(IPAddress.Any, port, new Logger(TCP_SERVER))
        {
        }

        /// <summary>
        /// Current logging instance where logs get written to.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Servers <see cref="System.Net.IPAddress"/>.
        /// </summary>
        public IPAddress IPAddress { get; }

        /// <summary>
        /// Servers port.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Occures when a client disconnected.
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> ClientDisconnected;

        /// <summary>
        /// Occures when a client connected.
        /// </summary>
        public event EventHandler<ConnectedEventArgs> ClientConnected;

        /// <summary>
        /// Occures when a packet is received.
        /// </summary>
        public event EventHandler<ReceivedPacketEventArgs> ReceivedPacket;

        /// <summary>
        /// Start accepting connections,
        /// Creates a new <see cref="Arrowgene.Services.Logging.Logger"/> instance if none is set.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Stops the server.
        /// </summary>
        public abstract void Stop();

        protected virtual void OnReceivedPacket(ITCPSocket clientSocket, IBuffer payload)
        {
            EventHandler<ReceivedPacketEventArgs> receivedPacket = ReceivedPacket;
            if (receivedPacket != null)
            {
                ReceivedPacketEventArgs receivedPacketEventArgs = new ReceivedPacketEventArgs(clientSocket, payload);
                receivedPacket(this, receivedPacketEventArgs);
            }
        }

        protected virtual void OnClientDisconnected(ITCPSocket clientSocket)
        {
            EventHandler<DisconnectedEventArgs> clientDisconnected = ClientDisconnected;
            if (clientDisconnected != null)
            {
                DisconnectedEventArgs clientDisconnectedEventArgs = new DisconnectedEventArgs(clientSocket);
                clientDisconnected(this, clientDisconnectedEventArgs);
            }
        }

        protected virtual void OnClientConnected(ITCPSocket clientSocket)
        {
            EventHandler<ConnectedEventArgs> clientConnected = ClientConnected;
            if (clientConnected != null)
            {
                ConnectedEventArgs clientConnectedEventArgs = new ConnectedEventArgs(clientSocket);
                clientConnected(this, clientConnectedEventArgs);
            }
        }
    }
}