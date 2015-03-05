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
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class MarryServer
    {
        private Socket serverSocket;
        private SocketManager socketManager;
        private Thread serverThread;
        private volatile bool isListening;

        public MarryServer(ServerConfig serverConfig)
        {
            this.ServerConfig = serverConfig;
            this.Logger = this.ServerConfig.Logger;
            this.socketManager = new SocketManager(this.ServerConfig);
            this.isListening = false;
        }


        public ServerConfig ServerConfig { get; private set; }
        public Logger Logger { get; private set; }
        public bool IsListening { get { return this.ServerConfig.IsListening; } }

        public event EventHandler<ClientConnectedEventArgs> ClientConnected
        {
            add { this.ServerConfig.ClientConnected += value; }
            remove { this.ServerConfig.ClientConnected -= value; }
        }

        public event EventHandler<ReceivedPacketEventArgs> ReceivedPacket
        {
            add { this.ServerConfig.ReceivedPacket += value; }
            remove { this.ServerConfig.ReceivedPacket -= value; }
        }

        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected
        {
            add { this.ServerConfig.ClientDisconnected += value; }
            remove { this.ServerConfig.ClientDisconnected -= value; }
        }


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
                serverSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.SetSocketOption(SocketOptionLevel.IPv6, BaseConfig.IPV6_V6ONLY, BaseConfig.IPV6_V6ONLY_VALUE);
                serverSocket.Bind(new IPEndPoint(ServerConfig.ServerIP, ServerConfig.ServerPort));
                serverSocket.Listen(ServerConfig.Backlog);
                this.Logger.Write("Listening on port: {0}", ServerConfig.ServerPort, LogType.SERVER);
                this.Logger.Write("Server Online.", LogType.SERVER);
                while (this.isListening)
                {
                    if (serverSocket.Poll(this.ServerConfig.PollTimeout, SelectMode.SelectRead))
                    {
                        this.socketManager.AddClient(new ClientSocket(serverSocket.Accept(), this.Logger, this.ServerConfig.Serializer));
                    }
                }

            }
            catch (Exception exception)
            {
                this.Logger.Write(exception.Message, LogType.SERVER);
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
            }

        }
    }
}