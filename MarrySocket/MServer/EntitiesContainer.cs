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
    using System.Collections.Generic;

    public class EntitiesContainer
    {
        public EventHandler<ReceiveObjectEventArgs> ReceivedObjectPacket;

        public EntitiesContainer(ServerConfig serverConfig)
        {
            this.ServerConfig = serverConfig;
            this.ClientList = new ClientList();
            this.ServerLog = new Logger();
        }

        public Action<ClientSocket> OnClientDisconnected { get; set; }
        public Action<ClientSocket> OnClientConnected { get; set; }
        public ServerConfig ServerConfig { get; private set; }
        public ClientList ClientList { get; private set; }
        public Logger ServerLog { get; private set; }

    }
}