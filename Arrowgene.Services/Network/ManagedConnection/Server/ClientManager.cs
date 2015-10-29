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
namespace Arrowgene.Services.Network.ManagedConnection.Server
{
    using Client;
    using Logging;
    using Packet;
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Threading;

    internal class ClientManager
    {
        private PacketManager packetManager;
        private ManagedServer server;
        private List<ClientSocket> clients;
        private Thread[] clientManager;
        private object myLock;
        private bool isRunning;

        internal ClientManager(ManagedServer server)
        {
            this.server = server;
            this.packetManager = new PacketManager(this.server.Serializer, this.server.Logger);
            this.myLock = new object();
            this.isRunning = false;
            this.clients = new List<ClientSocket>();
        }

        internal void Start()
        {
            this.clients.Clear();
            this.server.Logger.Write("Starting clientmanagers...", LogType.SERVER);

            this.clientManager = new Thread[this.server.ManagerCount];
            try
            {
                for (int i = 0; i < this.server.ManagerCount; i++)
                {
                    this.server.Logger.Write("Starting clientmanager number: {0}", i, LogType.SERVER);
                    this.clientManager[i] = new Thread(this.ManagerProcess);
                    this.clientManager[i].Name = "ClientManager[" + i + "]";
                    this.clientManager[i].Start();
                }

                this.isRunning = true;
            }
            catch (Exception e)
            {
                this.server.Logger.Write("Could not start one or more clientmanagers: {0}", e.Message, LogType.SERVER);
                this.Stop();
            }

            this.server.Logger.Write("Initialized clientmanagers: {0}", this.clientManager.Length, LogType.SERVER);
        }

        internal void Stop()
        {
            this.isRunning = false;

            this.server.Logger.Write("Shutting clientmanager down...", LogType.SERVER);

            lock (this.myLock)
            {
                for (int i = 0; i < this.clients.Count; i++)
                {
                    this.server.Logger.Write("Disconnecting client: {0}", this.clients[i].Id, LogType.SERVER);
                    this.clients[i].Close();
                }
            }

            for (int i = 0; i < this.server.ManagerCount; i++)
            {
                this.server.Logger.Write("Shutting clientmanager number: {0} down...", this.clientManager[i].Name, LogType.SERVER);
                this.clientManager[i].Join();
                this.server.Logger.Write("clientmanager number: {0} down", this.clientManager[i].Name, LogType.SERVER);
            }

            this.server.Logger.Write("All clientmanagers down.", LogType.SERVER);
        }

        internal void AddClient(ClientSocket clientSocket)
        {
            lock (this.myLock)
            {
                this.clients.Add(clientSocket);
            }

            this.server.Logger.Write("Client connected: {0}", clientSocket.Id.ToString(), LogType.CLIENT);
            this.server.OnClientConnected(clientSocket);
        }

        private void DisposeClient(ClientSocket clientSocket, string reason)
        {
            lock (this.myLock)
            {
                this.clients.Remove(clientSocket);
            }

            this.server.Logger.Write("Client[{0}]: disconnected: {1}", clientSocket.Id.ToString(), reason, LogType.CLIENT);
            this.server.OnClientDisconnected(clientSocket);
        }

        private void ManagerProcess()
        {

            List<ClientSocket> readyclients = new List<ClientSocket>();

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

                            if (clients[i].Socket.Connected && clients[i].Socket.Poll(this.server.PollTimeout, SelectMode.SelectRead) || !clients[i].IsAlive)
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
                        DisposeClient(readyclients[0], "Disconnected by server");
                        readyclients.RemoveAt(0);
                        continue;
                    }

                    byte[] headerBuffer = new byte[ManagedPacket.HEADER_SIZE];

                    try
                    {
                        if (readyclients[0].Socket.Receive(headerBuffer, 0, ManagedPacket.HEADER_SIZE, SocketFlags.None) < 1)
                        {
                            DisposeClient(readyclients[0], "Invalid header");
                            readyclients.RemoveAt(0);
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        if (!readyclients[0].Socket.Connected)
                        {
                            DisposeClient(readyclients[0], String.Format("Client error: {0}", e.Message));
                        }
                        else
                        {
                            this.server.Logger.Write("Failed to receive packet: {0}", e.Message, LogType.CLIENT);
                        }
                        readyclients.RemoveAt(0);
                        continue;
                    }


                    Int32 packetId = BitConverter.ToInt32(headerBuffer, ManagedPacket.HEADER_PAYLOAD_SIZE);

                    if (this.packetManager.CheckPacketId(packetId))
                    {
                        Int32 payloadSize = BitConverter.ToInt32(headerBuffer, 0);

                        if (payloadSize <= 0)
                        {
                            DisposeClient(readyclients[0], "Message length is zero or less.");
                            readyclients.RemoveAt(0);
                            continue;
                        }

                        // TODO MaxPacketSize ? keep reading with new buffer..
                        if (payloadSize > this.server.BufferSize)
                        {
                            DisposeClient(readyclients[0], String.Format("Message length {0} is larger than maximum message size {1}", payloadSize, this.server.BufferSize));
                            readyclients.RemoveAt(0);
                            continue;
                        }

                        byte[] payload = new byte[payloadSize];
                        int bytesReceived = 0;

                        while (bytesReceived < payloadSize)
                        {
                            if (readyclients[0].Socket.Poll(this.server.PollTimeout, SelectMode.SelectRead))
                                bytesReceived += readyclients[0].Socket.Receive(payload, bytesReceived, payloadSize - bytesReceived, SocketFlags.None);
                        }

                        ManagedPacket managedPacket = new ManagedPacket(packetId, headerBuffer, payload);

                        if (this.packetManager.Handle(readyclients[0], managedPacket))
                        {
                            this.server.OnReceivedPacket(packetId, readyclients[0], managedPacket);
                        }
                        else
                        {
                           // Packet could not be handled
                        }
                    }
                    else
                    {
                        if (this.server.LogUnknownPacket)
                        {
                            // TODO
                            //  this.server.Logger.Write("Packet ID: " + packetHeader.PacketId + " Length:" + packetHeader.PacketSize);
                        }
                    }

                    readyclients[0].IsBusy = false;
                    readyclients.RemoveAt(0);
                }

                Thread.Sleep(this.server.ReadTimeout);
            }
        }

    }
}