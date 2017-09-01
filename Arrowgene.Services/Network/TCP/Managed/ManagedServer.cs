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
namespace Arrowgene.Services.Network.TCP.Managed
{
    using Arrowgene.Services.Network.TCP.Server;
    using Client;
    using Common;
    using Logging;
    using Serialization;
    using System;
    using System.Collections.Generic;
    using System.Net;

    public class ManagedServer : TCPServer
    {
        private PacketManager packetManager;
        private ISerializer serializer;
        private Dictionary<int, ManagedClientSocket> managedClients;

        public ManagedServer(IPAddress ipAddress, int port, ISerializer serializer, Logger logger) : base(ipAddress, port, logger)
        {
            this.serializer = serializer;
            this.packetManager = new PacketManager(this.serializer, base.Logger);
            this.managedClients = new Dictionary<int, ManagedClientSocket>();
        }

        public ManagedServer(IPAddress ipAddress, int port, Logger logger) : this(ipAddress, port, new BinaryFormatterSerializer(), logger)
        {

        }

        public ManagedServer(IPAddress ipAddress, int port) : this(ipAddress, port, new Logger(TCPClient.Name))
        {

        }

        /// <summary>
        /// Occures when a packet is received.
        /// </summary>
        public event EventHandler<ManagedReceivedPacketEventArgs> ManagedReceivedPacket;

        /// <summary>
        /// Occures when a client disconnected.
        /// </summary>
        public event EventHandler<ManagedDisconnectedEventArgs> ManagedDisconnected;

        /// <summary>
        /// Occures when a client connected.
        /// </summary>
        public event EventHandler<ManagedConnectedEventArgs> ManagedConnected;

        internal override void OnClientDisconnected(ClientSocket clientSocket)
        {
            base.OnClientDisconnected(clientSocket);

            ManagedClientSocket managedClientSocket = this.GetManagedClientSocket(clientSocket);
            OnManagedDisconnected(managedClientSocket);
        }

        internal override void OnClientConnected(ClientSocket clientSocket)
        {
            base.OnClientConnected(clientSocket);

            ManagedClientSocket managedClientSocket = this.GetManagedClientSocket(clientSocket);
            OnManagedConnected(managedClientSocket);
        }

        internal override void OnReceivedPacket(ClientSocket clientSocket, ByteBuffer payload)
        {
            base.OnReceivedPacket(clientSocket, payload);

            ManagedClientSocket managedClientSocket = this.GetManagedClientSocket(clientSocket);
            ByteBuffer buffer = managedClientSocket.Buffer;
            buffer.WriteBuffer(payload);
            buffer.ResetPosition();
            ManagedPacket packet = this.packetManager.Handle(clientSocket, buffer);
            if (packet != null)
            {
                this.OnManagedReceivedPacket(packet.Id, packet, managedClientSocket);
            }
        }

        private void OnManagedReceivedPacket(int packetId, ManagedPacket packet, ManagedClientSocket managedClientSocket)
        {
            EventHandler<ManagedReceivedPacketEventArgs> managedReceivedPacket = this.ManagedReceivedPacket;
            if (managedReceivedPacket != null)
            {
                ManagedReceivedPacketEventArgs managedReceivedPacketEventArgs = new ManagedReceivedPacketEventArgs(packetId, managedClientSocket, packet);
                managedReceivedPacket(this, managedReceivedPacketEventArgs);
            }
        }

        private void OnManagedDisconnected(ManagedClientSocket managedClientSocket)
        {
            EventHandler<ManagedDisconnectedEventArgs> managedDisconnected = this.ManagedDisconnected;
            if (managedDisconnected != null)
            {
                ManagedDisconnectedEventArgs managedDisconnectedEventArgs = new ManagedDisconnectedEventArgs(managedClientSocket);
                managedDisconnected(this, managedDisconnectedEventArgs);
            }
        }

        private void OnManagedConnected(ManagedClientSocket managedClientSocket)
        {
            EventHandler<ManagedConnectedEventArgs> managedConnected = this.ManagedConnected;
            if (managedConnected != null)
            {
                ManagedConnectedEventArgs managedConnectedEventArgs = new ManagedConnectedEventArgs(managedClientSocket);
                managedConnected(this, managedConnectedEventArgs);
            }
        }

        private ManagedClientSocket GetManagedClientSocket(ClientSocket clientSocket)
        {
            ManagedClientSocket managedClientSocket;
            if (this.managedClients.ContainsKey(clientSocket.Id))
            {
                managedClientSocket = this.managedClients[clientSocket.Id];
            }
            else
            {
                managedClientSocket = new ManagedClientSocket(clientSocket, this.Logger, this.serializer);
                this.managedClients.Add(clientSocket.Id, managedClientSocket);
            }
            return managedClientSocket;
        }

    }
}
