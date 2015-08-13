namespace Arrowgene.Services.Networking.Proxy
{
    using Arrowgene.Services.Logging;
    using Arrowgene.Services.Provider;
    using System;
    using System.Diagnostics;
    using System.Net.Sockets;

    public abstract class ProxyBase
    {
        protected AGSocket socket;
        protected byte[] buffer;

        protected ProxyBase(ProxyConfig proxyConfig)
        {
            this.ProxyConfig = proxyConfig;
            this.buffer = new byte[this.ProxyConfig.BufferSize];
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
                ByteBuffer payload = new ByteBuffer();
                if (!this.socket.Connected)
                {
                    this.Stop();
                }

                if (this.socket.Poll(100, SelectMode.SelectRead))
                {
                    try
                    {
                        int received = 0;

                        while ((received = this.socket.Receive(this.buffer, 0, this.ProxyConfig.BufferSize, SocketFlags.None)) > 0)
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