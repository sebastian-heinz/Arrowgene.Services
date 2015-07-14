namespace SvrKit.Networking.Proxy
{
    public class ProxyPacket : PacketReader
    {
        public enum TrafficType { CLIENT, SERVER };
        public TrafficType Traffic { get; set; }
        public byte[] RawPacket { get; set; }

        public ProxyPacket(int bufferSize)
            : base(bufferSize)
        {

        }
    }
}
