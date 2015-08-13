namespace Arrowgene.Services.Networking.Proxy
{
    using Arrowgene.Services.Logging;
    using System.Net;

    public class ProxyConfig
    {
        public Logger Logger { get; private set; }
        public IPAddress ProxyListenIP { get; private set; }
        public int ProxyPort { get; private set; }
        public IPAddress ServerIP { get; private set; }
        public int ServerPort { get; private set; }
        public int BufferSize { get; set; }
        public IPEndPoint ProxyEndPoint { get { return new IPEndPoint(this.ProxyListenIP, this.ProxyPort); } }
        public IPEndPoint ServerEndPoint { get { return new IPEndPoint(this.ServerIP, this.ServerPort); } }

        public ProxyConfig(IPAddress proxyListenIP, int proxyPort, IPAddress serverIP, int serverPort)
        {
            this.Logger = new Logger();
            this.ProxyPort = proxyPort;
            this.ProxyListenIP = proxyListenIP;
            this.ServerIP = serverIP;
            this.ServerPort = serverPort;
            this.LoadDefaultConfig();
        }

        public void LoadDefaultConfig()
        {
            this.BufferSize = 10000;
        }

    }
}