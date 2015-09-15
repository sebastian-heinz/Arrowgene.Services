namespace Arrowgene.Services.Playground.Demo
{
    using Arrowgene.Services.Network;
    using Network.Broadcast;
    using Arrowgene.Services.Network.Discovery;
    using System.Threading;

    public class BroadcastDemo
    {
        public BroadcastDemo()
        {
            BroadcastServer bc = new BroadcastServer(15000);
            bc.ReceivedBroadcast += Bc_ReceivedBroadcast;
            bc.Listen();

              BroadcastClient b1c = new BroadcastClient();
               b1c.Send(new byte[309], 15000);

            while (true)
                Thread.Sleep(10);
        }

        private void Bc_ReceivedBroadcast(object sender, ReceivedBroadcastEventArgs e)
        {
            
        }
    }
}
