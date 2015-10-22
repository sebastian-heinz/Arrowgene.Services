namespace Arrowgene.Services.Network.ManagedConnection.Client
{
    using Arrowgene.Services.Network.ManagedConnection.Serialization;
    using System;
    using System.Net;
    using Exceptions;

    public class ManagedClient
    {

        private IPAddress serverIPAddress;
        private int serverPort;

        public ManagedClient(IPAddress serverIPAddress, int serverPort, ISerializer serializer)
        {
            if (serverIPAddress == null || serverPort <= 0)
                throw new InvalidParameterException(string.Format("IPAddress({0}) or Port({1}) invalid", serverIPAddress, serverPort));

            this.serverIPAddress = serverIPAddress;
            this.serverPort = serverPort;
        }

        /// <summary>
        /// Server <see cref="System.Net.IPAddress"/>.
        /// </summary>
        public IPAddress ServerIPAddress { get { return this.serverIPAddress; } }

        /// <summary>
        /// Server port.
        /// </summary>
        public int ServerPort { get { return this.serverPort; } }


        



    }
}
