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
namespace Arrowgene.Services.Network
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// 
    /// </summary>
    public class AGSocket
    {
        #region static
        /// <summary>
        /// Creates a <see cref="Socket"/> bound to a specified <see cref="IPEndPoint"/>.
        /// </summary>
        /// <param name="localEndPoint"></param>
        /// <returns></returns>
        public static Socket CreateBoundServerSocket(IPEndPoint localEndPoint, SocketType socketType, ProtocolType protocolType)
        {
            Socket socket = null;

            if (localEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                socket = new Socket(AddressFamily.InterNetworkV6, socketType, protocolType);
                socket.SetSocketOption(SocketOptionLevel.IPv6, AGSocket.USE_IPV6_ONLY, false);
                localEndPoint = new IPEndPoint(IPAddress.IPv6Any, localEndPoint.Port);
                Debug.WriteLine("AGSocket::CreateServerSocket: Created Socket (IPv4 and IPv6 Support)...");
            }
            else
            {
                socket = new Socket(AddressFamily.InterNetwork, socketType, protocolType);
                Debug.WriteLine("AGSocket::CreateServerSocket: Created Socket (IPv4 Support)...");
            }

            if(socket != null)
            {
                socket.Bind(localEndPoint);
                Debug.WriteLine(string.Format("Socket bound to {0}", localEndPoint.ToString()));
            }

            return socket;
        }

        /// <summary>
        /// Creates a new <see cref="Socket"/>.
        /// </summary>
        /// <param name="localEndPoint"></param>
        /// <param name="socketType"></param>
        /// <param name="protocolType"></param>
        /// <returns></returns>
        public static Socket CreateSocket(IPEndPoint localEndPoint, SocketType socketType, ProtocolType protocolType)
        {
            Socket socket = null;
            if (localEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                socket = new Socket(AddressFamily.InterNetworkV6, socketType, protocolType);
                Debug.WriteLine("AGSocket::CreateSocket: Created IPv6 Socket...");
            }
            else
            {
                socket = new Socket(AddressFamily.InterNetwork, socketType, protocolType);
                Debug.WriteLine("AGSocket::CreateSocket: Created IPv4 Socket...");
            }
            return socket;
        }

        /// <summary>
        /// Connects the specified socket.
        /// </summary>
        /// <param name="endpoint">The IP endpoint.</param>
        /// <param name="timeout">The timeout.</param>
        public static bool ConnectTest(IPEndPoint endpoint, TimeSpan timeout)
        {
            Socket socket = AGSocket.CreateSocket(endpoint, SocketType.Stream, ProtocolType.Tcp);
            IAsyncResult result = socket.BeginConnect(endpoint, null, null);

            bool success = result.AsyncWaitHandle.WaitOne(timeout, true);

            if (socket.Connected && success)
            {
                socket.EndConnect(result);
                return true;
            }
            else
            {
                socket.Close();
                return false;
            }
        }

        /// <summary>
        /// Connects the specified socket.
        /// </summary>
        /// <param name="ipAddress">IP endpoint</param>
        /// <param name="port">Port</param>
        /// <param name="timeout">timeout</param>
        public static bool ConnectTest(IPAddress ipAddress, int port, TimeSpan timeout)
        {
            return AGSocket.ConnectTest(new IPEndPoint(ipAddress, port), timeout);
        }

        #endregion static

        /// <summary>
        ///https://msdn.microsoft.com/en-us/library/system.net.sockets.socketoptionname.aspx
        /// IPv6Only	
        /// Indicates if a socket created for the AF_INET6 address family is restricted to IPv6 communications only.
        /// Sockets created for the AF_INET6 address family may be used for both IPv6 and IPv4 communications.
        /// Some applications may want to restrict their use of a socket created for the AF_INET6 address family to IPv6 communications only.
        /// When this value is non-zero (the default on Windows), a socket created for the AF_INET6 address family can be used to send and receive IPv6 packets only.
        /// When this value is zero, a socket created for the AF_INET6 address family can be used to send and receive packets to and from an IPv6 address or an IPv4 address.
        /// Note that the ability to interact with an IPv4 address requires the use of IPv4 mapped addresses.
        /// This socket option is supported on Windows Vista or later.
        /// </summary>
        public const SocketOptionName USE_IPV6_ONLY = (SocketOptionName)27;


    }
}
