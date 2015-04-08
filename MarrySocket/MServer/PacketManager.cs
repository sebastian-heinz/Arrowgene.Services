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
namespace MarrySocket.MServer
{
    using MarrySocket.MExtra.Logging;
    using MarrySocket.MExtra.Packet;
    using MarrySocket.MExtra.Serialization;

    internal class PacketManager
    {
        private Logger logger;
        private ServerConfig serverConfig;
        private ISerialization serializer;

        internal PacketManager(ServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;
            this.logger = this.serverConfig.Logger;
            this.serializer = this.serverConfig.Serializer;
        }

        internal void Handle(ClientSocket clientSocket, ReadPacket packet)
        {
            object myClass = this.serializer.Deserialize(packet.SerializedClass, this.logger);
            if (myClass != null)
            {
                clientSocket.InTraffic += packet.PacketHeader.PacketSize;
                this.serverConfig.OnReceivedPacket(packet.PacketHeader.PacketId, clientSocket, myClass);
                this.logger.Write("Client[{0}]: Handled Packet: {1}", clientSocket.Id, packet.PacketHeader.PacketId, LogType.PACKET);
            }
            else
            {
                this.logger.Write("Client[{0}]: Could not handled packet: {1}", clientSocket.Id, packet.PacketHeader.PacketId, LogType.PACKET);
            }
        }

    }
}
