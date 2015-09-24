namespace Arrowgene.Services.Network.UDP
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;

    public class Broadcast : UDPSocket
    {

        public Broadcast(IPAddress ipAddress, int port) : base(ipAddress, port)
        {

        }

        public void SendBroadcast(byte[] data, int port)
        {
            IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
            this.socket.EnableBroadcast = true;
            this.SendTo(data, broadcastEndPoint);
        }

        protected override void OnReceivedUDPPacket(int receivedBytesCount, byte[] received, IPEndPoint remoteIPEndPoint)
        {
          


        }

    }
}
