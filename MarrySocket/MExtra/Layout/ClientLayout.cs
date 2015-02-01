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
namespace MarrySocket.MExtra.Layout
{
    using MarrySocket.MClient;
    using MarrySocket.MExtra.Logging;
    using System.Net;

    public abstract class ClientLayout
    {
        protected MarryClient marryClient;
        protected ClientConfig clientConfig;
        protected EntitiesContainer entitiesContainer;
        protected Logger logger;

        public ClientLayout()
        {
            this.clientConfig = new ClientConfig();
            this.entitiesContainer = new EntitiesContainer(this.clientConfig);
            this.logger = this.entitiesContainer.ClientLog;
            this.entitiesContainer.OnConnected = this.OnConnected;
            this.entitiesContainer.OnDisconnected = this.OnDisconnected;

            this.marryClient = new MarryClient(this.entitiesContainer);
            this.entitiesContainer.ReceivedObjectPacket += EntitiesContainer_ReceivedObjectPacket;
        }

        public bool IsConnected { get { return this.entitiesContainer.IsConnected; } }

        private void EntitiesContainer_ReceivedObjectPacket(object sender, ReceiveObjectEventArgs e)
        {
            this.Handle(e.PacketId, e.MyObject, e.ServerSocket);
        }

        protected abstract void Handle(int packetId, object receivedClass, ServerSocket serverSocket);

        protected virtual void OnDisconnected(string reason)
        {
            logger.Write("Client disconnected: {0}", reason, LogType.CLIENT);
        }

        protected virtual void OnConnected(string reason)
        {
            logger.Write(reason, LogType.CLIENT);
        }

        public virtual void Start(IPAddress ipAddress, int port)
        {
            this.clientConfig.ServerIP = ipAddress;
            this.clientConfig.ServerPort = port;
            this.logger.Write("Connecting to: {0}", ipAddress.ToString(), LogType.CLIENT);
            this.marryClient.Connect();
        }

        public virtual void Stop()
        {
            this.marryClient.Disconnect("Client stopped");
        }
    }
}
