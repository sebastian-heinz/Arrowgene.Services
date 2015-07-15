namespace SvrKit.Networking.Proxy
{
    using System;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Threading;

    public class ProxyClient : ProxyBase
    {
        public ProxyClient(ProxyConfig proxyConfig)
            : base(proxyConfig)
        {

        }

        public override void Connect()
        {
            Thread thread = new Thread(_Connect);
            thread.Name = "ProxyClient";
            thread.Start();
        }

        public override void Disconnect()
        {
            base.IsConnected = false;
        }

        protected override void ReceivePacket(ProxyPacket proxyPacket)
        {
            proxyPacket.Traffic = ProxyPacket.TrafficType.CLIENT;
            base.ReceivePacket(proxyPacket);
        }

        private void _Connect()
        {
            base.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                base.socket.Connect(base.ProxyConfig.ServerEndPoint);
                base.IsConnected = true;
                base.Read();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                this.Disconnect();
            }
        }

    }
}
