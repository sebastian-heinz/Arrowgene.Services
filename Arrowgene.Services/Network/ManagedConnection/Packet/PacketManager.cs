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
namespace Arrowgene.Services.Network.ManagedConnection.Packet
{
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
        }

        internal bool Handle(ClientSocket clientSocket, ManagedPacket packet)
        {
            bool success = false;

            object myClass = this.serializer.Deserialize(packet.Payload, this.logger);
            if (myClass != null)
            {
                packet.Object = myClass;
                this.logger.Write("Handled Packet: {0}", packet.Id, LogType.PACKET);
                success = true;
            }
            else
            {
                this.logger.Write("Could not handled packet: {0}", packet.Id, LogType.PACKET);
            }

            return success;
        }

        internal bool CheckPacketId(int packetId)
        {
            return true;
        }

    }
}
