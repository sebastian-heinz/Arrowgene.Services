using Arrowgene.Services.Network;
using Arrowgene.Services.Network.Discovery;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace Arrowgene.Services.Playground
{
    public class Discover
    {

        public Discover()
        {

            Broadcast bc = new Broadcast(15000);
            bc.ReceivedBroadcast += Bc_ReceivedBroadcast;
            bc.Listen();


            Broadcast b1c = new Broadcast(15000);
            b1c.Send(new byte[10]);
        }

        private void Bc_ReceivedBroadcast(object sender, ReceivedBroadcastPacketEventArgs e)
        {
            
        }
    }
}
