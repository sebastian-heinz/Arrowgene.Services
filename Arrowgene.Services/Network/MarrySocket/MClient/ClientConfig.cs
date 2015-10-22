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
    using Exceptions;
    using Arrowgene.Services.Network.MarrySocket.MBase;
    using Arrowgene.Services.Network.MarrySocket.Serialization;

    using System;
    using System.Net;

    /// <summary>
    /// TODO SUMMARY
    /// </summary>
    public class ClientConfig : BaseConfig
    {
        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public ClientConfig(IPAddress ipAddress, int port, ISerialization serializer)
            : base(serializer)
        {
            if (ipAddress == null || port <= 0)
                throw new InvalidParameterException(string.Format("IPAddress({0}) or Port({1}) invalid", ipAddress, port));

            base.Logger.Name = "MarryClient";
            base.ServerIP = ipAddress;
            base.ServerPort = port;
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public ClientConfig(IPAddress ipAddress, int port)
            : this(ipAddress, port, new BinaryFormatterSerializer())
        {

        }

        internal event EventHandler<DisconnectedEventArgs> Disconnected;
        internal event EventHandler<ConnectedEventArgs> Connected;
        internal event EventHandler<ReceivedPacketEventArgs> ReceivedPacket;

        internal bool IsConnected { get; set; }

        internal void OnReceivedPacket(int packetId, ServerSocket serverSocket, object myObject)
        {
            EventHandler<ReceivedPacketEventArgs> receivedPacket = this.ReceivedPacket;
            if (receivedPacket != null)
            {
                ReceivedPacketEventArgs receivedPacketEventArgs = new ReceivedPacketEventArgs(packetId, serverSocket, myObject);
                receivedPacket(this, receivedPacketEventArgs);
            }
        }

        internal void OnDisconnected(ServerSocket serverSocket)
        {
            EventHandler<DisconnectedEventArgs> disconnected = this.Disconnected;
            if (disconnected != null)
            {
                DisconnectedEventArgs clientDisconnectedEventArgs = new DisconnectedEventArgs(serverSocket);
                disconnected(this, clientDisconnectedEventArgs);
            }
        }

        internal void OnConnected(ServerSocket serverSocket)
        {
            EventHandler<ConnectedEventArgs> connected = this.Connected;
            if (connected != null)
            {
                ConnectedEventArgs connectedEventArgs = new ConnectedEventArgs(serverSocket);
                connected(this, connectedEventArgs);
            }
        }

    }
}

