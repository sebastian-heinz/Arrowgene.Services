namespace SvrKit.Networking.Proxy
{
    using System;
    using System.Net.Sockets;
    using System.Threading;

    public class ProxyClient : ProxyBase
    {

        public ProxyClient(ProxyConfig proxyConfig)
            : base(proxyConfig)
        {

        }

        public void Connect(Socket serverSocket)
        {
            this.serverSocket = serverSocket;
            Thread thread = new Thread(_Connect);
            thread.Start();
        }

        private void _Connect()
        {
            this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                this.clientSocket.Connect(base.ProxyConfig.ServerEndPoint);
                this.isClientConnected = true;
                this.proxyHost.WriteOutput("Proxy Connected to Server");
                this.Listen();
            }
            catch (Exception ex)
            {
                this.proxyHost.WriteOutput(ex.ToString());
            }
        }

        protected override void ReceivedPacket(ProxyPacket packetReader)
        {
            packetReader.Traffic = ProxyPacket.TrafficType.SERVER;
            this.proxyHost.WriteLog(packetReader);
            this.serverSocket.Send(packetReader.RawPacket);
        }

    }
}
