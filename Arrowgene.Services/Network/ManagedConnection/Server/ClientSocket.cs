namespace Arrowgene.Services.Network.ManagedConnection.Server
{
    using Serialization;
    using System.Net.Sockets;
    using System;

    public class ClientSocket
    {

        private ISerializer serializer;

        public int Id { get; internal set; }
        public bool IsBusy { get; internal set; }
        public bool IsAlive { get; internal set; }

        internal Socket Socket { get; private set; }
        public int InTraffic { get; internal set; }

        public ClientSocket(Socket socket, ISerializer serializer)
        {
            this.Socket = socket;
            this.serializer = serializer;
        }

        internal void Close()
        {
            throw new NotImplementedException();
        }
    }


}
