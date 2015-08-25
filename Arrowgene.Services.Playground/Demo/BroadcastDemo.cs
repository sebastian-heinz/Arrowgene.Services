namespace Arrowgene.Services.Playground.Demo
{
    using Arrowgene.Services.Network;
    using Arrowgene.Services.Network.Discovery;

    public class BroadcastDemo
    {
        public BroadcastDemo()
        {
            Broadcast bc = new Broadcast(15000);
            bc.ReceivedBroadcast += Bc_ReceivedBroadcast;
            bc.Listen();

            Broadcast b1c = new Broadcast(15000);
            b1c.Send(new byte[309]);
        }

        private void Bc_ReceivedBroadcast(object sender, ReceivedBroadcastPacketEventArgs e)
        {
            
        }
    }
}
