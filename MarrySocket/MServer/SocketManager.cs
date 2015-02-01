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
    using MarrySocket.MExtra.Logging;
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Threading;

    public class SocketManager
    {
        private EntitiesContainer entitieContainer;
        private Action<ClientSocket> onClientDisconnected;
        private Action<ClientSocket> onClientConnected;
        private List<ClientSocket> clients;
        private ClientList clientList;
        private ServerConfig serverConfig;
        private Thread[] clientManager;
        private Logger serverLog;
        private object myLock = new object();
        private volatile bool isRunning;

        public SocketManager(EntitiesContainer entitieContainer)
        {
            this.entitieContainer = entitieContainer;
            this.onClientDisconnected = this.entitieContainer.OnClientDisconnected;
            this.onClientConnected = this.entitieContainer.OnClientConnected;
            this.clientList = this.entitieContainer.ClientList;
            this.serverConfig = this.entitieContainer.ServerConfig;
            this.serverLog = this.entitieContainer.ServerLog;
            this.clients = new List<ClientSocket>();
            this.clientManager = new Thread[this.serverConfig.ManagerCount];
            this.isRunning = false;
        }

        public void Start()
        {
            this.clients = new List<ClientSocket>();
            this.serverLog.Write("Starting Client Manager...", LogType.SERVER);
            this.isRunning = true;
            try
            {
                for (int i = 0; i < this.serverConfig.ManagerCount; i++)
                {
                    this.serverLog.Write("Starting Manager: {0}", i, LogType.SERVER);
                    clientManager[i] = new Thread(ManagerProcess);
                    clientManager[i].Name = "ClientManager[" + i + "]";
                    clientManager[i].Start();
                }
            }
            catch (Exception e)
            {
                this.serverLog.Write("Could not start one or more client managers: {0}", e.Message, LogType.SERVER);
                this.Stop();
            }

            this.serverLog.Write("Initialized Client Managers: {0}", this.clientManager.Length, LogType.SERVER);
        }

        public void Stop()
        {

            this.isRunning = false;

            this.serverLog.Write("Shutting Client Manager down...", LogType.SERVER);

            lock (myLock)
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    this.serverLog.Write(String.Format("Disconnecting Client: {0}", clients[i].Id), LogType.SERVER);
                    clients[i].Close();
                }
            }

            for (int i = 0; i < this.serverConfig.ManagerCount; i++)
            {
                this.serverLog.Write(String.Format("Joining Thread: {0}", clientManager[i].Name), LogType.SERVER);
                clientManager[i].Join();
            }

            this.serverLog.Write("Client Manager down.", LogType.SERVER);

        }

        internal void AddClient(ClientSocket clientSocket)
        {
            lock (myLock)
            {
                this.clients.Add(clientSocket);
            }
            this.clientList.AddClient(clientSocket);

            this.serverLog.Write("Client connected: " + clientSocket.Id.ToString(), LogType.SERVER);

            if (this.onClientConnected != null)
            {
                this.onClientConnected(clientSocket);
            }
        }

        public void DisposeClient(ClientSocket clientSocket, string reason)
        {
            this.clientList.RemoveClient(clientSocket.Id);

            lock (myLock)
            {
                this.clients.Remove(clientSocket);
            }

            this.serverLog.Write("Client[{0}]: disconnected: {1}", clientSocket.Id.ToString(), reason, LogType.SERVER);

            this.onClientDisconnected(clientSocket);
        }

        public void ManagerProcess()
        {
            PacketManager packetManager = new PacketManager(this.entitieContainer);
            List<ClientSocket> readyclients = new List<ClientSocket>();
            byte[] headerBuffer = new byte[Packet.HEADER_SIZE];

            while (this.isRunning)
            {
                readyclients.Clear();
                lock (myLock)
                {
                    for (int i = 0; i < clients.Count; i++)
                    {
                        if (!clients[i].IsBusy)
                        {
                            clients[i].IsBusy = true;

                            if (clients[i].Socket.Connected && clients[i].Socket.Poll(this.serverConfig.PollTimeout, SelectMode.SelectRead) ||
                                !clients[i].IsAlive)
                            {
                                readyclients.Add(clients[i]);
                            }
                            else
                            {
                                clients[i].IsBusy = false;
                            }
                        }
                    }
                }

                while (readyclients.Count > 0)
                {

                    if (!readyclients[0].IsAlive)
                    {
                        DisposeClient(readyclients[0], "Disconnected by Server");
                        readyclients.RemoveAt(0);
                        continue;
                    }

                    try
                    {
                        if (readyclients[0].Socket.Receive(headerBuffer, 0, Packet.HEADER_SIZE, SocketFlags.None) < 1)
                        {
                            DisposeClient(readyclients[0], "Disconnected");
                            readyclients.RemoveAt(0);
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        if (!readyclients[0].Socket.Connected)
                        {
                            DisposeClient(readyclients[0], "Client Error" + e.Message);
                        }
                        else
                        {
                            this.serverLog.Write("failed to receive packet: " + e.Message);
                        }
                        readyclients.RemoveAt(0);
                        continue;
                    }

                    Int32 packetLength = BitConverter.ToInt32(headerBuffer, 0) - Packet.HEADER_SIZE;
                    Int16 packetId = BitConverter.ToInt16(headerBuffer, Packet.PACKET_LENGTH_SIZE);

                    if (packetId < Int16.MaxValue)
                    {
                        if (packetLength < 0)
                        {
                            DisposeClient(readyclients[0], "Message length is less than zero");
                            readyclients.RemoveAt(0);
                            continue;
                        }

                        if (this.serverConfig.BufferSize > 0 && packetLength > this.serverConfig.BufferSize)
                        {
                            DisposeClient(readyclients[0], " Message length " + packetLength + " is larger than maximum message size " + this.serverConfig.BufferSize);
                            readyclients.RemoveAt(0);
                            continue;
                        }

                        if (packetLength == 0)
                        {
                            readyclients[0].IsBusy = false;
                            readyclients.RemoveAt(0);
                            continue;
                        }

                        byte[] dataBuffer = new byte[packetLength];
                        int bytesReceived = 0;

                        while (bytesReceived < packetLength)
                        {
                            if (readyclients[0].Socket.Poll(this.serverConfig.PollTimeout, SelectMode.SelectRead))
                                bytesReceived += readyclients[0].Socket.Receive(dataBuffer, bytesReceived, packetLength - bytesReceived, SocketFlags.None);
                        }

                        Packet packet = new Packet();
                        packet.PacketId = packetId;
                        packet.SetPacketData(dataBuffer);

                        packetManager.Handle(readyclients[0], packet);
                    }
                    else
                    {
                        if (serverConfig.LogUnknownPacket == true)
                            this.serverLog.Write("Packet ID: " + packetId + " Length:" + packetLength);
                    }

                    readyclients[0].IsBusy = false;
                    readyclients.RemoveAt(0);
                }
                Thread.Sleep(this.serverConfig.ReadTimeout);
            }
        }

    }
}

