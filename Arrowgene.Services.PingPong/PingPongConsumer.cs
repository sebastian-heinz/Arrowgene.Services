using System;
using System.Collections.Generic;
using Arrowgene.Services.Logging;
using Arrowgene.Services.Networking.Tcp;
using Arrowgene.Services.Networking.Tcp.Consumer.BlockingQueueConsumption;
using Arrowgene.Services.Networking.Tcp.Server.AsyncEvent;

namespace Arrowgene.Services.PingPong
{
    public class PingPongConsumer : ThreadedBlockingQueueConsumer
    {
        private readonly Dictionary<int, IHandler> _handlers;
        private readonly Dictionary<ITcpSocket, Connection> _connections;
        private readonly object _lock;
        private readonly ILogger _logger;

        public PingPongConsumer(AsyncEventSettings socketSetting, string identity = "PingPongConsumer") : base(
            socketSetting, identity)
        {
            _logger = LogProvider.Logger(this);
            _connections = new Dictionary<ITcpSocket, Connection>();
            _handlers = new Dictionary<int, IHandler>();
            _lock = new object();
        }
        
        public void AddHandler(IHandler handler)
        {
            if (_handlers.ContainsKey(handler.Id))
            {
                _logger.Error($"ClientHandlerId: {handler.Id} already exists");
            }
            else
            {
                _handlers.Add(handler.Id, handler);
            }
        }

        protected override void HandleReceived(ITcpSocket socket, byte[] data)
        {
            if (!socket.IsAlive)
            {
                return;
            }

            Connection connection;
            lock (_lock)
            {
                if (!_connections.ContainsKey(socket))
                {
                    _logger.Error($"[{socket.Identity}] Client does not exist in lookup");
                    return;
                }

                connection = _connections[socket];
            }

            List<Packet> packets = connection.Receive(data);
            foreach (Packet packet in packets)
            {
                if (!_handlers.ContainsKey(packet.Id))
                {
                    _logger.Error($"[{socket.Identity}] Unknown Packet");
                    return;
                }

                IHandler handler = _handlers[packet.Id];
                try
                {
                    handler.Handle(connection, packet);
                }
                catch (Exception ex)
                {
                    _logger.Error($"[{connection.Identity}] Exception");
                    _logger.Exception(ex);
                }
            }
        }

        protected override void HandleDisconnected(ITcpSocket socket)
        {
            Connection connection;
            lock (_lock)
            {
                if (!_connections.ContainsKey(socket))
                {
                    _logger.Error($"[{socket.Identity}] Disconnected client does not exist in lookup");
                    return;
                }

                connection = _connections[socket];
                _connections.Remove(socket);
                _logger.Debug($"Clients Count: {_connections.Count}");
            }

            _logger.Info($"[{connection.Identity}] Client disconnected");
        }

        protected override void HandleConnected(ITcpSocket socket)
        {
            Connection connection = new Connection(socket);
            lock (_lock)
            {
                _connections.Add(socket, connection);
                _logger.Debug($"Clients Count: {_connections.Count}");
            }

            _logger.Info($"[{connection.Identity}] Client connected");
        }
    }
}