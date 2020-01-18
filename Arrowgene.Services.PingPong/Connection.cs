using System;
using System.Collections.Generic;
using Arrowgene.Services.Logging;
using Arrowgene.Services.Networking.Tcp;

namespace Arrowgene.Services.PingPong
{
    public class Connection
    {
        private readonly ILogger _logger;

        public Connection(ITcpSocket clientSocket)
        {
            _logger = LogProvider.Logger(this);
            Socket = clientSocket;
        }

        public string Identity => Socket.Identity;
        public ITcpSocket Socket { get; }

        public List<Packet> Receive(byte[] data)
        {
            List<Packet> packets;
            try
            {
                packets = new List<Packet>();
                packets.Add(new Packet(data));
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                packets = new List<Packet>();
            }

            return packets;
        }

        public void Send(byte[] data)
        {
            Socket.Send(data);
        }

        public void Send(Packet packet)
        {
            Socket.Send(packet.Data);
        }
    }
}