namespace Arrowgene.Services.Playground.Demo
{
    using Arrowgene.Services.Network.Proxy;
    using Network.TCP.Client;
    using Network.TCP.Server;
    using System;
    using System.Net;
    using System.Threading;

    public class ProxyDemo
    {
        TCPServer server;
        TCPClient client;
        ProxyServer proxyServer;

        public ProxyDemo()
        {
            this.Run();
        }

        internal void close()
        {
            client.Disconnect();
            proxyServer.Stop();
            server.Stop();
        }

        internal void Run()
        {
            IPAddress serverListenIp = IPAddress.Parse("127.0.0.1");
            int serverListenPort = 2349;
            server = new TCPServer(serverListenIp, serverListenPort);
            server.ReceivedPacket += Server_ServerReceivedPacket;

            IPAddress proxyIp = IPAddress.Parse("192.168.178.20");
            int proxyPort = 2345;
            client = new TCPClient();
            client.ReceivedPacket += Client_ClientReceivedPacket;

            ProxyConfig proxyConfig = new ProxyConfig(proxyIp, proxyPort, serverListenIp, serverListenPort);
            proxyServer = new ProxyServer(proxyConfig);
            proxyServer.ReceivedPacket += ProxyServer_ReceivedPacket;


            server.Start();

            while (!server.IsListening)
                Thread.Sleep(100);

            proxyServer.Start();

            while (!proxyServer.IsListening)
                Thread.Sleep(100);

            client.Connect(proxyIp, proxyPort);

            if (client.IsConnected)
            {
                Console.WriteLine("ProxyDemo::Run: Client Sends Packet...");
                client.Send(new byte[9]);
            }
        }


        private void ProxyServer_ReceivedPacket(object sender, ReceivedProxyPacketEventArgs e)
        {
            Console.WriteLine("ProxyDemo::ProxyServer_ReceivedPacket: Proxy forwarded packet from: " + e.ProxyPacket.Traffic);
        }

        private void Client_ClientReceivedPacket(object sender, ClientReceivedPacketEventArgs e)
        {
            Console.WriteLine("ProxyDemo::Client_ReceivedPacket: " + e.Payload.Size.ToString());
        }

        private void Server_ServerReceivedPacket(object sender, ReceivedPacketEventArgs e)
        {
            Console.WriteLine("ProxyDemo::Server_ReceivedPacket: " + e.Payload.Size.ToString());
            e.ClientSocket.Send(new byte[10]);
        }
    }
}