using System;
using System.Collections.Generic;
using System.Net;
using Arrowgene.Services.Networking.ServerBridge.Messages;
using Arrowgene.Services.Networking.Tcp;
using Arrowgene.Services.Networking.Tcp.Client;
using Arrowgene.Services.Networking.Tcp.Client.SyncReceive;
using Arrowgene.Services.Networking.Tcp.Consumer;
using Arrowgene.Services.Networking.Tcp.Consumer.GenericConsumption;
using Arrowgene.Services.Networking.Tcp.Server.AsyncEvent;


namespace Arrowgene.Services.Networking.ServerBridge.Tcp
{
    /// <summary>
    /// Bridge utilizing TCP to exchange messages.
    /// </summary>
    public class TcpBridge : Bridge, IConsumer
    {
        private readonly AsyncEventServer _tcpServer;
        private readonly Dictionary<ITcpSocket, IPEndPoint> _endPointLookup;
        private readonly Dictionary<IPEndPoint, ITcpSocket> _socketLookup;
        private readonly Dictionary<ITcpSocket, Queue<Message>> _queued;
        private readonly GenericConsumer<Message> _consumer;
        private readonly TcpBridgeSettings _settings;
        private readonly List<IPEndPoint> _clients;

        /// <summary>
        /// Creates a new instance of <see cref="TcpBridge"/>
        /// </summary>
        /// <param name="settings">Configuration</param>
        public TcpBridge(TcpBridgeSettings settings)
        {
            _settings = new TcpBridgeSettings(settings);
            _clients = new List<IPEndPoint>();
            _consumer = new GenericConsumer<Message>();
            _consumer.ReceivedGeneric += ConsumerOnReceivedGeneric;
            _endPointLookup = new Dictionary<ITcpSocket, IPEndPoint>();
            _socketLookup = new Dictionary<IPEndPoint, ITcpSocket>();
            _queued = new Dictionary<ITcpSocket, Queue<Message>>();
            _tcpServer = new AsyncEventServer(_settings.ListenEndPoint.Address, _settings.ListenEndPoint.Port, this);
            foreach (NetworkPoint networkPoint in _settings.ClientEndPoints)
            {
                _clients.Add(networkPoint.ToIpEndPoint());
            }
        }

        public override void Start()
        {
            _tcpServer.Start();
        }

        public override void Stop()
        {
            _tcpServer.Stop();
        }

        public override void Send(IPEndPoint receiver, Message message)
        {
            IPEndPoint instance = FindEndpoint(receiver.Address.ToString(), receiver.Port);
            if (instance == null)
            {
                Logger.Error("Destination is not allowed: {0}:{1}", receiver.Address, receiver.Port);
            }
            else if (_socketLookup.ContainsKey(instance) && _socketLookup[instance] != null)
            {
                Send(_socketLookup[instance], message);
            }
            else
            {
                SyncReceiveTcpClient client = new SyncReceiveTcpClient(this);
                client.ConnectError += ClientOnConnectError;
                Register(client, instance);
                TcpBridgeRegistration registration = new TcpBridgeRegistration(_settings.PublicEndPoint);
                Queue(client, registration);
                Queue(client, message);
                client.Connect(receiver.Address, (ushort) receiver.Port, TimeSpan.FromSeconds(30));
            }
        }

        private void ClientOnConnectError(object sender, ConnectErrorEventArgs eventArgs)
        {
            UnRegister(eventArgs.Client);
        }

        private void Send(ITcpSocket client, Message message)
        {
            byte[] data = _consumer.Serialize(message);
            client.Send(data);
        }

        private void ConsumerOnReceivedGeneric(object sender, ReceivedGenericEventArgs<Message> eventArgs)
        {
            if (eventArgs.Generic is TcpBridgeRegistration)
            {
                TcpBridgeRegistration registration = (TcpBridgeRegistration) eventArgs.Generic;
                IPEndPoint endPoint = FindEndpoint(registration.SourceIp, registration.SourcePort);
                if (endPoint == null)
                {
                    Logger.Error("Source is not allowed: {0}:{1}", registration.SourceIp, registration.SourcePort);
                }
                else
                {
                    Register(eventArgs.Socket, endPoint);
                    Logger.Info("Registered new conenction: {0}", eventArgs.Socket.RemoteIpAddress);
                }
            }
            else if (_endPointLookup.ContainsKey(eventArgs.Socket))
            {
                IPEndPoint endPoint = _endPointLookup[eventArgs.Socket];
                HandleMessage(endPoint, eventArgs.Generic);
            }
            else
            {
                Logger.Error("Received data from unknown source: {0}", eventArgs.Socket.RemoteIpAddress);
            }
        }

        private void Queue(ITcpSocket client, Message message)
        {
            if (_queued.ContainsKey(client))
            {
                _queued[client].Enqueue(message);
            }
            else
            {
                _queued.Add(client, new Queue<Message>(new[] {message}));
            }
        }

        private void DeQueue(ITcpSocket client)
        {
            if (_queued.ContainsKey(client))
            {
                Queue<Message> messages = _queued[client];
                while (messages.Count > 0)
                {
                    Message msg = messages.Dequeue();
                    Send(client, msg);
                }
            }
        }

        private void Register(ITcpSocket socket, IPEndPoint endPoint)
        {
            _endPointLookup.Add(socket, endPoint);
            _socketLookup.Add(endPoint, socket);
        }

        private void UnRegister(ITcpSocket socket)
        {
            if (_endPointLookup.ContainsKey(socket))
            {
                IPEndPoint endPoint = _endPointLookup[socket];
                _endPointLookup.Remove(socket);
                _socketLookup.Remove(endPoint);
            }
        }

        private IPEndPoint FindEndpoint(string ip, int port)
        {
            foreach (IPEndPoint endPoint in _clients)
            {
                if (endPoint.Address.ToString() == ip && endPoint.Port == port)
                {
                    return endPoint;
                }
            }

            return null;
        }

        void IConsumer.OnStart()
        {
            _consumer.OnStart();
        }

        void IConsumer.OnReceivedData(ITcpSocket socket, byte[] data)
        {
            _consumer.OnReceivedData(socket, data);
        }

        void IConsumer.OnClientDisconnected(ITcpSocket socket)
        {
            UnRegister(socket);
            _consumer.OnClientDisconnected(socket);
        }

        void IConsumer.OnClientConnected(ITcpSocket socket)
        {
            if (socket is SyncReceiveTcpClient)
            {
                DeQueue(socket);
            }

            _consumer.OnClientConnected(socket);
        }

        void IConsumer.OnStop()
        {
            _consumer.OnStart();
        }
    }
}