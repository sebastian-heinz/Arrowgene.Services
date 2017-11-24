using System;
using System.Net;
using System.Threading;
using Arrowgene.Services.Network.Udp;

namespace Arrowgene.Services.Playground.Demo
{
    public class UdpServerDemo
    {
        private const int Port = 15000;
        private bool _received;

        public UdpServerDemo()
        {
            UdpSocket echoServer = new UdpSocket();
            echoServer.ReceivedPacket += EchoServer_ReceivedPacket;
            echoServer.StartListen(new IPEndPoint(IPAddress.Any, Port));

            Console.WriteLine("Waiting");

            // wait for response.
            while (!_received)
                Thread.Sleep(10);

            echoServer.StopReceive();
        }

        private void EchoServer_ReceivedPacket(object sender, ReceivedUdpPacketEventArgs e)
        {
            Console.WriteLine("UdpDemoServer::EchoServer_ReceivedPacket: received: " + e.Size + "bytes from " + e.RemoteIpEndPoint);
            UdpSocket echoServer = sender as UdpSocket;
            if (echoServer != null)
            {
                echoServer.Send(new byte[20], e.RemoteIpEndPoint);
            }
            _received = true;
        }
    }
}