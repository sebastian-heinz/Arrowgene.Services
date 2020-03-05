using Arrowgene.Buffers;

namespace Arrowgene.Services.Networking.Tcp.Consumer.GenericConsumption
{
    public class GenericState
    {
        public GenericState(byte[] data)
        {
            ReadLength = false;
            Length = 0;
            Position = 0;
            Buffer = new StreamBuffer(data);
        }

        public int Length { get; set; }
        public int Position { get; set; }
        public IBuffer Buffer { get; }
        public bool ReadLength { get; set; }
    }
}