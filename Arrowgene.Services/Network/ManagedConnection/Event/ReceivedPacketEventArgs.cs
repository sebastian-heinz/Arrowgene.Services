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
namespace Arrowgene.Services.Network.ManagedConnection.Event
{
    using Packet;
    using Server;
    using System;

    public class ReceivedPacketEventArgs : EventArgs
    {
        public ReceivedPacketEventArgs(int packetId, ClientSocket clientSocket, ManagedPacket packet)
        {
            this.ClientSocket = clientSocket;
            this.PacketId = packetId;
            this.Packet = packet;
        }

        public int PacketId { get; private set; }

        public ClientSocket ClientSocket { get; private set; }

        public ManagedPacket Packet { get; private set; }
    }
}
