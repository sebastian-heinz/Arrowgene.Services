namespace Arrowgene.Services.Playground.Demo
{
    using Network.UDP;
    using System;
    using System.Net;
    using System.Threading;

    public class UdpDemoClient
    {
        private const int PORT = 15000;
        private bool received = false;

        public UdpDemoClient()
        {
            IPAddress serverIP = IPAddress.Parse("127.0.0.1");

            UDPSocket client = new UDPSocket();
            client.ReceivedPacket += Client_ReceivedPacket;


            Console.WriteLine("UdpDemoClient::ctor: Sending Message to " + serverIP.ToString());
            client.Send(new byte[10], new IPEndPoint(serverIP, PORT));

            // wait for echo server response.
            while (!this.received)
                Thread.Sleep(10);

            client.Close();
        }

        private void Client_ReceivedPacket(object sender, ReceivedUDPPacketEventArgs e)
        {
            Console.WriteLine("UdpDemo::EchoServer_ReceivedPacket: received: " + e.Size + "bytes from " + e.RemoteIPEndPoint.ToString());
            this.received = true;
        }
    }
}