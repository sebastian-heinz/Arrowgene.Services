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
namespace ArrowgeneServices.Networking.MarrySocket.MClient
{
    using ArrowgeneServices.Logging;
    using ArrowgeneServices.Networking.MarrySocket.Packet;
    using System;
    using System.Net.Sockets;
    using System.Threading;

    internal class SocketManager
    {
        private ClientConfig clientConfig;
        private PacketManager packetManager;
        private Logger logger;
        private Thread serverManager;
        private object myLock = new object();
        private ServerSocket serverSocket;
        private volatile bool isRunning;

        internal SocketManager(ClientConfig clientConfig, ServerSocket serverSocket)
        {
            this.clientConfig = clientConfig;
            this.logger = this.clientConfig.Logger;
            this.packetManager = new PacketManager(this.clientConfig);
            this.serverSocket = serverSocket;
            this.isRunning = false;
        }

        internal void Start()
        {
            this.isRunning = true;
            this.serverManager = new Thread(ManagerProcess);
            this.serverManager.Name = "ServerManager";
            this.serverManager.Start();
        }

        internal void Stop()
        {
            this.isRunning = false;
            if (serverManager != null && serverManager.IsAlive)
                this.serverManager.Join();
        }

        internal void DestroySocket(string reason)
        {
            if (this.serverSocket != null)
            {
                this.serverSocket.Close();
            }

            this.clientConfig.IsConnected = false;
            this.logger.Write("Client disconnected: {0}", reason, LogType.CLIENT);
            this.isRunning = false;
            this.clientConfig.OnDisconnected(this.serverSocket);
        }

        internal void ManagerProcess()
        {
            byte[] headerBuffer = new byte[PacketHeader.HEADER_SIZE];
            byte[] dataBuffer;

            while (this.isRunning)
            {
                if (this.serverSocket.Socket.Poll(this.clientConfig.PollTimeout, SelectMode.SelectRead))
                {
                    try
                    {
                        if (this.serverSocket.Socket.Receive(headerBuffer, 0, PacketHeader.HEADER_SIZE, SocketFlags.None) < PacketHeader.HEADER_SIZE)
                        {
                            this.DestroySocket("Invalid Header");
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        this.DestroySocket(e.ToString());
                    }


                    PacketHeader packetHeader = PacketHeader.CreateInstance(headerBuffer);


                    if (packetHeader != null)
                    {

                        if (packetHeader.PacketSize < 0)
                        {
                            this.DestroySocket("Message length is less than zero");
                            continue;
                        }

                        if (packetHeader.PacketSize > 0 && packetHeader.PacketSize > this.clientConfig.BufferSize)
                        {
                            this.DestroySocket("Message length " + packetHeader.PacketSize + " is larger than maximum message size " + this.clientConfig.BufferSize);
                            continue;
                        }

                        if (packetHeader.PacketSize == 0)
                        {
                            this.DestroySocket("packet length is zero");
                            continue;
                        }

                        dataBuffer = new byte[packetHeader.DataSize];
                        int bytesReceived = 0;


                        //TODO TRY CATCH, Server disconnects client during read, will crash.
                        while (bytesReceived < packetHeader.DataSize)
                        {
                            if (this.serverSocket.Socket.Poll(this.clientConfig.PollTimeout, SelectMode.SelectRead))
                                bytesReceived += this.serverSocket.Socket.Receive(dataBuffer, bytesReceived, packetHeader.DataSize - bytesReceived, SocketFlags.None);
                        }

                        ReadPacket readPacket = new ReadPacket(packetHeader, dataBuffer);

                        packetManager.Handle(this.serverSocket, readPacket);
                    }
                }
            }

        }

    }
}
