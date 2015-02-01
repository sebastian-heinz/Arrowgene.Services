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
namespace MarrySocket.MBase
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;

    public abstract class BaseConfig
    {
        public const SocketOptionName IPv6V6Only = (SocketOptionName)27;
        public const int IPv6V6OnlyValue = 0;


        public IPAddress ServerIP { get; set; }
        public int ServerPort { get; set; }

        public int PollTimeout { get; set; }
        public int BufferSize { get; set; }

        public BaseConfig()
        {
            this.PollTimeout = 100;
            this.BufferSize = 1500;
        }
    }
}

