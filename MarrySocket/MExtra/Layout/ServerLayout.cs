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
    using MarrySocket.MExtra.Logging;
    using MarrySocket.MServer;

    public abstract class ServerLayout
    {
        protected EntitiesContainer entitiesContainer;
        protected ServerConfig serverConfig;
        protected MarryServer marryServer;
        protected Logger logger;

        protected ServerLayout()
        {
            this.serverConfig = new ServerConfig();
            this.entitiesContainer = new EntitiesContainer(this.serverConfig);
            this.logger = this.entitiesContainer.ServerLog;

            this.entitiesContainer.OnClientConnected = this.onClientConnected;
            this.entitiesContainer.OnClientDisconnected = this.onClientDisconnected;
            this.entitiesContainer.ServerLog.OnLogWrite = this.OnLogWrite;
            this.entitiesContainer.ReceivedObjectPacket += EntitiesContainer_ReceivedObjectPacket;
            this.marryServer = new MarryServer(this.entitiesContainer);
        }

        protected abstract void Handle(int packetId, object receivedClass, ClientSocket clientSocket);

        private void EntitiesContainer_ReceivedObjectPacket(object sender, ReceiveObjectEventArgs e)
        {
            this.Handle(e.PacketId, e.MyObject, e.ClientSocket);
        }

        protected virtual void OnLogWrite(Log log)
        {

        }

        protected virtual void onClientDisconnected(ClientSocket clientSocket)
        {

        }

        protected virtual void onClientConnected(ClientSocket clientSocket)
        {

        }

        public virtual void Start()
        {
            this.marryServer.Start();
        }

        public virtual void Stop()
        {
            this.marryServer.Stop();
        }

    }
}
