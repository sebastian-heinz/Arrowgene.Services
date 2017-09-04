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
    using Client;
    using Common;
    using Logging;
    using System;
    using TCP.Managed.Serialization;

    public class ManagedClient : TCPClient
    {
        private PacketManager packetManager;
        private ISerializer serializer;
        private ByteBuffer buffer;

        public ManagedClient(ISerializer serializer, Logger logger) : base(logger)
        {
            this.buffer = new ByteBuffer();
            this.serializer = serializer;
            this.packetManager = new PacketManager(this.serializer, this.Logger);
        }

        public ManagedClient() : this(new BinaryFormatterSerializer(), new Logger(TCPClient.Name))
        {

        }

        public event EventHandler<ManagedClientReceivedPacketEventArgs> ManagedReceivedPacket;

        /// <summary>
        /// Occures when a client disconnected.
        /// </summary>
        public event EventHandler<ManagedClientDisconnectedEventArgs> ManagedDisconnected;

        /// <summary>
        /// Occures when a client connected.
        /// </summary>
        public event EventHandler<ManagedClientConnectedEventArgs> ManagedConnected;

        public void Send(int packetId, object myClass)
        {
            byte[] serialized = this.serializer.Serialize(packetId, myClass, base.Logger);
            if (serialized != null)
            {
                ByteBuffer packet = new ByteBuffer();
                packet.WriteInt32(packetId);
                packet.WriteInt32(serialized.Length + ManagedPacket.HeaderSize);
                packet.WriteBytes(serialized);
                base.Send(packet.ReadBytes());
            }
        }

        internal override void OnClientReceivedPacket(ClientSocket clientSocket, ByteBuffer payload)
        {
            base.OnClientReceivedPacket(clientSocket, payload);

            this.buffer.WriteBuffer(payload);
            this.buffer.ResetPosition();
            ManagedPacket packet = this.packetManager.Handle(clientSocket, buffer);
            if (packet != null)
            {
                this.OnReceivedManagedPacket(packet.Id, packet);
            }
        }

        internal override void OnDisconnected()
        {
            base.OnDisconnected();
            OnManagedDisconnected();
        }

        internal override void OnConnected()
        {
            base.OnConnected();
            OnManagedConnected();
        }

        private void OnReceivedManagedPacket(int packetId, ManagedPacket packet)
        {
            EventHandler<ManagedClientReceivedPacketEventArgs> managedReceivedPacket = this.ManagedReceivedPacket;
            if (managedReceivedPacket != null)
            {
                ManagedClientReceivedPacketEventArgs managedClientReceivedPacketEventArgs = new ManagedClientReceivedPacketEventArgs(packetId, this, packet);
                managedReceivedPacket(this, managedClientReceivedPacketEventArgs);
            }
        }

        private void OnManagedDisconnected()
        {
            EventHandler<ManagedClientDisconnectedEventArgs> managedDisconnected = this.ManagedDisconnected;
            if (managedDisconnected != null)
            {
                ManagedClientDisconnectedEventArgs managedClientDisconnectedEventArgs = new ManagedClientDisconnectedEventArgs(this);
                managedDisconnected(this, managedClientDisconnectedEventArgs);
            }
        }

        private void OnManagedConnected()
        {
            EventHandler<ManagedClientConnectedEventArgs> managedConnected = this.ManagedConnected;
            if (managedConnected != null)
            {
                ManagedClientConnectedEventArgs managedClientConnectedEventArgs = new ManagedClientConnectedEventArgs(this);
                managedConnected(this, managedClientConnectedEventArgs);
            }
        }
    }
}