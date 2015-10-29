namespace Arrowgene.Services.Network.ManagedConnection.Packet
{
    using System;

    public class ManagedPacket
    {
        public const Int32 HEADER_SIZE = 16;
        public const Int32 HEADER_PACKET_SIZE = 4;
        public const Int32 HEADER_ID_SIZE = 4;

        public static byte[] CreatePacketbytes(int packetId, byte[] payload)
        {
            int packetSize = HEADER_SIZE + payload.Length;

            byte[] packetSizeBytes = BitConverter.GetBytes(packetSize);
            byte[] packetIdBytes = BitConverter.GetBytes(packetId);
            byte[] header = new byte[HEADER_SIZE];

            Buffer.BlockCopy(packetSizeBytes, 0, header, 0, packetSizeBytes.Length);
            Buffer.BlockCopy(packetIdBytes, 0, header, packetSizeBytes.Length, packetIdBytes.Length);

            byte[] data = new byte[packetSize];

            Buffer.BlockCopy(header, 0, data, 0, header.Length);
            Buffer.BlockCopy(payload, 0, data, header.Length, payload.Length);

            return data;
        }

        private int size;
        private int id;

        public ManagedPacket(int size, int id, byte[] headerBuffer, byte[] payload)
        {
            this.size = size;
            this.id = id;
            this.Header = headerBuffer;
            this.Payload = payload;
        }

        public byte[] Header { get; set; }
        public byte[] Payload { get; set; }
        public int Size { get { return this.size; } }
        public int Id { get { return this.id; } }


    }
}