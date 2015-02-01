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
namespace MarrySocket.MClient
{
    using MarrySocket.MBase;
    using MarrySocket.MExtra.Logging;
    using System;
    using System.Net.Sockets;
    using System.Threading;

    public class SocketManager
    {
        private Action<string> onConnected;
        private Action<string> onDisconnected;
        private ClientConfig clientConfig;
        private PacketManager packetManager;
        private Logger logger;
        private Thread serverManager;
        private object myLock = new object();
        private EntitiesContainer entitiesContainer;
        private ServerSocket serverSocket;
        private volatile bool isRunning;

        public SocketManager(EntitiesContainer entitiesContainer)
        {
            this.entitiesContainer = entitiesContainer;
            this.onConnected = this.entitiesContainer.OnConnected;
            this.onDisconnected = this.entitiesContainer.OnDisconnected;
            this.clientConfig = this.entitiesContainer.ClientConfig;
            this.logger = this.entitiesContainer.ClientLog;
            this.packetManager = new PacketManager(this.entitiesContainer);
            this.serverSocket = this.entitiesContainer.ServerSocket;
            this.isRunning = false;
        }

        public void Start()
        {
            this.isRunning = true;
            this.serverManager = new Thread(ManagerProcess);
            this.serverManager.Name = "ServerManager";
            this.serverManager.Start();
        }

        public void Stop()
        {
            this.isRunning = false;
            this.entitiesContainer.IsConnected = false;
            if (serverManager != null && serverManager.IsAlive)
                this.serverManager.Join();
        }

        public void DestroySocket(string reason)
        {
            if (this.serverSocket != null)
            {
                this.serverSocket.Close();
            }

            if (this.onDisconnected != null)
            {
                this.onDisconnected(reason);
            }
            else
            {
                this.logger.Write("Client disconnected: {0}", reason, LogType.CLIENT);
            }

            this.isRunning = false;
            this.entitiesContainer.IsConnected = false;
        }

        public void ManagerProcess()
        {
            byte[] headerBuffer = new byte[Packet.HEADER_SIZE];
            byte[] dataBuffer;

            while (this.isRunning)
            {
                if (this.serverSocket.Socket.Poll(this.clientConfig.PollTimeout, SelectMode.SelectRead))
                {
                    try
                    {
                        if (this.serverSocket.Socket.Receive(headerBuffer, 0, Packet.HEADER_SIZE, SocketFlags.None) < Packet.HEADER_SIZE)
                        {
                            this.DestroySocket("Invalid Header");
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        this.DestroySocket(e.ToString());
                    }

                    Int32 packetLength = BitConverter.ToInt32(headerBuffer, 0) - Packet.HEADER_SIZE;
                    Int16 packetId = BitConverter.ToInt16(headerBuffer, Packet.PACKET_LENGTH_SIZE);

                    if (packetId < Int16.MaxValue)
                    {

                        if (packetLength < 0)
                        {
                            this.DestroySocket("Message length is less than zero");
                            continue;
                        }

                        if (packetLength > 0 && packetLength > this.clientConfig.BufferSize)
                        {
                            this.DestroySocket("Message length " + packetLength + " is larger than maximum message size " + this.clientConfig.BufferSize);
                            continue;
                        }

                        if (packetLength == 0)
                        {
                            this.DestroySocket("packet length is zero");
                            continue;
                        }

                        dataBuffer = new byte[packetLength];
                        int bytesReceived = 0;


                        //TODO TRY CATCH, Server disconnects client during read, will crash.
                        while (bytesReceived < packetLength)
                        {
                            if (this.serverSocket.Socket.Poll(this.clientConfig.PollTimeout, SelectMode.SelectRead))
                                bytesReceived += this.serverSocket.Socket.Receive(dataBuffer, bytesReceived, packetLength - bytesReceived, SocketFlags.None);
                        }

                        Packet packet = new Packet();
                        packet.PacketId = packetId;
                        packet.SetPacketData(dataBuffer);


                        packetManager.Handle(this.serverSocket, packet);
                    }
                }
            }

        }

    }
}
