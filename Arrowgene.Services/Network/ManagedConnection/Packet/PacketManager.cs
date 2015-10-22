namespace Arrowgene.Services.Network.ManagedConnection.Packet
{
    using Logging;
    using Arrowgene.Services.Network.ManagedConnection.Server;

    internal class PacketManager
    {
        private ManagedServer server;
        internal PacketManager(ManagedServer server)
        {
            this.server = server;
        }

        internal void Handle(ClientSocket clientSocket, ManagedPacket packet)
        {

            object myClass = this.server.Serializer.Deserialize(packet.RawPacket, this.server.Logger);
            if (myClass != null)
            {
                this.server.InTraffic += packet.Size;
                this.server.OnReceivedPacket(packet.Id, clientSocket , packet);
                this.server.Logger.Write("Handled Packet: {0}", packet.Id, LogType.PACKET);
            }
            else
            {
                this.server.Logger.Write("Could not handled packet: {0}", packet.Id, LogType.PACKET);
            }



        }
    }
}
