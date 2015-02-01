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
namespace MarrySocket.MClient
{
    using MarrySocket.MExtra.Logging;
    using System;
    using System.Net;
    using System.Net.Sockets;

    public class MarryClient
    {
        private Action<string> onConnected;
        private Action<string> onDisconnected;
        private ClientConfig clientConfig;
        private ServerSocket serverSocket;
        private SocketManager socketManager;
        private EntitiesContainer entitiesContainer;
        private Logger logger;

        public MarryClient(EntitiesContainer entitiesContainer)
        {
            this.entitiesContainer = entitiesContainer;
            this.socketManager = new SocketManager(this.entitiesContainer);
            this.clientConfig = this.entitiesContainer.ClientConfig;
            this.logger = this.entitiesContainer.ClientLog;
            this.serverSocket = this.entitiesContainer.ServerSocket;
            this.onConnected = this.entitiesContainer.OnConnected;
            this.onDisconnected = this.entitiesContainer.OnDisconnected;
        }

        public void Connect()
        {
            if (!this.entitiesContainer.IsConnected)
            {
                if (this.clientConfig.ServerIP.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    this.logger.Write("Creating IPv4 Socket...", LogType.CLIENT);
                    this.serverSocket.Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                }
                else
                {
                    this.logger.Write("Creating IPv6 Socket...", LogType.CLIENT);
                    this.serverSocket.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }

                IPEndPoint remoteEP = new IPEndPoint(this.clientConfig.ServerIP, this.clientConfig.ServerPort);

                try
                {
                    this.serverSocket.Socket.Connect(remoteEP);
                    this.socketManager.Start();
                    this.entitiesContainer.IsConnected = true;

                    if (this.onConnected != null)
                    {
                        this.onConnected("Client connected");
                    }
                    else
                    {
                        this.logger.Write("Client connected", LogType.CLIENT);
                    }
                }
                catch (SocketException socketException)
                {
                    this.Disconnect(socketException.SocketErrorCode.ToString());
                }
                catch (Exception ex)
                {
                    this.Disconnect(ex.ToString());
                }
            }
        }

        public void Disconnect(string reason)
        {
            this.entitiesContainer.IsConnected = false;
            this.socketManager.Stop();
            this.serverSocket.Close();
            if (this.onDisconnected != null)
            {
                this.onDisconnected(reason);
            }
            else
            {
                this.logger.Write("Client disconnected: {0}", reason, LogType.CLIENT);
            }
        }

    }
}
