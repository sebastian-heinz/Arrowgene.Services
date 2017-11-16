namespace Arrowgene.Services.Playground.Demo
{
    using Network.Udp;
    using System;
    using System.Net;
    using System.Threading;

    public class UdpClientDemo
    {
        private const int PORT = 15000;
        private bool received = false;

        public UdpClientDemo()
        {
            IPAddress serverIP = IPAddress.Parse("127.0.0.1");

            UdpSocket client = new UdpSocket();
            client.ReceivedPacket += Client_ReceivedPacket;


            Console.WriteLine("UdpDemoClient::ctor: Sending Message to " + serverIP.ToString());
            client.Send(new byte[10], new IPEndPoint(serverIP, PORT));

            // wait for echo server response.
            while (!this.received)
                Thread.Sleep(10);

            client.StopReceive();
        }

        private void Client_ReceivedPacket(object sender, ReceivedUdpPacketEventArgs e)
        {
            Console.WriteLine("UdpDemo::EchoServer_ReceivedPacket: received: " + e.Size + "bytes from " + e.RemoteIPEndPoint.ToString());
            this.received = true;
        }
    }
}