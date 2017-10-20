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
    using Common.Buffers;
    using Client;
    using Logging;
    using Serialization;

    internal class PacketManager
    {
        private ISerializer serializer;
        private Logger logger;

        internal PacketManager(ISerializer serializer, Logger logger)
        {
            this.serializer = serializer;
            this.logger = logger;
            this.LogUnknownPacket = true;
        }

        public bool LogUnknownPacket { get; set; }

        internal ManagedPacket Handle(ClientSocket clientSocket, IBuffer buffer)
        {
            ManagedPacket managedPacket = null;
            int packetId = buffer.ReadInt32();
            int contentSize = buffer.ReadInt32();

            if (this.CheckPacketId(packetId))
            {
                if (contentSize <= 0)
                {
                    clientSocket.Close();
                    this.logger.Write("Message length is zero or less.");
                }

                byte[] content = buffer.ReadBytes(contentSize);

                object myClass = this.serializer.Deserialize(packetId, content, this.logger);
                if (myClass != null)
                {
                    managedPacket = new ManagedPacket(packetId, myClass);
                    this.logger.Write("Handled Packet: {0}", packetId, LogType.PACKET);
                    clientSocket.InTraffic += contentSize;
                }
                else
                {
                    this.logger.Write("Could not handled packet: {0}", packetId, LogType.PACKET);
                }
            }
            else
            {
                if (this.LogUnknownPacket)
                {
                      this.logger.Write("Packet ID: " + packetId + " Length:" + contentSize);
                }
            }

            return managedPacket;
        }

        internal bool CheckPacketId(int packetId)
        {
            return true;
        }

    }
}
