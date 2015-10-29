namespace Arrowgene.Services.Network.ManagedConnection.Client
{
    using Logging;
    using Arrowgene.Services.Network.ManagedConnection.Serialization;
    using Event;
    using Exceptions;
    using Packet;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class ManagedClient
    {
        private const string DEFAULT_LOGGER_NAME = "Managed Client";

        private IPAddress serverIPAddress;
        private int serverPort;
        private ClientSocket clientSocket;
        private Thread readThread;
        private object myLock;
        private bool isConnected;
        private PacketManager packetManager;
        private ISerializer serializer;

        public ManagedClient(IPAddress serverIPAddress, int serverPort, ISerializer serializer, Logger logger)
        {
            if (serverIPAddress == null || serverPort <= 0)
                throw new InvalidParameterException(String.Format("IPAddress({0}) or Port({1}) invalid", serverIPAddress, serverPort));

            this.serializer = serializer;
            this.serverIPAddress = serverIPAddress;
            this.serverPort = serverPort;
            this.isConnected = false;
            this.myLock = new object();
            this.Logger = logger;

            this.packetManager = new PacketManager(this.serializer, this.Logger);
        }

        public ManagedClient(IPAddress serverIPAddress, int serverPort) : this(serverIPAddress, serverPort, new BinaryFormatterSerializer(), new Logger(DEFAULT_LOGGER_NAME))
        {

        }

        /// <summary>
        /// Server <see cref="System.Net.IPAddress"/>.
        /// </summary>
        public IPAddress ServerIPAddress { get { return this.serverIPAddress; } }

        /// <summary>
        /// Server port.
        /// </summary>
        public int ServerPort { get { return this.serverPort; } }

        /// <summary>
        /// Current logging instance where logs get written to.
        /// </summary>
        public Logger Logger { get; private set; }

        public int PollTimeout { get; set; }

        public int BufferSize { get; set; }

        /// <summary>
        /// Occures when a client disconnected.
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        /// Occures when a client connected.
        /// </summary>
        public event EventHandler<ConnectedEventArgs> Connected;

        /// <summary>
        /// Occures when a packet is received.
        /// </summary>
        public event EventHandler<ReceivedPacketEventArgs> ReceivedPacket;

        public void Connect()
        {
            this.isConnected = true;
            this.readThread = new Thread(ReadProcess);
            this.readThread.Name = "ManagedClient";
            this.readThread.Start();
        }

        public void Disconnect()
        {
            this.isConnected = false;

            if (readThread != null && readThread.IsAlive)
                this.readThread.Join();
            if (this.clientSocket != null)
            {
                this.clientSocket.Close();
            }

        }

        private void ReadProcess()
        {
            byte[] headerBuffer = new byte[ManagedPacket.HEADER_SIZE];
            byte[] dataBuffer;

            while (this.isConnected)
            {
                if (this.clientSocket.Socket.Poll(this.PollTimeout, SelectMode.SelectRead))
                {
                    try
                    {
                        if (this.clientSocket.Socket.Receive(headerBuffer, 0, ManagedPacket.HEADER_SIZE, SocketFlags.None) < ManagedPacket.HEADER_SIZE)
                        {
                            this.Logger.Write("Invalid Header", LogType.CLIENT);
                            this.Disconnect();
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        this.Logger.Write("Error {0}", e.ToString(), LogType.CLIENT);
                        this.Disconnect();
                        break;
                    }

                    Int32 packetId = BitConverter.ToInt32(headerBuffer, ManagedPacket.HEADER_PACKET_SIZE);

                    if (this.packetManager.CheckPacketId(packetId))
                    {
                        Int32 packetSize = BitConverter.ToInt32(headerBuffer, 0);

                        if (packetSize <= 0)
                        {
                            this.Logger.Write("Message length ({0}bytes) is zero or less", packetSize, LogType.CLIENT);
                            this.Disconnect();
                            break;
                        }

                        if (packetSize > this.BufferSize)
                        {
                            this.Logger.Write("Message length ({0}bytes) is larger than maximum message size ({1}bytes)", packetSize, this.BufferSize, LogType.CLIENT);
                            this.Disconnect();
                            break;
                        }

                        dataBuffer = new byte[packetSize];
                        int bytesReceived = 0;

                        //TODO TRY CATCH, Server disconnects client during read, will crash.
                        while (bytesReceived < packetSize)
                        {
                            if (this.clientSocket.Socket.Poll(this.PollTimeout, SelectMode.SelectRead))
                                bytesReceived += this.clientSocket.Socket.Receive(dataBuffer, bytesReceived, packetSize - bytesReceived, SocketFlags.None);
                        }

                        ManagedPacket managedPacket = new ManagedPacket(packetSize, packetId, headerBuffer, dataBuffer);
                        packetManager.Handle(this.clientSocket, managedPacket);
                    }
                }
            }

        }

        internal void OnReceivedPacket(int packetId, ManagedPacket packet)
        {
            EventHandler<ReceivedPacketEventArgs> receivedPacket = this.ReceivedPacket;
            if (receivedPacket != null)
            {
                ReceivedPacketEventArgs receivedPacketEventArgs = new ReceivedPacketEventArgs(packetId, this.clientSocket, packet);
                receivedPacket(this, receivedPacketEventArgs);
            }
        }

        internal void OnDisconnected()
        {
            EventHandler<DisconnectedEventArgs> clientDisconnected = this.Disconnected;
            if (clientDisconnected != null)
            {
                DisconnectedEventArgs clientDisconnectedEventArgs = new DisconnectedEventArgs(this.clientSocket);
                clientDisconnected(this, clientDisconnectedEventArgs);
            }
        }

        internal void OnConnected()
        {
            EventHandler<ConnectedEventArgs> clientConnected = this.Connected;
            if (clientConnected != null)
            {
                ConnectedEventArgs clientConnectedEventArgs = new ConnectedEventArgs(this.clientSocket);
                clientConnected(this, clientConnectedEventArgs);
            }
        }

    }
}