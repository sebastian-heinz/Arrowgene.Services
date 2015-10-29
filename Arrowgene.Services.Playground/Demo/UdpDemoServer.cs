namespace Arrowgene.Services.Playground.Demo
{
    using Network.UDP;
    using System;
    using System.Net;
    using System.Threading;

    public class UdpDemoServer
    {
        private const int PORT = 15000;
        private bool received = false;

        public UdpDemoServer()
        {
            UDPSocket echoServer = new UDPSocket();
            echoServer.ReceivedPacket += EchoServer_ReceivedPacket;
            echoServer.StartListen(new IPEndPoint(IPAddress.Any, PORT));

            Console.WriteLine("Waiting");

            // wait for response.
            while (!this.received)
                Thread.Sleep(10);

            echoServer.StopReceive();
        }

        private void EchoServer_ReceivedPacket(object sender, ReceivedUDPPacketEventArgs e)
        {
            Console.WriteLine("UdpDemoServer::EchoServer_ReceivedPacket: received: " + e.Size + "bytes from " + e.RemoteIPEndPoint.ToString());
            UDPSocket echoServer = sender as UDPSocket;
            echoServer.Send(new byte[20], e.RemoteIPEndPoint);
            this.received = true;
        }
    }
}
