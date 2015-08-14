namespace Arrowgene.Services.Network.Proxy
{
    using System;
    using System.Diagnostics;
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
            base.socket = new AGSocket();
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

    }
}
