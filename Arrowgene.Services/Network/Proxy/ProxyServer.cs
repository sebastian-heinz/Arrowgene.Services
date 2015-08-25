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
    using System.Net.Sockets;
    using System.Threading;

    public class ProxyServer : ProxyBase
    {
        private AGSocket serverSocket;
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
                this.serverSocket = new AGSocket();

                if (this.serverSocket != null)
                {
                    this.serverSocket.Bind(this.ProxyConfig.ProxyEndPoint);
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

    }
}