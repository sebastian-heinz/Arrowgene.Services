using System;
using System.Net;
using System.Runtime.Serialization;

namespace Arrowgene.Services.Networking
{
    [DataContract]
    public class NetworkPoint : ICloneable
    {
        [DataMember]
        public ushort Port { get; set; }

        [IgnoreDataMember]
        public IPAddress Address { get; set; }

        [DataMember(Name = "PublicIpAddress")]
        public string DataPublicIpAddress
        {
            get => Port.ToString();
            set => Address = string.IsNullOrEmpty(value) ? null : IPAddress.Parse(value);
        }

        public NetworkPoint(IPAddress address, ushort port)
        {
            Address = address;
            Port = port;
        }

        public NetworkPoint(NetworkPoint networkPoint)
        {
            string ip = networkPoint.Address.ToString();
            Address = string.IsNullOrEmpty(ip) ? null : IPAddress.Parse(ip);
            Port = networkPoint.Port;
        }

        public NetworkPoint(IPEndPoint ipEndPoint)
        {
            Address = ipEndPoint.Address;
            Port = (ushort) ipEndPoint.Port;
        }

        public IPEndPoint ToIpEndPoint()
        {
            return new IPEndPoint(Address, Port);
        }

        public object Clone()
        {
            return new NetworkPoint(this);
        }
    }
}