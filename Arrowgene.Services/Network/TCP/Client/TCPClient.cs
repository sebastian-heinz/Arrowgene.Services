namespace Arrowgene.Services.Network.TCP.Client
{
    using Common;
    using Exceptions;
    using Logging;
    using System;
    using Server;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class TCPClient
    {
        public const string Name = "Managed Client";
        private const int ThreadJoinTimeout = 1000;

        private ClientSocket clientSocket;
        private Thread readThread;

        public TCPClient(Logger logger)
        {
            this.Logger = logger;

            this.IsConnected = false;
            this.PollTimeout = 10;
            this.BufferSize = 1024;
        }

        public TCPClient() : this(new Logger(Name))
        {

        }

        /// <summary>
        /// Server <see cref="System.Net.IPAddress"/>.
        /// </summary>
        public IPAddress ServerIPAddress { get; private set; }

        /// <summary>
        /// Server port.
        /// </summary>
        public int ServerPort { get; private set; }

        /// <summary>
        /// Current logging instance where logs get written to.
        /// </summary>
        public Logger Logger { get; private set; }

        public int PollTimeout { get; set; }

        public int BufferSize { get; set; }

        public bool IsConnected { get; private set; }

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
        public event EventHandler<ClientReceivedPacketEventArgs> ReceivedPacket;

        /// <summary>
        /// Occures when a client could't establish a connection.
        /// </summary>
        public event EventHandler<ConnectErrorEventArgs> ConnectError;

        public void Connect(String serverIPAddress, int serverPort)
        {
            this.Connect(IPAddress.Parse(serverIPAddress), serverPort, TimeSpan.Zero);
        }

        public void Connect(String serverIPAddress, int serverPort, TimeSpan timeout)
        {
            this.Connect(IPAddress.Parse(serverIPAddress), serverPort, timeout);
        }

        public void Connect(IPAddress serverIPAddress, int serverPort)
        {
            this.Connect(serverIPAddress, serverPort, TimeSpan.Zero);
        }

        public void Connect(IPAddress serverIPAddress, int serverPort, TimeSpan timeout)
        {
            if (!this.IsConnected)
            {
                if (serverIPAddress == null || serverPort <= 0)
                    throw new InvalidParameterException(String.Format("IPAddress({0}) or Port({1}) invalid", serverIPAddress, serverPort));

                this.ServerIPAddress = serverIPAddress;
                this.ServerPort = serverPort;

                try
                {
                    Socket socket = this.CreateSocket();
                    if (socket != null)
                    {
                        if (timeout != TimeSpan.Zero)
                        {
                            IAsyncResult result = socket.BeginConnect(serverIPAddress, serverPort, null, null);
                            bool success = result.AsyncWaitHandle.WaitOne(timeout, true);
                            if (socket.Connected && success)
                            {
                                socket.EndConnect(result);
                                ConnectionEstablished(socket);
                            }
                            else
                            {
                                this.Logger.Write("Client connection timed out", LogType.SERVER);
                                socket.Close();
                                this.OnConnectError("Client connection timed out", serverIPAddress, serverPort, timeout);
                            }
                        }
                        else
                        {
                            socket.Connect(this.ServerIPAddress, this.ServerPort);
                            ConnectionEstablished(socket);
                        }
                    }
                    else
                    {
                        this.Logger.Write("Client could not connect.", LogType.SERVER);
                        this.OnConnectError("Client could not connect.", serverIPAddress, serverPort, timeout);
                    }
                }
                catch (Exception exception)
                {
                    this.Logger.Write(exception.Message, LogType.ERROR);
                    this.OnConnectError(exception.Message, serverIPAddress, serverPort, timeout);
                }
            }
            else
            {
                this.Logger.Write("Client is already connected.", LogType.SERVER);
                this.OnConnectError("Client is already connected.", serverIPAddress, serverPort, timeout);
            }
        }


        public void Disconnect()
        {
            this.IsConnected = false;

            if (readThread != null)
            {
                this.Logger.Write("Shutting reading thread down...", LogType.CLIENT);

                if (Thread.CurrentThread != this.readThread)
                {
                    if (this.readThread.Join(ThreadJoinTimeout))
                    {
                        this.Logger.Write("Reading thread ended clean.", LogType.CLIENT);
                    }
                    else
                    {
                        this.Logger.Write("Exceeded thread join timeout of {0}ms, aborting thread...", ThreadJoinTimeout, LogType.CLIENT);
                        this.readThread.Abort();
                    }
                }
                else
                {
                    this.Logger.Write("Tried to join thread from within thread, letting thread run out..", LogType.CLIENT);
                }
            }

            if (this.clientSocket != null)
            {
                this.clientSocket.Close();
            }
        }

        public void Send(byte[] payload)
        {
            this.clientSocket.Send(payload);
        }

        private Socket CreateSocket()
        {
            Socket socket = null;

            this.Logger.Write("Creating Socket...", LogType.CLIENT);

            if (this.ServerIPAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                this.Logger.Write("Created Socket (IPv6 Support)", LogType.CLIENT);
            }
            else
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.Logger.Write("Created Socket (IPv4 Support)", LogType.CLIENT);
            }

            return socket;
        }

        private void ConnectionEstablished(Socket socket)
        {
            this.clientSocket = new ClientSocket(0, socket, this.Logger);

            this.readThread = new Thread(ReadProcess);
            this.readThread.Name = Name;
            this.readThread.Start();

            this.IsConnected = true;

            this.Logger.Write("Client connected", LogType.CLIENT);
            this.OnConnected();
        }

        private void ReadProcess()
        {
            this.Logger.Write("Reading thread started.", LogType.CLIENT);

            while (this.IsConnected)
            {
                if (this.clientSocket.Socket.Poll(this.PollTimeout, SelectMode.SelectRead))
                {
                    int bufferSize = this.BufferSize;
                    byte[] buffer = new byte[bufferSize];
                    int bytesReceived = 0;

                    ByteBuffer payload = new ByteBuffer();

                    try
                    {
                        if (this.clientSocket.Socket.Poll(this.PollTimeout, SelectMode.SelectRead))
                        {
                            while (this.clientSocket.Socket.Available > 0 && (bytesReceived = this.clientSocket.Socket.Receive(buffer, 0, bufferSize, SocketFlags.None)) > 0)
                            {
                                payload.WriteBytes(buffer, 0, bytesReceived);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (!this.clientSocket.Socket.Connected)
                        {
                            this.Logger.Write(new Log(String.Format("Client error: {0}", e.Message)));
                        }
                        else
                        {
                            this.Logger.Write(new Log(String.Format("Failed to receive packet: {0}", e.Message)));
                        }
                        this.Disconnect();
                    }

                    payload.ResetPosition();
                    this.OnClientReceivedPacket(this.clientSocket, payload);
                }
            }

            this.Logger.Write("Reading thread ended.", ThreadJoinTimeout, LogType.CLIENT);
        }

        internal virtual void OnClientReceivedPacket(ClientSocket clientSocket, ByteBuffer payload)
        {
            EventHandler<ClientReceivedPacketEventArgs> receivedPacket = this.ReceivedPacket;
            if (receivedPacket != null)
            {
                ClientReceivedPacketEventArgs clientReceivedPacketEventArgs = new ClientReceivedPacketEventArgs(this, payload);
                receivedPacket(this, clientReceivedPacketEventArgs);
            }
        }

        internal virtual void OnDisconnected()
        {
            EventHandler<DisconnectedEventArgs> clientDisconnected = this.Disconnected;
            if (clientDisconnected != null)
            {
                DisconnectedEventArgs clientDisconnectedEventArgs = new DisconnectedEventArgs(this.clientSocket);
                clientDisconnected(this, clientDisconnectedEventArgs);
            }
        }

        internal virtual void OnConnected()
        {
            EventHandler<ConnectedEventArgs> clientConnected = this.Connected;
            if (clientConnected != null)
            {
                ConnectedEventArgs clientConnectedEventArgs = new ConnectedEventArgs(this.clientSocket);
                clientConnected(this, clientConnectedEventArgs);
            }
        }

        internal virtual void OnConnectError(string reason, IPAddress serverIPAddress, int serverPort, TimeSpan timeout)
        {
            EventHandler<ConnectErrorEventArgs> connectError = this.ConnectError;
            if (connectError != null)
            {
                ConnectErrorEventArgs connectErrorEventArgs = new ConnectErrorEventArgs(reason, serverIPAddress, serverPort, timeout);
                connectError(this, connectErrorEventArgs);
            }
        }

    }
}