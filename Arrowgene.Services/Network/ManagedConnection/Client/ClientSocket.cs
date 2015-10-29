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
namespace Arrowgene.Services.Network.ManagedConnection.Client
{
    using Packet;
    using Logging;
    using Serialization;
    using System;
    using System.Net.Sockets;
    using Common;

    public class ClientSocket
    {
        private ISerializer serializer;
        private Logger logger;

        internal Socket Socket { get; private set; }

        public int Id { get; internal set; }
        public bool IsBusy { get; internal set; }
        public bool IsAlive { get; internal set; }
        public int InTraffic { get; internal set; }
        public int OutTraffic { get; internal set; }

        public ClientSocket(Socket socket, ISerializer serializer, Logger logger)
        {
            this.Socket = socket;
            this.serializer = serializer;
            this.logger = logger;

            this.Id = Application.Random.Next(1, 1000);
            this.IsAlive = true;
            this.IsBusy = false;
            this.InTraffic = 0;
            this.OutTraffic = 0;
        }

        public void SendObject(Int32 packetId, object myClass)
        {
            byte[] serialized = this.serializer.Serialize(myClass, this.logger);
            if (serialized != null)
            {
                ManagedPacket packet = ManagedPacket.CreatePacket(packetId, serialized);
                this.Socket.Send(packet.GetBytes());
            }
        }

        public void Close()
        {
            this.IsAlive = false;
            this.Socket.Shutdown(SocketShutdown.Both);
        }
    }


}
