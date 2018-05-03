using System.Collections.Generic;
using System.Net;

namespace Arrowgene.Services.Bridging.Tcp
{
    public class TcpBridgeConfiguration
    {
        public IPEndPoint PublicEndPoint { get; set; }

        public IPEndPoint ListenEndPoint { get; set; }

        public List<IPEndPoint> AllowedEndPoints { get; set; }

        public TcpBridgeConfiguration(IPEndPoint listenEndPoint, IPEndPoint publicEndPoint, List<IPEndPoint> allowedEndPoints)
        {
            ListenEndPoint = listenEndPoint;
            PublicEndPoint = publicEndPoint;
            AllowedEndPoints = allowedEndPoints;
            if (!AllowedEndPoints.Contains(PublicEndPoint))
            {
                AllowedEndPoints.Add(PublicEndPoint);
            }
        }
    }
}