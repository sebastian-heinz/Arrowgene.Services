namespace Arrowgene.Services.Network.Proxy
{
    using Arrowgene.Services.Logging;
    using Exception;
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
        public int ReadTimeout { get; set; }
        public int Backlog { get; set; }
        public int PollResponseWait { get; set; }

        public ProxyConfig(IPAddress proxyListenIP, int proxyPort, IPAddress serverIP, int serverPort)
        {
            if (proxyListenIP == null || proxyPort <= 0 || serverIP == null || serverPort <= 0)
                throw new InvalidParameterException(string.Format("ProxyServer(IP:{0}/Port:{1}) or ProxyDestination(IP:{2}/Port:{3}) invalid",
                    proxyListenIP, proxyPort, serverIP, serverPort));

            this.Logger = new Logger("ProxyServer");
            this.ProxyPort = proxyPort;
            this.ProxyListenIP = proxyListenIP;
            this.ServerIP = serverIP;
            this.ServerPort = serverPort;

            this.LoadDefaultConfig();
        }

        public void LoadDefaultConfig()
        {
            this.BufferSize = 10000;
            this.ReadTimeout = 100;
            this.Backlog = 10;
            this.PollResponseWait = 100;
        }

    }
}