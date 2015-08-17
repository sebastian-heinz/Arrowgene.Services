namespace Arrowgene.Services.Network.Proxy
{
    using Arrowgene.Services.Logging;
    using Arrowgene.Services.Provider;
    using System;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Threading;

    public abstract class ProxyBase
    {
        protected AGSocket socket;
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
                    ByteBuffer payload = new ByteBuffer();

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