using System.Net;
using Arrowgene.Services.Networking.ServerBridge.Messages;
using Arrowgene.Services.Networking.Udp;
using Arrowgene.Services.Serialization;

namespace Arrowgene.Services.Networking.ServerBridge.Udp
{
    public class UdpBridge : Bridge<IPEndPoint>
    {
        private UdpSocket _socket;
        private BinaryFormatterSerializer<Message> _serializer;
        private IPEndPoint _host;

        public UdpBridge(IPEndPoint host)
        {
            _host = host;
            _serializer = new BinaryFormatterSerializer<Message>();
            _socket = new UdpSocket();
            _socket.MaxPayloadSizeBytes = 2048;
        }

        public override void Start()
        {
            _socket.ReceivedPacket += SocketOnReceivedPacket;
            _socket.StartListen(_host);
        }

        public override void Stop()
        {
            _socket.ReceivedPacket -= SocketOnReceivedPacket;
            _socket.StopReceive();
        }

        public override void Send(IPEndPoint receiver, Message message)
        {
            byte[] data = _serializer.Serialize(message);
            _socket.Send(data, receiver);
        }

        private void SocketOnReceivedPacket(object sender, ReceivedUdpPacketEventArgs eventArgs)
        {
            Message message = _serializer.Deserialize(eventArgs.Received);
            HandleMessage(eventArgs.RemoteIpEndPoint, message);
        }
    }
}