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
namespace Arrowgene.Services.Network.Proxy
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class ProxyClient : ProxyBase
    {

        public ProxyClient(ProxyConfig proxyConfig)
            : base(proxyConfig)
        {

        }

        public override void Start()
        {
            base.Start();
            Thread thread = new Thread(_Connect);
            thread.Name = "ProxyClient";
            thread.Start();
        }

        protected override void ReceivePacket(ProxyPacket proxyPacket)
        {
            proxyPacket.Traffic = ProxyPacket.TrafficType.SERVER;
            base.ReceivePacket(proxyPacket);
        }

        private void _Connect()
        {
            base.socket = this.CreateSocket(base.ProxyConfig.ServerEndPoint, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                base.socket.Connect(base.ProxyConfig.ServerEndPoint);
                base.IsRunning = true;
                base.logger.Write("Proxy Client Connected");
                base.Read();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                this.Stop();
            }
        }


        private Socket CreateSocket(IPEndPoint localEndPoint, SocketType socketType, ProtocolType protocolType)
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

    }
}
