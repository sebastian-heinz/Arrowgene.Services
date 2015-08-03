namespace SvrKitConsolePlayground
{
    using ArrowgeneServices.Networking;
    using ArrowgeneServices.Networking.MarrySocket.MClient;
    using ArrowgeneServices.Networking.MarrySocket.MServer;
    using ArrowgeneServices.Networking.Proxy;
    using System;
    using System.Net;
    using System.Threading;

    public class Proxy
    {
        MarryServer server;
        MarryClient client;
        ProxyServer proxyServer;

        public Proxy()
        {
            ServerConfig serverConfig = new ServerConfig();
            serverConfig.ServerPort = 2345;
            server = new MarryServer(serverConfig);
            server.ReceivedPacket += Server_ReceivedPacket;

            ClientConfig clientConfig = new ClientConfig();
            clientConfig.ServerPort = 2349;
            client = new MarryClient(clientConfig);
            client.ReceivedPacket += Client_ReceivedPacket;

            ProxyConfig proxyConfig = new ProxyConfig(IPAddress.IPv6Any, 2349, AGSocket.IPAddressLookup("127.0.0.1", System.Net.Sockets.AddressFamily.InterNetworkV6), 2345);
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
                client.ServerSocket.SendObject(1000, "hello");
            }


        }

        private void ProxyServer_ReceivedPacket(object sender, ReceivedProxyPacketEventArgs e)
        {
            Console.WriteLine("a");
        }

        private void Client_ReceivedPacket(object sender, ArrowgeneServices.Networking.MarrySocket.MClient.ReceivedPacketEventArgs e)
        {
            Console.WriteLine("b");
        }

        private void Server_ReceivedPacket(object sender, ArrowgeneServices.Networking.MarrySocket.MServer.ReceivedPacketEventArgs e)
        {
            Console.WriteLine("c");
        }

        internal void close()
        {
            proxyServer.Stop();
            client.Disconnect();
            server.Stop();
        }
    }
}