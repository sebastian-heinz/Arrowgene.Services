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
namespace Arrowgene.Services.Network.MarrySocket.MClient
{
    using Arrowgene.Services.Logging;
    using Arrowgene.Services.Network.MarrySocket.Packet;
    using Arrowgene.Services.Network.MarrySocket.Serialization;

    internal class PacketManager
    {
        private ClientConfig clientConfig;
        private Logger logger;
        private ISerialization serializer;

        internal PacketManager(ClientConfig clientConfig)
        {
            this.clientConfig = clientConfig;
            this.serializer = this.clientConfig.Serializer;
            this.logger = this.clientConfig.Logger;
        }

        internal void Handle(ServerSocket serverSocket, ReadPacket packet)
        {
            object myClass = this.serializer.Deserialize(packet.SerializedClass, this.logger);
            if (myClass != null)
            {
                serverSocket.InTraffic += packet.PacketHeader.PacketSize;
                this.clientConfig.OnReceivedPacket(packet.PacketHeader.PacketId, serverSocket, myClass);
                this.logger.Write("Handled Packet: {0}", packet.PacketHeader.PacketId, LogType.PACKET);
            }
            else
            {
                this.logger.Write("Could not handled packet: {0}", packet.PacketHeader.PacketId, LogType.PACKET);
            }
        }
    }
}
