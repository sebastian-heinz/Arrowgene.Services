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

        internal bool Handle(ClientSocket clientSocket, ManagedPacket packet)
        {
            bool success = false;

            object myClass = this.serializer.Deserialize(packet.Payload, this.logger);
            if (myClass != null)
            {
                packet.Object = myClass;
                this.logger.Write("Handled Packet: {0}", packet.Id, LogType.PACKET);
                success = true;
            }
            else
            {
                this.logger.Write("Could not handled packet: {0}", packet.Id, LogType.PACKET);
            }

            return success;
        }

        internal bool CheckPacketId(int packetId)
        {
            return true;
        }

    }
}
