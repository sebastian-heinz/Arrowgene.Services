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
    using MarrySocket.MBase;
    using System;
    using System.Collections.Generic;


    public class EntitiesContainer
    {
        public EventHandler<ReceiveObjectEventArgs> ReceivedObjectPacket;

        public EntitiesContainer(ClientConfig clientConfig)
        {
            this.ClientConfig = clientConfig;
            this.ServerSocket = new ServerSocket();
            this.IsConnected = false;
        }

        public Action<string> OnConnected { get; set; }
        public Action<string> OnDisconnected { get; set; }
        public ClientConfig ClientConfig { get; private set; }
        public ServerSocket ServerSocket { get; set; }
        public bool IsConnected { get; internal set; }
    }
}
