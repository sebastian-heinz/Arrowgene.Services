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
namespace Arrowgene.Services.Network.ManagedConnection.Client
{
    using Arrowgene.Services.Network.ManagedConnection.Serialization;
    using Event;
    using Exceptions;
    using Logging;
    using Packet;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class ManagedClient
    {
        private const string DEFAULT_NAME = "Managed Client";
        private const int THREAD_JOIN_TIMEOUT = 1000;

        private ClientSocket clientSocket;
        private Thread readThread;
        private PacketManager packetManager;
        private ISerializer serializer;

        public ManagedClient(IPAddress serverIPAddress, int serverPort, ISerializer serializer, Logger logger)
        {
            if (serverIPAddress == null || serverPort <= 0)
                throw new InvalidParameterException(String.Format("IPAddress({0}) or Port({1}) invalid", serverIPAddress, serverPort));

            this.serializer = serializer;
            this.ServerIPAddress = serverIPAddress;
            this.ServerPort = serverPort;
            this.Logger = logger;

            this.IsConnected = false;
            this.PollTimeout = 10;
            this.BufferSize = 1024;

            this.packetManager = new PacketManager(this.serializer, this.Logger);
        }

        public ManagedClient(IPAddress serverIPAddress, int serverPort) : this(serverIPAddress, serverPort, new BinaryFormatterSerializer(), new Logger(DEFAULT_NAME))
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
        public event EventHandler<ReceivedPacketEventArgs> ReceivedPacket;

        public void Connect()
        {
            try
            {
                Socket socket = this.CreateSocket();

                if (socket != null)
                {
                    this.clientSocket = new ClientSocket(socket, this.serializer, this.Logger);
                    this.clientSocket.Socket.Connect(this.ServerIPAddress, this.ServerPort);

                    this.readThread = new Thread(ReadProcess);
                    this.readThread.Name = DEFAULT_NAME;
                    this.readThread.Start();

                    this.IsConnected = true;

                    this.Logger.Write("Client connected", LogType.CLIENT);
                    this.OnConnected();
                }
                else
                {
                    this.Logger.Write("Client could notconnect.", LogType.SERVER);
                }
            }
            catch (Exception exception)
            {
                this.Logger.Write(exception.Message, LogType.ERROR);
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
                    if (this.readThread.Join(THREAD_JOIN_TIMEOUT))
                    {
                        this.Logger.Write("Reading thread ended clean.", LogType.CLIENT);
                    }
                    else
                    {
                        this.Logger.Write("Exceeded thread join timeout of {0}ms, aborting thread...", THREAD_JOIN_TIMEOUT, LogType.CLIENT);
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

        public void SendObject(int packetId, object myObject)
        {
            this.clientSocket.SendObject(packetId, myObject);
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

        private void ReadProcess()
        {
            byte[] headerBuffer = new byte[ManagedPacket.HEADER_SIZE];
            byte[] payload;

            this.Logger.Write("Reading thread started.", LogType.CLIENT);

            while (this.IsConnected)
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

                    Int32 packetId = BitConverter.ToInt32(headerBuffer, ManagedPacket.HEADER_PAYLOAD_SIZE);

                    if (this.packetManager.CheckPacketId(packetId))
                    {
                        Int32 payloadSize = BitConverter.ToInt32(headerBuffer, 0);

                        if (payloadSize <= 0)
                        {
                            this.Logger.Write("Message length ({0}bytes) is zero or less", payloadSize, LogType.CLIENT);
                            this.Disconnect();
                            break;
                        }

                        if (payloadSize > this.BufferSize)
                        {
                            this.Logger.Write("Message length ({0}bytes) is larger than maximum message size ({1}bytes)", payloadSize, this.BufferSize, LogType.CLIENT);
                            this.Disconnect();
                            break;
                        }

                        payload = new byte[payloadSize];
                        int bytesReceived = 0;

                        //TODO TRY CATCH, Server disconnects client during read, will crash.
                        while (bytesReceived < payloadSize)
                        {
                            if (this.clientSocket.Socket.Poll(this.PollTimeout, SelectMode.SelectRead))
                                bytesReceived += this.clientSocket.Socket.Receive(payload, bytesReceived, payloadSize - bytesReceived, SocketFlags.None);
                        }

                        ManagedPacket managedPacket = new ManagedPacket(packetId, headerBuffer, payload);

                        if (packetManager.Handle(this.clientSocket, managedPacket))
                        {
                            this.OnReceivedPacket(packetId, managedPacket);
                        }
                        else
                        {
                            // packet could not be handled
                        }

                    }
                }
            }

            this.Logger.Write("Reading thread ended.", THREAD_JOIN_TIMEOUT, LogType.CLIENT);
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