namespace Arrowgene.Services.Network.UDP
{
    using Provider;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;

    public abstract class UDPBase
    {
        /// <summary>
        /// Defines the maximum size to be received,
        /// drops send requests exceeding this limit.
        /// </summary>
        public const int MAX_PAYLOAD_SIZE_BYTES = 384;

        protected int port;
        protected IPAddress ipAddress;
        protected Socket socket;
        protected byte[] buffer;
        protected IAsyncResult asyncResult;

        public UDPBase(int port)
        {
            this.buffer = new byte[MAX_PAYLOAD_SIZE_BYTES];
            this.ipAddress = IPAddress.Any;
            this.port = port;
        }

        /// <summary>
        /// Socket IPEndPoint
        /// </summary>
        public IPEndPoint IPEndPoint { get { return new IPEndPoint(this.ipAddress, this.port); } }

        /// <summary>
        /// Notifies packet received
        /// </summary>
        public event EventHandler<ReceivedUDPPacketEventArgs> ReceivedPacket;

        /// <summary>
        /// Send
        /// </summary>
        /// <param name="buffer"></param>
        public virtual void SendTo(byte[] buffer, EndPoint remoteEP)
        {
            if (buffer.Length <= UDPServer.MAX_PAYLOAD_SIZE_BYTES)
            {
                this.socket.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, remoteEP, null, null);
            }
            else
            {
                Debug.WriteLine(string.Format("UDPBase::SendTo: Exceeded maximum size of {0} byte", UDPServer.MAX_PAYLOAD_SIZE_BYTES));
            }
        }

        protected virtual void Receive()
        {
                EndPoint localEndPoint = this.IPEndPoint as EndPoint;
                this.asyncResult = this.socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref localEndPoint, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult iar)
        {
            EndPoint remoteEnd = new IPEndPoint(IPAddress.Any, this.port);
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

            if (receivedBytesCount >= UDPBase.MAX_PAYLOAD_SIZE_BYTES)
            {
                Debug.WriteLine(string.Format("UDPBase::ReceiveCallbackPacket: dropped packet({0} bytes), exceeded maximum size of {1} bytes", receivedBytesCount, UDPBase.MAX_PAYLOAD_SIZE_BYTES));
                return;
            }

            IPEndPoint remoteIPEndPoint = (IPEndPoint)remoteEnd;
            byte[] received = ByteBuffer.BlockCopy(this.buffer, receivedBytesCount);

            this.Receive();

            this.OnReceivedUDPPacket(receivedBytesCount, received, remoteIPEndPoint);
        }

        private void OnReceivedUDPPacket(int receivedBytesCount, byte[] received, IPEndPoint remoteIPEndPoint)
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
