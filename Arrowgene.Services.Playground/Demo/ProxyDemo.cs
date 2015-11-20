namespace Arrowgene.Services.Playground.Demo
{
    using Arrowgene.Services.Network.Proxy;
    using Common;
    using Network.ManagedConnection.Client;
    using Network.ManagedConnection.Server;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class ProxyDemo
    {
        ManagedServer server;
        ManagedClient client;
        ProxyServer proxyServer;

        public ProxyDemo()
        {
            this.Run();
        }

        private void ProxyServer_ReceivedPacket(object sender, ReceivedProxyPacketEventArgs e)
        {
            Console.WriteLine("ProxyDemo::ProxyServer_ReceivedPacket: Proxy forwarded packet from: " + e.ProxyPacket.Traffic);
        }

        private void Client_ReceivedPacket(object sender, Network.ManagedConnection.Event.ReceivedPacketEventArgs e)
        {
            Console.WriteLine("ProxyDemo::Client_ReceivedPacket: " + (string)e.Packet.Object);
        }

        private void Server_ReceivedPacket(object sender, Network.ManagedConnection.Event.ReceivedPacketEventArgs e)
        {
            Console.WriteLine("ProxyDemo::Server_ReceivedPacket: " + (string)e.Packet.Object);
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
            server = new ManagedServer(IPAddress.Any, 2345);
            server.ReceivedPacket += Server_ReceivedPacket;


            client = new ManagedClient();
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

            client.Connect(IPAddress.Parse("192.168.178.20"), 2345);

            if (client.IsConnected)
            {
                Debug.WriteLine("ProxyDemo::Run: Client Sends Packet...");
                client.SendObject(1000, "hello?");
            }
        }


    }
}