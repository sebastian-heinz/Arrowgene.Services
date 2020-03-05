using System;
using System.Net;
using System.Threading;
using Arrowgene.Networking.Udp;

namespace Arrowgene.Services.Playground.Demo
{
    public class UdpClientDemo
    {
        private const int Port = 15000;
        private bool _received;

        public UdpClientDemo()
        {
            IPAddress serverIp = IPAddress.Parse("127.0.0.1");

            UdpSocket client = new UdpSocket();
            client.ReceivedPacket += Client_ReceivedPacket;

            Console.WriteLine("UdpDemoClient::ctor: Sending Message to " + serverIp);
            client.Send(new byte[10], new IPEndPoint(serverIp, Port));

            // wait for echo server response.
            while (!_received)
                Thread.Sleep(10);

            client.StopReceive();
        }

        private void Client_ReceivedPacket(object sender, ReceivedUdpPacketEventArgs e)
        {
            Console.WriteLine("UdpDemo::EchoServer_ReceivedPacket: received: " + e.Size + "bytes from " + e.RemoteIpEndPoint);
            _received = true;
        }
    }
}