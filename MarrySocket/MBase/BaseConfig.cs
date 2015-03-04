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
    using MarrySocket.MExtra.Logging;
    using MarrySocket.MExtra.Serialization;
    using System.Net;
    using System.Net.Sockets;

    public abstract class BaseConfig
    {
        /*
         * https://msdn.microsoft.com/en-us/library/system.net.sockets.socketoptionname.aspx
         * 
         * IPv6Only	
         * Indicates if a socket created for the AF_INET6 address family is restricted to IPv6 communications only.
         * Sockets created for the AF_INET6 address family may be used for both IPv6 and IPv4 communications.
         * Some applications may want to restrict their use of a socket created for the AF_INET6 address family to IPv6 communications only.
         * When this value is non-zero (the default on Windows), a socket created for the AF_INET6 address family can be used to send and receive IPv6 packets only.
         * When this value is zero, a socket created for the AF_INET6 address family can be used to send and receive packets to and from an IPv6 address or an IPv4 address.
         * Note that the ability to interact with an IPv4 address requires the use of IPv4 mapped addresses.
         * This socket option is supported on Windows Vista or later.
         * 
         */

        //TODO MS below Vista (XP) does not support DualMode Sockets
        //Check for Operating system
        //Create IpV4 Socket if below Vista
        //Notify Ipv6 Client about issue, ask to use Ipv4
        public const SocketOptionName IPV6_V6ONLY = (SocketOptionName)27;
        public const bool IPV6_V6ONLY_VALUE = false;

        public BaseConfig(ISerialization serializer)
        {
            this.PollTimeout = 100;
            this.BufferSize = 1500;
            this.Logger = new Logger();
            this.Serializer = serializer;
        }


        public ISerialization Serializer { get; private set; }

        public IPAddress ServerIP { get; set; }
        public int ServerPort { get; set; }
        public int PollTimeout { get; set; }
        public int BufferSize { get; set; }
        public Logger Logger { get; private set; }

    }
}

