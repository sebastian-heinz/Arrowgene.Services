namespace Arrowgene.Services.Playground.Demo
{
    using Network;
    using Arrowgene.Services.Network.MarrySocket.MClient;
    using Arrowgene.Services.Network.MarrySocket.MServer;
    using Arrowgene.Services.Network.Proxy;
    using Network.Discovery;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class ProxyDemo
    {
        MarryServer server;
        MarryClient client;
        ProxyServer proxyServer;

        public ProxyDemo()
        {
            this.Run();
        }

        private void ProxyServer_ReceivedPacket(object sender, ReceivedProxyPacketEventArgs e)
        {

        }

        private void Client_ReceivedPacket(object sender, Arrowgene.Services.Network.MarrySocket.MClient.ReceivedPacketEventArgs e)
        {

        }

        private void Server_ReceivedPacket(object sender, Arrowgene.Services.Network.MarrySocket.MServer.ReceivedPacketEventArgs e)
        {
            e.ClientSocket.SendObject(1000, "world!");
        }

        internal void close()
        {
            client.Disconnect();
            proxyServer.Stop();
            server.Stop();
        }

        internal void Run()
        {
            ServerConfig serverConfig = new ServerConfig(IP.AddressLocalhost(AddressFamily.InterNetworkV6), 2345);
            server = new MarryServer(serverConfig);
            server.ReceivedPacket += Server_ReceivedPacket;

            ClientConfig clientConfig = new ClientConfig(IP.AddressLocalhost(AddressFamily.InterNetworkV6), 2349);
            client = new MarryClient(clientConfig);
            client.ReceivedPacket += Client_ReceivedPacket;

            ProxyConfig proxyConfig = new ProxyConfig(IPAddress.IPv6Any, 2349, IP.AddressLocalhost(AddressFamily.InterNetworkV6), 2345);
            proxyServer = new ProxyServer(proxyConfig);
            proxyServer.ReceivedPacket += ProxyServer_ReceivedPacket;


            server.Start();

            while (!server.IsListening)
                Thread.Sleep(100);

            proxyServer.Start();

            while (!proxyServer.IsListening)
                Thread.Sleep(100);

            client.Connect();

            if (client.IsConnected)
            {
                client.ServerSocket.SendObject(1000, "hello?");
            }
        }
    }
}