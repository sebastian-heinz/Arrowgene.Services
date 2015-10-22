namespace Arrowgene.Services.Network.ManagedConnection.Packet
{
    using System;

    public class ManagedPacket
    {
        public const Int32 HEADER_SIZE = 16;
        public const Int32 HEADER_PACKET_SIZE = 4;
        public const Int32 HEADER_ID_SIZE = 4;

        public static ManagedPacket CreateInstance(byte[] headerBuffer)
        {
            Int32 packetSize = BitConverter.ToInt32(headerBuffer, 0);
            Int32 packetId = BitConverter.ToInt32(headerBuffer, HEADER_PACKET_SIZE);
            return new ManagedPacket(packetSize, packetId);
        }

        private int size;
        private int id;

        public ManagedPacket(int size, int id)
        {
            this.size = size;
            this.id = id;
        }


        public byte[] RawPacket { get; set; }
        public int Size { get { return this.size; } }
        public int Id { get { return this.id; } }


    }
}
