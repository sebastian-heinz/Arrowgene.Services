namespace Arrowgene.Services.Playground.Demo
{
    using Network.Udp;
    using System;
    using System.Net;
    using System.Threading;

    public class UdpServerDemo
    {
        private const int PORT = 15000;
        private bool received = false;

        public UdpServerDemo()
        {
            UdpSocket echoServer = new UdpSocket();
            echoServer.ReceivedPacket += EchoServer_ReceivedPacket;
            echoServer.StartListen(new IPEndPoint(IPAddress.Any, PORT));

            Console.WriteLine("Waiting");

            // wait for response.
            while (!this.received)
                Thread.Sleep(10);

            echoServer.StopReceive();
        }

        private void EchoServer_ReceivedPacket(object sender, ReceivedUdpPacketEventArgs e)
        {
            Console.WriteLine("UdpDemoServer::EchoServer_ReceivedPacket: received: " + e.Size + "bytes from " + e.RemoteIpEndPoint.ToString());
            UdpSocket echoServer = sender as UdpSocket;
            echoServer.Send(new byte[20], e.RemoteIpEndPoint);
            this.received = true;
        }
    }
}
