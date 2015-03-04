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
        private Logger serverLog;
        private ServerConfig serverConfig;
        private volatile bool isListening;

        public MarryServer(ServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;
            this.serverLog = this.serverConfig.Logger;
            this.socketManager = new SocketManager(this.serverConfig);
            this.isListening = false;
        }

        public void Start()
        {
            if (!this.isListening)
            {
                this.serverLog.Write("Starting Server...", LogType.SERVER);
                this.serverThread = new Thread(ServerThread);
                this.serverThread.Name = "ServerThread";
                this.serverThread.Start();
                this.isListening = true;
            }
            else
            {
                this.serverLog.Write("Server already Online.", LogType.SERVER);
            }
        }

        public void Stop()
        {
            if (this.isListening)
            {
                this.serverLog.Write("Shutting Server down...", LogType.SERVER);
                this.isListening = false;
                this.serverThread.Join();
                this.serverLog.Write("Server Offline.", LogType.SERVER);
            }
            else
            {
                this.serverLog.Write("Server already Offline.", LogType.SERVER);
            }

        }

        private void ServerThread()
        {
            this.socketManager.Start();

            try
            {
                serverSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.SetSocketOption(SocketOptionLevel.IPv6, BaseConfig.IPV6_V6ONLY, BaseConfig.IPV6_V6ONLY_VALUE);
                serverSocket.Bind(new IPEndPoint(serverConfig.ServerIP, serverConfig.ServerPort));
                serverSocket.Listen(serverConfig.Backlog);
                this.serverLog.Write("Listening on port: {0}", serverConfig.ServerPort, LogType.SERVER);
                this.serverLog.Write("Server Online.", LogType.SERVER);
                while (this.isListening)
                {
                    if (serverSocket.Poll(this.serverConfig.PollTimeout, SelectMode.SelectRead))
                    {
                        this.socketManager.AddClient(new ClientSocket(serverSocket.Accept(), this.serverLog, this.serverConfig.Serializer));
                    }
                }

            }
            catch (Exception exception)
            {
                this.serverLog.Write(exception.Message, LogType.SERVER);
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