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
namespace SvrKit.Networking.MarrySocket.MServer
{
    using SvrKit.Logging;
    using SvrKit.Networking.MarrySocket.MBase;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// Managed server for handling multiple connection.
    /// </summary>
    public class MarryServer
    {
        private Socket serverSocket;
        private SocketManager socketManager;
        private Thread serverThread;
        private bool isListening;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarryServer"/> class.
        /// </summary>
        /// <param name="serverConfig">Configuration for the server.</param>
        public MarryServer(ServerConfig serverConfig)
        {
            this.ServerConfig = serverConfig;
            this.Logger = this.ServerConfig.Logger;
            this.socketManager = new SocketManager(this.ServerConfig);
            this.isListening = false;
        }

        /// <summary>
        /// Configuration of the server.
        /// </summary>
        public ServerConfig ServerConfig { get; private set; }

        /// <summary>
        /// Logging of server events.
        /// </summary>
        public Logger Logger { get; private set; }

        /// <summary>
        /// Server status.
        /// </summary>
        public bool IsListening { get { return this.isListening; } }

        /// <summary>
        /// Occurs when a new client connected.
        /// </summary>
        public event EventHandler<ClientConnectedEventArgs> ClientConnected
        {
            add { this.ServerConfig.ClientConnected += value; }
            remove { this.ServerConfig.ClientConnected -= value; }
        }

        /// <summary>
        /// Occurs when a new packet arrived.
        /// </summary>
        public event EventHandler<ReceivedPacketEventArgs> ReceivedPacket
        {
            add { this.ServerConfig.ReceivedPacket += value; }
            remove { this.ServerConfig.ReceivedPacket -= value; }
        }

        /// <summary>
        /// Occurs when a client disconnected.
        /// </summary>
        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected
        {
            add { this.ServerConfig.ClientDisconnected += value; }
            remove { this.ServerConfig.ClientDisconnected -= value; }
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        public void Start()
        {
            if (!this.isListening)
            {
                this.Logger.Write("Starting Server...", LogType.SERVER);
                this.serverThread = new Thread(ServerThread);
                this.serverThread.Name = "ServerThread";
                this.serverThread.Start();
                this.isListening = true;
            }
            else
            {
                this.Logger.Write("Server already Online.", LogType.SERVER);
            }
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop()
        {
            if (this.isListening)
            {
                this.Logger.Write("Shutting Server down...", LogType.SERVER);
                this.isListening = false;
                this.serverThread.Join();
                this.Logger.Write("Server Offline.", LogType.SERVER);
            }
            else
            {
                this.Logger.Write("Server already Offline.", LogType.SERVER);
            }

        }

        private void ServerThread()
        {
            this.socketManager.Start();

            try
            {
                this.serverSocket = this.CreateSocket(this.ServerConfig.ServerIP);

                if (this.serverSocket != null)
                {
                    this.serverSocket.Bind(new IPEndPoint(this.ServerConfig.ServerIP, this.ServerConfig.ServerPort));
                    this.serverSocket.Listen(this.ServerConfig.Backlog);
                    this.Logger.Write("Listening on port: {0}", this.ServerConfig.ServerPort, LogType.SERVER);
                    this.Logger.Write("Server Online.", LogType.SERVER);
                    while (this.isListening)
                    {
                        if (this.serverSocket.Poll(this.ServerConfig.PollTimeout, SelectMode.SelectRead))
                        {
                            this.socketManager.AddClient(new ClientSocket(this.serverSocket.Accept(), this.Logger, this.ServerConfig.Serializer));
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
                if (this.serverSocket.Connected)
                {
                    this.serverSocket.Shutdown(SocketShutdown.Both);
                }

                this.serverSocket.Close();
                this.socketManager.Stop();
                this.isListening = false;
                this.Logger.Write("Server stopped.", LogType.SERVER);
            }

        }

        private Socket CreateSocket(IPAddress ipAddress)
        {
            Socket socket = null;
            if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.IPv6, BaseConfig.USE_IPV6_ONLY, false);
                this.Logger.Write("Created IPv4 and IPv6 Socket...", LogType.SERVER);
            }
            else
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.Logger.Write("Created IPv4 Socket...", LogType.CLIENT);
            }
            return socket;
        }


    }
}