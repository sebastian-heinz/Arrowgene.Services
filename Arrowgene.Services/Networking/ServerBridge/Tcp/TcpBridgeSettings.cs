using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Arrowgene.Services.Networking.Tcp.Server.AsyncEvent;

namespace Arrowgene.Services.Networking.ServerBridge.Tcp
{
    [DataContract]
    public class TcpBridgeSettings : ICloneable
    {
        [DataMember(Order = 0)]
        public NetworkPoint PublicEndPoint { get; set; }

        [DataMember(Order = 1)]
        public NetworkPoint ListenEndPoint { get; set; }

        [DataMember(Order = 2)]
        public List<NetworkPoint> ClientEndPoints { get; set; }

        [DataMember(Order = 3)]
        public AsyncEventSettings ServerSettings { get; set; }

        public TcpBridgeSettings(NetworkPoint listenEndPoint, NetworkPoint publicEndPoint, List<NetworkPoint> clientEndPoints)
        {
            PublicEndPoint = new NetworkPoint(publicEndPoint);
            ListenEndPoint = new NetworkPoint(listenEndPoint);
            ClientEndPoints = new List<NetworkPoint>();
            foreach (NetworkPoint tcpEndpoint in clientEndPoints)
            {
                ClientEndPoints.Add(new NetworkPoint(tcpEndpoint));
            }

            if (!ClientEndPoints.Contains(PublicEndPoint))
            {
                ClientEndPoints.Add(PublicEndPoint);
            }

            ServerSettings = new AsyncEventSettings();
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