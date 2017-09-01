namespace Arrowgene.Services.Network.TCP.Managed
{
    using Arrowgene.Services.Network.TCP.Client;
    using Common;
    using Logging;
    using Serialization;

    public class ManagedClientSocket
    {
        private ISerializer serializer;
        private ClientSocket clientSocket;
        private Logger logger;

        public ManagedClientSocket(ClientSocket clientSocket, Logger logger, ISerializer serializer)
        {
            this.clientSocket = clientSocket;
            this.serializer = serializer;
            this.logger = logger;
            this.Buffer = new ByteBuffer();
        }

        public int Id
        {
            get
            {
                return this.clientSocket.Id;
            }
        }

        internal ByteBuffer Buffer { get; private set; }

        public void Send(int packetId, object myClass)
        {
            byte[] serialized = this.serializer.Serialize(myClass, this.logger);
            if (serialized != null)
            {
                ByteBuffer packet = new ByteBuffer();
                packet.WriteInt32(packetId);
                packet.WriteInt32(serialized.Length + ManagedPacket.HeaderSize);
                packet.WriteBytes(serialized);
                this.clientSocket.Send(packet.ReadBytes());
            }
        }

    }
}