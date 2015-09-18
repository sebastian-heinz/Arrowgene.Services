namespace Arrowgene.Services.Playground.Demo
{
    using Network.UDP;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;

    public class UdpDemo
    {
        private const int PORT = 15000;
        private bool received = false;

        public UdpDemo()
        {
            IPAddress serverIP = IPAddress.Parse("127.0.0.1");

            UDPServer echoServer = new UDPServer(PORT);
            echoServer.ReceivedPacket += EchoServer_ReceivedPacket;
            echoServer.Listen();

            UDPClient client = new UDPClient(PORT);
            client.ReceivedPacket += Client_ReceivedPacket;
            client.Connect(serverIP, PORT);

            Debug.WriteLine("UdpDemo::ctor: Sending Message...");
            client.SendTo(new byte[10], new IPEndPoint(serverIP, PORT));

            // wait for echo server response.
            while (!this.received)
                Thread.Sleep(10);
        }

        private void Client_ReceivedPacket(object sender, ReceivedUDPPacketEventArgs e)
        {
            Debug.WriteLine("UdpDemo::EchoServer_ReceivedPacket: received: " + e.Size + "bytes");
            this.received = true;
        }

        private void EchoServer_ReceivedPacket(object sender, ReceivedUDPPacketEventArgs e)
        {
            Debug.WriteLine("UdpDemo::EchoServer_ReceivedPacket: received: " + e.Size + "bytes");
            UDPServer echoServer = sender as UDPServer;
            echoServer.SendTo(new byte[20], e.RemoteIPEndPoint);
        }

    }
}
