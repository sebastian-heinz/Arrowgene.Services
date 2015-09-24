namespace Arrowgene.Services.Playground.Demo
{
    using Network.UDP;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;

    public class BroadcastDemo
    {
        private bool received = false;

        public BroadcastDemo()
        {
            IPAddress serverIP = IPAddress.Parse("127.0.0.1");
            Broadcast bc = new Broadcast(serverIP, 15000);
            bc.ReceivedPacket += Bc_ReceivedPacket;
     
     

            //Wait to receive broadcast.
            while (!this.received)
                Thread.Sleep(10);
        }

        private void Bc_ReceivedPacket(object sender, ReceivedUDPPacketEventArgs e)
        {
            UDPSocket server = sender as UDPSocket;
            Debug.WriteLine("Server: " + server.LocalIPEndPoint.ToString() + " Received: " + e.Size + " bytes from " + e.RemoteIPEndPoint.ToString());
            //this.received = true;
        }

    }

}