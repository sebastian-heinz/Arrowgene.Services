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
    using Common;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class ProxyServer : ProxyBase
    {
        private Socket serverSocket;
        private ProxyClient proxyClient;

        public ProxyServer(ProxyConfig proxyConfig)
            : base(proxyConfig)
        {
            this.proxyClient = new ProxyClient(proxyConfig);
            this.proxyClient.ReceivedPacket += proxyClient_ReceivedPacket;
        }

        public bool IsListening { get; private set; }

        public override void Start()
        {
            Thread thread = new Thread(_Start);
            thread.Name = "ProxyServer";
            thread.Start();
        }

        public override void Stop()
        {
            base.Stop();
            this.IsListening = false;
            this.proxyClient.Stop();
        }

        protected override void ReceivePacket(ProxyPacket proxyPacket)
        {
            byte[] forward = proxyPacket.Payload.GetBytes();
            this.proxyClient.Write(forward);

            proxyPacket.Traffic = ProxyPacket.TrafficType.CLIENT;
            base.ReceivePacket(proxyPacket);
        }

        private void proxyClient_ReceivedPacket(object sender, ReceivedProxyPacketEventArgs e)
        {
            byte[] forward = e.ProxyPacket.Payload.GetBytes();
            base.Write(forward);

            base.ReceivePacket(e.ProxyPacket);
        }

        private void _Start()
        {
            try
            {
                base.logger.Write("Proxy Server Started");
                this.serverSocket = this.CreateBoundServerSocket(this.ProxyConfig.ProxyEndPoint, SocketType.Stream, ProtocolType.Tcp);

                if (this.serverSocket != null)
                {
                    this.serverSocket.Listen(base.ProxyConfig.Backlog);

                    this.IsListening = true;

                    while (this.IsListening)
                    {
                        if (this.serverSocket.Poll(base.ProxyConfig.PollResponseWait, SelectMode.SelectRead))
                        {
                            base.socket = this.serverSocket.Accept();
                            base.IsRunning = true;

                            this.proxyClient.Start();

                            while (!proxyClient.IsRunning)
                            {
                                Thread.Sleep(base.ProxyConfig.ReadTimeout);
                            }

                            base.Read();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }


        private Socket CreateBoundServerSocket(IPEndPoint localEndPoint, SocketType socketType, ProtocolType protocolType)
        {
            Socket socket = null;

            if (localEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                socket = new Socket(AddressFamily.InterNetworkV6, socketType, protocolType);
                socket.SetSocketOption(SocketOptionLevel.IPv6, IP.USE_IPV6_ONLY, false);
                localEndPoint = new IPEndPoint(IPAddress.IPv6Any, localEndPoint.Port);
                Debug.WriteLine("AGSocket::CreateServerSocket: Created Socket (IPv4 and IPv6 Support)...");
            }
            else
            {
                socket = new Socket(AddressFamily.InterNetwork, socketType, protocolType);
                Debug.WriteLine("AGSocket::CreateServerSocket: Created Socket (IPv4 Support)...");
            }

            if (socket != null)
            {
                socket.Bind(localEndPoint);
                Debug.WriteLine(string.Format("Socket bound to {0}", localEndPoint.ToString()));
            }

            return socket;
        }

    }
}