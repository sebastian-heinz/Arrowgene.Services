namespace Arrowgene.Services.PingPong
{
    public class Packet
    {
        public byte[] Data { get; set; }
        public int Id { get; set; }

        public Packet(byte[] data)
        {
            Id = 0;
            Data = data;
        }
    }
}