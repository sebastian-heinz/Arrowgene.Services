namespace Arrowgene.Services.Playground.Demo
{
    using Network.UDP;
    using Network.UDP.Broadcast;
    using System.Diagnostics;

    public class BroadcastDemo
    {
        public BroadcastDemo()
        {
            //Listen for all UDP Packets
            UDPServer bc = new UDPServer(15000);
            bc.ReceivedPacket += Bc_ReceivedPacket;
            bc.Listen();
            bc.Stop();
            //Send a Broadcast
            BroadcastClient b1c = new BroadcastClient();
            b1c.Send(new byte[309], 15000);
        }

        private void Bc_ReceivedPacket(object sender, ReceivedUDPPacketEventArgs e)
        {
            //Receive UDP Packet

            //(optional) acces server object
            UDPServer server = sender as UDPServer;

            //e contains all packet informations
            Debug.WriteLine("Server: " + server.IPEndPoint.ToString() + " Received: " + e.Size + " bytes from " + e.RemoteIPEndPoint.ToString());
        }
    }
}
