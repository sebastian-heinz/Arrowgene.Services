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

             IPAddress ad =  IPAddress.Parse("127.0.0.1");
            IPAddress b  =IP.QueryRoutingInterface(ad).Address;
            NetworkInterface a = IP.GetNetworkInterface(b);
            int i;
            if (a != null)
            {
                i = 1;
            }
            else
            {
                i = 2;
            }

            i = 0;
        }

       

    }
}
