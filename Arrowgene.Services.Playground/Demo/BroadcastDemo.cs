namespace Arrowgene.Services.Playground.Demo
{
    using Arrowgene.Services.Network;
    using Network.Broadcast;
    using Arrowgene.Services.Network.Discovery;

    public class BroadcastDemo
    {
        public BroadcastDemo()
        {
            UDPBroadcast bc = new UDPBroadcast(15000);
            bc.ReceivedBroadcast += Bc_ReceivedBroadcast;
            bc.Listen();

            UDPBroadcast b1c = new UDPBroadcast(15000);
            b1c.Send(new byte[309]);
        }

        private void Bc_ReceivedBroadcast(object sender, ReceivedUDPBroadcastPacketEventArgs e)
        {
            
        }
    }
}
