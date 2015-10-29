namespace Arrowgene.Services.Network.ManagedConnection.Packet
{
    using Client;
    using Logging;
    using Serialization;

    internal class PacketManager
    {
        private ISerializer serializer;
        private Logger logger;
        internal PacketManager(ISerializer serializer, Logger logger)
        {
            this.serializer = serializer;
            this.logger = logger;
        }

        internal void Handle(ClientSocket clientSocket, ManagedPacket packet)
        {
            object myClass = this.serializer.Deserialize(packet.Payload, this.logger);
            if (myClass != null)
            {
                this.logger.Write("Handled Packet: {0}", packet.Id, LogType.PACKET);
            }
            else
            {
                this.logger.Write("Could not handled packet: {0}", packet.Id, LogType.PACKET);
            }
        }

        internal bool CheckPacketId(int packetId)
        {
            return true;
        }

    }
}
