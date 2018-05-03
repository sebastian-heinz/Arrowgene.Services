using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Arrowgene.Services.Networking.Tcp.Server.AsyncEvent;

namespace Arrowgene.Services.Networking.ServerBridge.Tcp
{
    [DataContract]
    public class TcpBridgeSettings : ICloneable
    {
        [DataMember]
        public NetworkPoint PublicEndPoint { get; set; }

        [DataMember]
        public NetworkPoint ListenEndPoint { get; set; }

        [DataMember]
        public List<NetworkPoint> ClientEndPoints { get; set; }

        [DataMember]
        public AsyncEventSettings ServerSettings { get; set; }

        public TcpBridgeSettings(NetworkPoint listenEndPoint, NetworkPoint publicEndPoint, List<NetworkPoint> clientEndPoints)
        {
            PublicEndPoint = publicEndPoint;
            ListenEndPoint = listenEndPoint;
            ClientEndPoints = clientEndPoints;
            if (!ClientEndPoints.Contains(PublicEndPoint))
            {
                ClientEndPoints.Add(PublicEndPoint);
            }
        }

        public TcpBridgeSettings(TcpBridgeSettings settings)
        {
            PublicEndPoint = new NetworkPoint(settings.PublicEndPoint);
            ListenEndPoint = new NetworkPoint(settings.ListenEndPoint);
            ClientEndPoints = new List<NetworkPoint>();
            foreach (NetworkPoint tcpEndpoint in settings.ClientEndPoints)
            {
                ClientEndPoints.Add(new NetworkPoint(tcpEndpoint));
            }

            ServerSettings = new AsyncEventSettings(settings.ServerSettings);
        }

        public object Clone()
        {
            return new TcpBridgeSettings(this);
        }
    }
}