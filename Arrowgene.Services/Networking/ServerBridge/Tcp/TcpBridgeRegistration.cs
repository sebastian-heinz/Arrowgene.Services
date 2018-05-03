using System;
using Arrowgene.Services.Networking.ServerBridge.Messages;

namespace Arrowgene.Services.Networking.ServerBridge.Tcp
{
    [Serializable]
    public class TcpBridgeRegistration : Message
    {
        public string SourceIp { get; }
        public int SourcePort { get; }

        public TcpBridgeRegistration(NetworkPoint networkPoint)
        {
            SourceIp = networkPoint.Address.ToString();
            SourcePort = networkPoint.Port;
        }
    }
}