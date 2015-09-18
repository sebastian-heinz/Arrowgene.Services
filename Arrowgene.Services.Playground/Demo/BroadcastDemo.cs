namespace Arrowgene.Services.Playground.Demo
{
    using Network.UDP;
    using System.Diagnostics;
    using System.Threading;

    public class BroadcastDemo
    {
        private bool received = false;

        public BroadcastDemo()
        {
            UDPServer bc = new UDPServer(15000);
            bc.ReceivedPacket += Bc_ReceivedPacket;
            bc.Listen();

            UDPClient.SendBroadcast(new byte[309], 15000);

            //Wait to receive broadcast.
            while (!this.received)
                Thread.Sleep(10);
        }

        private void Bc_ReceivedPacket(object sender, ReceivedUDPPacketEventArgs e)
        {
            UDPServer server = sender as UDPServer;
            Debug.WriteLine("Server: " + server.IPEndPoint.ToString() + " Received: " + e.Size + " bytes from " + e.RemoteIPEndPoint.ToString());
            this.received = true;
        }

    }

}