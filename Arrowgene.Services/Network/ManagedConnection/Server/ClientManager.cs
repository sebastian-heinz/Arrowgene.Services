namespace Arrowgene.Services.Network.ManagedConnection.Server
{
    using Packet;
    using Logging;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Net.Sockets;

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
            this.packetManager = new PacketManager(this.server);
            this.myLock = new object();
            this.isRunning = false;
            this.clients = new List<ClientSocket>();
            this.clientManager = new Thread[this.server.ManagerCount];
        }

        internal void Start()
        {
            this.clients.Clear();
            this.server.Logger.Write("Starting Client Manager...", LogType.SERVER);

            try
            {
                for (int i = 0; i < this.server.ManagerCount; i++)
                {
                    this.server.Logger.Write("Starting Manager: {0}", i, LogType.SERVER);
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
                    this.server.Logger.Write(String.Format("Disconnecting Client: {0}", this.clients[i].Id), LogType.SERVER);
                    this.clients[i].Close();
                }
            }

            for (int i = 0; i < this.server.ManagerCount; i++)
            {
                this.server.Logger.Write(String.Format("Joining Thread: {0}", this.clientManager[i].Name), LogType.SERVER);
                this.clientManager[i].Join();
            }

            this.server.Logger.Write("Client Manager down.", LogType.SERVER);
        }

        internal void AddClient(ClientSocket clientSocket)
        {
            lock (this.myLock)
            {
                this.clients.Add(clientSocket);
            }

            this.server.Logger.Write("Client connected: " + clientSocket.Id.ToString(), LogType.SERVER);
            this.server.OnClientConnected(clientSocket);
        }

        private void DisposeClient(ClientSocket clientSocket, string reason)
        {
            lock (this.myLock)
            {
                this.clients.Remove(clientSocket);
            }

            this.server.Logger.Write("Client[{0}]: disconnected: {1}", clientSocket.Id.ToString(), reason, LogType.SERVER);
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
                            this.server.Logger.Write("failed to receive packet: " + e.Message);
                        }
                        readyclients.RemoveAt(0);
                        continue;
                    }

                    ManagedPacket managedPacket = ManagedPacket.CreateInstance(headerBuffer);

                    if (managedPacket != null)
                    {
                        if (managedPacket.Size <= 0)
                        {
                            DisposeClient(readyclients[0], "Message length is zero or less.");
                            readyclients.RemoveAt(0);
                            continue;
                        }

                        // TODO MaxPacketSize ? keep reading with new buffer..
                        if (managedPacket.Size > this.server.BufferSize)
                        {
                            DisposeClient(readyclients[0], " Message length " + managedPacket.Size + " is larger than maximum message size " + this.server.BufferSize);
                            readyclients.RemoveAt(0);
                            continue;
                        }

                        byte[] dataBuffer = new byte[managedPacket.Size];
                        int bytesReceived = 0;

                        while (bytesReceived < managedPacket.Size)
                        {
                            if (readyclients[0].Socket.Poll(this.server.PollTimeout, SelectMode.SelectRead))
                                bytesReceived += readyclients[0].Socket.Receive(dataBuffer, bytesReceived, managedPacket.Size - bytesReceived, SocketFlags.None);
                        }

                        managedPacket.RawPacket = dataBuffer;

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