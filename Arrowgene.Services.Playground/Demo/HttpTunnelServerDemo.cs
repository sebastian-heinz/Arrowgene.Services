namespace Arrowgene.Services.Playground.Demo
{
    using Network.Http.Tunnel;
    using System;
    using System.Net;

    public class HttpTunnelServerDemo
    {
        public HttpTunnelServerDemo()
        {
            string localIP = "127.0.0.1";
            int localPort = 80;

            IPEndPoint localIPEndPoint = new IPEndPoint(IPAddress.Parse(localIP), localPort);

            HttpTunnelServer server = new HttpTunnelServer();

            server.Start(localIPEndPoint);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            server.Stop();
        }

    }
}
