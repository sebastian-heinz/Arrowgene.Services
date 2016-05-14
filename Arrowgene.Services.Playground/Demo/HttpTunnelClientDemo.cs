namespace Arrowgene.Services.Playground.Demo
{
    using Network.Http.Tunnel;
    using System;
    using System.Net;

    public class HttpTunneClientDemo
    {
        public HttpTunneClientDemo()
        {
            string localIP = "127.0.0.1";
            int localPort = 2345;

            string tunnelIP = "127.0.0.1";
            int tunnelPort = 80;

            string destinationIP = "127.0.0.1";
            int destinationPort = 23;

            IPEndPoint localIPEndPoint = new IPEndPoint(IPAddress.Parse(localIP), localPort);
            IPEndPoint tunnelIPEndPoint = new IPEndPoint(IPAddress.Parse(tunnelIP), tunnelPort);
            IPEndPoint destinationIPEndPoint = new IPEndPoint(IPAddress.Parse(destinationIP), destinationPort);


            HttpTunnelClient client = new HttpTunnelClient();

            client.Start(localIPEndPoint, tunnelIPEndPoint, destinationIPEndPoint);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            client.Stop();
        }

    }
}
