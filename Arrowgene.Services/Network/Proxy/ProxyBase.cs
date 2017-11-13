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
    using Buffers;
    using Logging;

    public abstract class ProxyBase
    {
        protected Socket socket;
        protected byte[] buffer;
        protected Logger logger;

        protected ProxyBase(ProxyConfig proxyConfig)
        {
            this.ProxyConfig = proxyConfig;
            this.buffer = new byte[this.ProxyConfig.BufferSize];
            this.logger = proxyConfig.Logger;
        }

        public event EventHandler<ReceivedProxyPacketEventArgs> ReceivedPacket;

        public ProxyConfig ProxyConfig { get; private set; }
        public bool IsRunning { get; protected set; }

        public virtual void Start()
        {
            this.IsRunning = true;
        }

        public virtual void Stop()
        {
            this.IsRunning = false;
        }

        protected virtual void ReceivePacket(ProxyPacket proxyPacket)
        {
            this.OnReceivedPacket(proxyPacket);
        }

        public void Write(byte[] data)
        {
            this.socket.Send(data);
        }

        protected void Read()
        {
            while (this.IsRunning)
            {
                if (this.socket.Connected)
                {
                    IBuffer payload = new ByteBuffer();

                    try
                    {
                        int received = 0;

                        while (this.socket.Poll(this.ProxyConfig.PollResponseWait, SelectMode.SelectRead) && (received = this.socket.Receive(this.buffer, 0, this.ProxyConfig.BufferSize, SocketFlags.None)) > 0)
                        {
                            payload.WriteBytes(this.buffer, 0, received);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                        this.Stop();
                    }

                    if (payload.Size > 0)
                    {
                        ProxyPacket proxyPacket = new ProxyPacket(payload);
                        this.ReceivePacket(proxyPacket);
                    }

                    Thread.Sleep(this.ProxyConfig.ReadTimeout);
                }
                else
                {
                    this.Stop();
                }
            }
        }

        protected void OnReceivedPacket(ProxyPacket proxyPacket)
        {
            EventHandler<ReceivedProxyPacketEventArgs> receivedPacket = this.ReceivedPacket;

            if (receivedPacket != null)
            {
                ReceivedProxyPacketEventArgs receivedProxyPacketEventArgs = new ReceivedProxyPacketEventArgs(proxyPacket);
                receivedPacket(this, receivedProxyPacketEventArgs);
            }
        }
    }
}