namespace Arrowgene.Services.Network.ManagedConnection.Client
{
    using Packet;
    using Logging;
    using Serialization;
    using System;
    using System.Net.Sockets;

    public class ClientSocket
    {
        private ISerializer serializer;
        private Logger logger;

        internal Socket Socket { get; private set; }

        public int Id { get; internal set; }
        public bool IsBusy { get; internal set; }
        public bool IsAlive { get; internal set; }
        public int InTraffic { get; internal set; }
        public int OutTraffic { get; internal set; }

        public ClientSocket(Socket socket, ISerializer serializer, Logger logger)
        {
            this.Socket = socket;
            this.serializer = serializer;
            this.logger = logger;
        }

        public void SendObject(Int32 packetId, object myClass)
        {
            byte[] serialized = this.serializer.Serialize(myClass, this.logger);
            if (serialized != null)
            {
               byte[] data = ManagedPacket.CreatePacketbytes(packetId, serialized);
                this.Socket.Send(data);
            }
        }

        public void Close()
        {
            this.IsAlive = false;
        }
    }


}
