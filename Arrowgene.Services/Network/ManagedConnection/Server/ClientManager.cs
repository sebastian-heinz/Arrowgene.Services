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
            this.server.Logger.Write("Starting Client Managers...", LogType.SERVER);

            this.clientManager = new Thread[this.server.ManagerCount];
            try
            {
                for (int i = 0; i < this.server.ManagerCount; i++)
                {
                    this.server.Logger.Write("Starting ClientManager: {0}", i, LogType.SERVER);
                    this.clientManager[i] = new Thread(this.ManagerProcess);
                    this.clientManager[i].Name = "ClientManager[" + i + "]";
                    this.clientManager[i].Start();
                }

                this.isRunning = true;
            }
            catch (Exception e)
            {
                this.server.Logger.Write("Could not start one or more client managers: {0}", e.Message, LogType.SERVER);
                this.Stop();
            }

            this.server.Logger.Write("Initialized Client Managers: {0}", this.clientManager.Length, LogType.SERVER);
        }

        internal void Stop()
        {
            this.isRunning = false;

            this.server.Logger.Write("Shutting Client Manager down...", LogType.SERVER);

            lock (this.myLock)
            {
                for (int i = 0; i < this.clients.Count; i++)
                {
                    this.server.Logger.Write("Disconnecting Client: {0}", this.clients[i].Id, LogType.SERVER);
                    this.clients[i].Close();
                }
            }

            for (int i = 0; i < this.server.ManagerCount; i++)
            {
                this.server.Logger.Write("Joining Thread: {0}", this.clientManager[i].Name, LogType.SERVER);
                this.clientManager[i].Join();
            }

            this.server.Logger.Write("All Client Managers down.", LogType.SERVER);
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
                        DisposeClient(readyclients[0], "Disconnected by Server");
                        readyclients.RemoveAt(0);
                        continue;
                    }

                    byte[] headerBuffer = new byte[ManagedPacket.HEADER_SIZE];

                    try
                    {
                        if (readyclients[0].Socket.Receive(headerBuffer, 0, ManagedPacket.HEADER_SIZE, SocketFlags.None) < 1)
                        {
                            DisposeClient(readyclients[0], "Invalid Header");
                            readyclients.RemoveAt(0);
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        if (!readyclients[0].Socket.Connected)
                        {
                            DisposeClient(readyclients[0], String.Format("Client Error: {0}", e.Message));
                        }
                        else
                        {
                            this.server.Logger.Write("Failed to receive packet: {0}", e.Message, LogType.CLIENT);
                        }
                        readyclients.RemoveAt(0);
                        continue;
                    }


                    Int32 packetId = BitConverter.ToInt32(headerBuffer, ManagedPacket.HEADER_PACKET_SIZE);

                    if (this.packetManager.CheckPacketId(packetId))
                    {
                        Int32 packetSize = BitConverter.ToInt32(headerBuffer, 0);

                        if (packetSize <= 0)
                        {
                            DisposeClient(readyclients[0], "Message length is zero or less.");
                            readyclients.RemoveAt(0);
                            continue;
                        }

                        // TODO MaxPacketSize ? keep reading with new buffer..
                        if (packetSize > this.server.BufferSize)
                        {
                            DisposeClient(readyclients[0], String.Format("Message length {0} is larger than maximum message size {1}", packetSize, this.server.BufferSize));
                            readyclients.RemoveAt(0);
                            continue;
                        }

                        byte[] dataBuffer = new byte[packetSize];
                        int bytesReceived = 0;

                        while (bytesReceived < packetSize)
                        {
                            if (readyclients[0].Socket.Poll(this.server.PollTimeout, SelectMode.SelectRead))
                                bytesReceived += readyclients[0].Socket.Receive(dataBuffer, bytesReceived, packetSize - bytesReceived, SocketFlags.None);
                        }

                        ManagedPacket managedPacket = new ManagedPacket(packetSize, packetId, headerBuffer, dataBuffer);
                        this.packetManager.Handle(readyclients[0], managedPacket);
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