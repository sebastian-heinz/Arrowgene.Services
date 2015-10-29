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
    using Arrowgene.Services.Network.ManagedConnection.Event;
    using Client;
    using Common;
    using Exceptions;
    using Logging;
    using Packet;
    using Serialization;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class ManagedServer
    {
        private const string DEFAULT_NAME = "Managed Server";

        private Thread serverThread;
        private ClientManager clientManager;

        /// <summary>
        /// Creates a new <see cref="ManagedServer"/> instance with a specified <see cref="ISerializer"/> serializer and <see cref="Logger"/>.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public ManagedServer(IPAddress ipAddress, int port, ISerializer serializer, Logger logger)
        {
            if (ipAddress == null)
                throw new InvalidParameterException(String.Format("IPAddress({0}) invalid", ipAddress));

            if (port <= 0 || port > 65535)
                throw new InvalidParameterException(String.Format("Port({0}) invalid", port));

            if (serializer == null)
                throw new InvalidParameterException("Serializer is null");

            if (logger == null)
                throw new InvalidParameterException("Logger is null");

            this.IPAddress = ipAddress;
            this.Port = port;
            this.Serializer = serializer;
            this.Logger = logger;

            this.IsListening = false;
            this.LogUnknownPacket = true;
            this.ManagerCount = 5;
            this.Backlog = 10;
            this.ReadTimeout = 20;
            this.PollTimeout = 10;
            this.BufferSize = 1024;
            this.IPv4v6AgnosticSocket = true;

            this.clientManager = new ClientManager(this);
        }

        /// <summary>
        /// Creates a new <see cref="ManagedServer"/> instance.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        public ManagedServer(IPAddress ipAddress, int port) : this(ipAddress, port, new BinaryFormatterSerializer(), new Logger(DEFAULT_NAME))
        {

        }

        /// <summary>
        /// Creates a new <see cref="ManagedServer"/> instance.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="logger"></param>
        public ManagedServer(IPAddress ipAddress, int port, Logger logger) : this(ipAddress, port, new BinaryFormatterSerializer(), logger)
        {

        }

        /// <summary>
        /// Creates a new <see cref="ManagedServer"/> instance.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="serializer"></param>
        public ManagedServer(IPAddress ipAddress, int port, ISerializer serializer) : this(ipAddress, port, serializer, new Logger(DEFAULT_NAME))
        {

        }

        public bool LogUnknownPacket { get; set; }

        public int ManagerCount { get; set; }

        public int Backlog { get; set; }

        public int ReadTimeout { get; set; }

        public int PollTimeout { get; set; }

        public int BufferSize { get; set; }

        /// <summary>
        /// Enables measures to achieve an IPv4/IPv6 agnostic socket.
        /// Binds <see cref="System.Net.Sockets.Socket"/> always automatically to <see cref="IPAddress.IPv6Any"/>. 
        /// Sets the <see cref="SocketOptionLevel"/>(27) "USE_IPV6_ONLY" to false.
        /// </summary>
        public bool IPv4v6AgnosticSocket { get; set; }

        /// <summary>
        /// Current logging instance where logs get written to.
        /// </summary>
        public Logger Logger { get; private set; }

        /// <summary>
        /// Server status.
        /// </summary>
        public bool IsListening { get; private set; }

        /// <summary>
        /// Servers <see cref="System.Net.IPAddress"/>.
        /// </summary>
        public IPAddress IPAddress { get; private set; }

        /// <summary>
        /// Servers port.
        /// </summary>
        public int Port { get; private set; }

        internal ISerializer Serializer { get; set; }

        internal Socket Socket { get; set; }

        /// <summary>
        /// Occures when a client disconnected.
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> ClientDisconnected;

        /// <summary>
        /// Occures when a client connected.
        /// </summary>
        public event EventHandler<ConnectedEventArgs> ClientConnected;

        /// <summary>
        /// Occures when a packet is received.
        /// </summary>
        public event EventHandler<ReceivedPacketEventArgs> ReceivedPacket;

        /// <summary>
        /// Start accepting connections,
        /// Creates a new <see cref="Arrowgene.Services.Logging.Logger"/> instance if none is set.
        /// </summary>
        public void Start()
        {
            if (!this.IsListening)
            {
                this.Logger.Write("Starting server...", LogType.SERVER);
                this.serverThread = new Thread(ServerThread);
                this.serverThread.Name = DEFAULT_NAME;
                this.serverThread.Start();
            }
            else
            {
                this.Logger.Write("Server already online.", LogType.SERVER);
            }
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop()
        {
            if (this.IsListening)
            {
                this.Logger.Write("Shutting server down...", LogType.SERVER);
                this.IsListening = false;
                this.serverThread.Join();
                this.Logger.Write("Server offline.", LogType.SERVER);
            }
            else
            {
                this.Logger.Write("Server already offline.", LogType.SERVER);
            }
        }

        private void ServerThread()
        {
            this.clientManager.Start();

            try
            {
                this.Socket = this.CreateSocket();

                if (this.Socket != null)
                {
                    this.Socket.Bind(new IPEndPoint(this.IPAddress, this.Port));
                    this.Socket.Listen(this.Backlog);
                    this.IsListening = true;
                    this.Logger.Write("Listening on port: {0}", this.Port, LogType.SERVER);
                    this.Logger.Write("Server online.", LogType.SERVER);
                    while (this.IsListening)
                    {
                        if (this.Socket.Poll(this.PollTimeout, SelectMode.SelectRead))
                        {
                            this.clientManager.AddClient(new ClientSocket(this.Socket.Accept(), this.Serializer, this.Logger));
                        }
                    }
                }
                else
                {
                    this.Logger.Write("Server could not be started.", LogType.SERVER);
                }
            }
            catch (Exception exception)
            {
                this.Logger.Write(exception.Message, LogType.ERROR);
            }
            finally
            {
                if (this.Socket.Connected)
                {
                    this.Socket.Shutdown(SocketShutdown.Both);
                }

                this.clientManager.Stop();
                this.Socket.Close();
                this.IsListening = false;
                this.Logger.Write("Server stopped.", LogType.SERVER);
            }
        }

        private Socket CreateSocket()
        {
            Socket socket = null;

            this.Logger.Write("Creating socket", LogType.SERVER);

            if (this.IPv4v6AgnosticSocket)
            {
                socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.IPv6, IP.USE_IPV6_ONLY, false);
                this.IPAddress = IPAddress.IPv6Any;
                this.Logger.Write("Changed server ip to IPAddress.IPv6Any ({0})", this.IPAddress, LogType.SERVER);
                this.Logger.Write("Created socket (IPv4 and IPv6 support)", LogType.SERVER);
            }

            if (socket == null)
            {
                if (this.IPAddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                    this.Logger.Write("Created socket (IPv6 support)", LogType.SERVER);
                }
                else
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    this.Logger.Write("Created socket (IPv4 support)", LogType.SERVER);
                }
            }

            return socket;
        }

        internal void OnReceivedPacket(int packetId, ClientSocket clientSocket, ManagedPacket packet)
        {
            EventHandler<ReceivedPacketEventArgs> receivedPacket = this.ReceivedPacket;
            if (receivedPacket != null)
            {
                ReceivedPacketEventArgs receivedPacketEventArgs = new ReceivedPacketEventArgs(packetId, clientSocket, packet);
                receivedPacket(this, receivedPacketEventArgs);
            }
        }

        internal void OnClientDisconnected(ClientSocket clientSocket)
        {
            EventHandler<DisconnectedEventArgs> clientDisconnected = this.ClientDisconnected;
            if (clientDisconnected != null)
            {
                DisconnectedEventArgs clientDisconnectedEventArgs = new DisconnectedEventArgs(clientSocket);
                clientDisconnected(this, clientDisconnectedEventArgs);
            }
        }

        internal void OnClientConnected(ClientSocket clientSocket)
        {
            EventHandler<ConnectedEventArgs> clientConnected = this.ClientConnected;
            if (clientConnected != null)
            {
                ConnectedEventArgs clientConnectedEventArgs = new ConnectedEventArgs(clientSocket);
                clientConnected(this, clientConnectedEventArgs);
            }
        }

    }
}
