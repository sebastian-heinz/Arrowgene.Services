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

        public event EventHandler<ClientReceivedManagedPacketEventArgs> ClientReceivedManagedPacket;

        public void Send(int packetId, object myClass)
        {
            byte[] serialized = this.serializer.Serialize(myClass, base.Logger);
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
            this.Read(clientSocket, payload);
            base.OnClientReceivedPacket(clientSocket, payload);
        }

        private void Read(ClientSocket clientSocket, ByteBuffer payload)
        {
            this.buffer.WriteBuffer(payload);
            this.buffer.ResetPosition();

            ManagedPacket packet = this.packetManager.Handle(clientSocket, buffer);
            if (packet != null)
            {
                this.OnReceivedManagedPacket(packet.Id, packet);
            }
        }

        private void OnReceivedManagedPacket(int packetId, ManagedPacket packet)
        {
            EventHandler<ClientReceivedManagedPacketEventArgs> clientReceivedManagedPacket = this.ClientReceivedManagedPacket;
            if (clientReceivedManagedPacket != null)
            {
                ClientReceivedManagedPacketEventArgs clientReceivedManagedPacketEventArgs = new ClientReceivedManagedPacketEventArgs(packetId, this, packet);
                clientReceivedManagedPacket(this, clientReceivedManagedPacketEventArgs);
            }
        }
    }
}