namespace SvrKitConsolePlayground
{
    using SvrKit.Networking;
    using SvrKit.Networking.MarrySocket.MClient;
    using SvrKit.Networking.MarrySocket.MServer;
    using SvrKit.Networking.Proxy;
    using System.Net;
    using System.Threading;
    public class Proxy
    {
        public Proxy()
        {
            ServerConfig serverConfig = new ServerConfig();
            serverConfig.ServerPort = 2345;
            MarryServer server = new MarryServer(serverConfig);

            ClientConfig clientConfig = new ClientConfig();
            clientConfig.ServerPort = 2349;
            MarryClient client = new MarryClient(clientConfig);

            ProxyConfig proxyConfig = new ProxyConfig(IPAddress.IPv6Any, 2349, IPTools.IPAddressLookup("127.0.0.1", System.Net.Sockets.AddressFamily.InterNetworkV6), 2345);
            ProxyServer proxyServer = new ProxyServer(proxyConfig);


        //   server.Start();

        //    while (!server.IsListening)
        //       Thread.Sleep(100);

            proxyServer.Connect();

            while (!proxyServer.IsListening)
                Thread.Sleep(100);

            client.Connect();

            if (client.IsConnected)
            {
                client.ServerSocket.SendObject(1000, "hello");
            }


        }


    }
}