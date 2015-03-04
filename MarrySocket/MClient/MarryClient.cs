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
        private ClientConfig clientConfig;
        private SocketManager socketManager;
        private Logger logger;
        private bool isValidConfiguration;

        public MarryClient(ClientConfig clientConfig)
        {
            this.clientConfig = clientConfig;
            this.isValidConfiguration = SanityCheck();
            if (this.isValidConfiguration)
            {
                this.logger = this.clientConfig.Logger;
                this.socketManager = new SocketManager(this.clientConfig);
                this.ServerSocket = new ServerSocket(this.CreateSocket(), this.logger, this.clientConfig.Serializer);
            }
            else
            {
                this.logger.Write("Bad Configuration", LogType.ERROR);
            }
        }

        public ServerSocket ServerSocket { get; private set; }

        private bool SanityCheck()
        {
            //TODO check clientConfig
            return true;
        }

        private Socket CreateSocket()
        {
            Socket socket;
            if (this.clientConfig.ServerIP.AddressFamily == AddressFamily.InterNetworkV6)
            {
                this.logger.Write("Creating IPv4 Socket...", LogType.CLIENT);
                socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            }
            else
            {
                this.logger.Write("Creating IPv6 Socket...", LogType.CLIENT);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            return socket;
        }

        public void Connect()
        {
            if (!this.clientConfig.IsConnected && this.isValidConfiguration)
            {
                try
                {
                    IPEndPoint remoteEP = new IPEndPoint(this.clientConfig.ServerIP, this.clientConfig.ServerPort);
                    this.ServerSocket.Socket.Connect(remoteEP);
                    this.socketManager.Start();
                    this.clientConfig.IsConnected = true;
                    this.clientConfig.OnConnected(this.ServerSocket);
                    this.logger.Write("Client connected", LogType.CLIENT);
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
            this.socketManager.Stop();
            this.ServerSocket.Close();
            this.clientConfig.OnDisconnected(this.ServerSocket);
            this.logger.Write("Client disconnected: {0}", reason, LogType.CLIENT);
            this.clientConfig.IsConnected = false;
        }

    }
}
