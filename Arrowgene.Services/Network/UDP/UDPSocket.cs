namespace Arrowgene.Services.Network.UDP
{
    using Provider;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;

    public class UDPSocket
    {
        /// <summary>
        /// Defines the maximum size to be received,
        /// drops send requests exceeding this limit.
        /// </summary>
        public const int MAX_PAYLOAD_SIZE_BYTES = 384;

        protected int localPort;
        protected IPAddress localIPAddress;

        protected Socket socket;
        private byte[] buffer;

        private IAsyncResult asyncResult;

        public UDPSocket(IPEndPoint localEP) : this(localEP.Address, localEP.Port)
        {
        }

        public UDPSocket(IPAddress localIPAddress, int localPort)
        {
            this.localPort = localPort;
            this.localIPAddress = localIPAddress;
            this.Init();
        }

        public IPEndPoint LocalIPEndPoint { get { return new IPEndPoint(this.localIPAddress, this.localPort); } }


        public event EventHandler<ReceivedUDPPacketEventArgs> ReceivedPacket;

        private void Init()
        {
            this.buffer = new byte[MAX_PAYLOAD_SIZE_BYTES];
            this.socket = new Socket(this.localIPAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            this.socket.Bind(this.LocalIPEndPoint);
            this.Receive();
        }

        public void Stop()
        {
            this.socket.Close();
        }

        public void SendTo(byte[] buffer, EndPoint remoteEP)
        {
            if (buffer.Length <= UDPSocket.MAX_PAYLOAD_SIZE_BYTES)
            {
                this.socket.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, remoteEP, null, null);
            }
            else
            {
                Debug.WriteLine(string.Format("UDPBase::SendTo: Exceeded maximum size of {0} byte", UDPSocket.MAX_PAYLOAD_SIZE_BYTES));
            }
        }

        private void Receive()
        {
            EndPoint localEndPoint = this.LocalIPEndPoint as EndPoint;
            this.asyncResult = this.socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref localEndPoint, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult iar)
        {
            EndPoint remoteEnd = (EndPoint)iar.AsyncState;
            int receivedBytesCount = 0;

            try
            {
                receivedBytesCount = this.socket.EndReceiveFrom(iar, ref remoteEnd);
            }
            catch (ObjectDisposedException odex)
            {
                Debug.WriteLine("UDPBase::ReceiveCallbackPacket: Socket Closed");
                return;
            }

            if (receivedBytesCount <= 0)
            {
                Debug.WriteLine(string.Format("UDPBase::ReceiveCallbackPacket: Invalid Packet size ({0} bytes)", receivedBytesCount));
                return;
            }

            if (receivedBytesCount >= UDP.UDPSocket.MAX_PAYLOAD_SIZE_BYTES)
            {
                Debug.WriteLine(string.Format("UDPBase::ReceiveCallbackPacket: dropped packet({0} bytes), exceeded maximum size of {1} bytes", receivedBytesCount, UDP.UDPSocket.MAX_PAYLOAD_SIZE_BYTES));
                return;
            }

            IPEndPoint remoteIPEndPoint = (IPEndPoint)remoteEnd;
            byte[] received = ByteBuffer.BlockCopy(this.buffer, receivedBytesCount);

            this.Receive();

            this.OnReceivedUDPPacket(receivedBytesCount, received, remoteIPEndPoint);
        }

        protected virtual void OnReceivedUDPPacket(int receivedBytesCount, byte[] received, IPEndPoint remoteIPEndPoint)
        {
            EventHandler<ReceivedUDPPacketEventArgs> receivedBroadcast = this.ReceivedPacket;

            if (received != null)
            {
                ReceivedUDPPacketEventArgs receivedProxyPacketEventArgs = new ReceivedUDPPacketEventArgs(receivedBytesCount, received, remoteIPEndPoint);
                receivedBroadcast(this, receivedProxyPacketEventArgs);
            }
        }

    }
}