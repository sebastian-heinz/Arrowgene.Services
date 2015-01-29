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
        private EntitiesContainer entitieContainer;
        private SocketManager socketManager;
        private ServerConfig serverConfig;
        private Logger serverLog;
        private volatile bool isListening;
        private Thread serverThread;

        public MarryServer(EntitiesContainer entitieContainer)
        {
            this.entitieContainer = entitieContainer;
            this.serverConfig = this.entitieContainer.ServerConfig;
            this.serverLog = this.entitieContainer.ServerLog;
            this.socketManager = new SocketManager(this.entitieContainer);
            this.isListening = false;
        }

        public void Start()
        {
            if (!this.isListening)
            {
                this.serverLog.Clear();
                this.isListening = true;
                this.serverThread = new Thread(ServerThread);
                this.serverThread.Name = "ServerThread";
                this.serverThread.Start();
            }
        }

        public void Stop()
        {
            if (this.isListening)
            {
                this.isListening = false;
                this.serverThread.Join();
                this.serverLog.Write("Server Offline!");
                if (this.serverSocket.Connected)
                    this.serverSocket.Shutdown(SocketShutdown.Both);
                this.serverSocket.Close();
            }
        }

        public void ServerThread()
        {
            this.socketManager.Start();
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(serverConfig.ServerIP, serverConfig.ServerPort));
                serverSocket.Listen(serverConfig.Backlog);
                this.serverLog.Write("Listening on port: " + serverConfig.ServerPort);
                this.serverLog.Write("Server Online!");
                while (this.isListening)
                {
                    if (serverSocket.Poll(this.serverConfig.PollTimeout, SelectMode.SelectRead))
                        this.socketManager.AddClient(new ClientSocket(serverSocket.Accept(), this.serverLog));
                }
            }
            catch (Exception exception)
            {
                this.serverLog.Write(exception.Message);
            }
            this.socketManager.Stop();
        }
    }
}