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
    using MarrySocket.MBase;
    using MarrySocket.MExtra.Serialization;
    using System;
    using System.Net;

    public class ServerConfig : BaseConfig
    {
        public ServerConfig(ISerialization serializer)
            : base(serializer)
        {
            base.ServerIP = IPAddress.IPv6Any;
            base.ServerPort = 2345;
            this.Backlog = 10;
            this.ReadTimeout = 20;
            this.ManagerCount = 5;
            this.LogUnknownPacket = true;
        }

        public ServerConfig()
            : this(new BinaryFormatterSerializer())
        {

        }

        internal event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;
        internal event EventHandler<ClientConnectedEventArgs> ClientConnected;
        internal event EventHandler<ReceivedPacketEventArgs> ReceivedPacket;

        public bool LogUnknownPacket { get; set; }
        public int ManagerCount { get; set; }
        public int Backlog { get; set; }
        public int ReadTimeout { get; set; }
        internal bool IsListening { get; set; }

        internal void OnReceivedPacket(int packetId, ClientSocket clientSocket, object myObject)
        {
            EventHandler<ReceivedPacketEventArgs> receivedPacket = this.ReceivedPacket;
            if (receivedPacket != null)
            {
                ReceivedPacketEventArgs receivedPacketEventArgs = new ReceivedPacketEventArgs(packetId, clientSocket, myObject);
                receivedPacket(this, receivedPacketEventArgs);
            }
        }

        internal void OnClientDisconnected(ClientSocket clientSocket)
        {
            EventHandler<ClientDisconnectedEventArgs> clientDisconnected = this.ClientDisconnected;
            if (ClientDisconnected != null)
            {
                ClientDisconnectedEventArgs clientDisconnectedEventArgs = new ClientDisconnectedEventArgs(clientSocket);
                clientDisconnected(this, clientDisconnectedEventArgs);
            }
        }

        internal void OnClientConnected(ClientSocket clientSocket)
        {
            EventHandler<ClientConnectedEventArgs> clientConnected = this.ClientConnected;
            if (clientConnected != null)
            {
                ClientConnectedEventArgs clientConnectedEventArgs = new ClientConnectedEventArgs(clientSocket);
                clientConnected(this, clientConnectedEventArgs);
            }
        }

    }
}

