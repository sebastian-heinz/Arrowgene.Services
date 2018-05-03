using System;
using Arrowgene.Services.Bridging.Messages;

namespace Arrowgene.Services.Bridging.Tcp
{
    [Serializable]
    public class TcpBridgeRegistration : Message
    {
        public string SourceIp { get; }
        public int SourcePort { get; }

        public TcpBridgeRegistration(string sourceIp, int sourcePort)
        {
            SourceIp = sourceIp;
            SourcePort = sourcePort;
        }
    }
}