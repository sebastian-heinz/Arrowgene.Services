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
    using System.Net;

    public class ServerConfig : BaseConfig
    {
        public ServerConfig()
            : base()
        {
            base.ServerIP = IPAddress.IPv6Any;
            base.ServerPort = 2345;

            this.Backlog = 10;
            this.ReadTimeout = 20;
            this.ManagerCount = 5;
            this.LogUnknownPacket = true;
        }

        public bool LogUnknownPacket { get; set; }
        public int ManagerCount { get; set; }
        public int Backlog { get; set; }
        public int ReadTimeout { get; set; }
    }
}

