namespace Arrowgene.Services.Network.UDP
{
    using Provider;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class UDPSocket
    {
        /// <summary>
        /// Defines the maximum size to be received or send,
        /// drops requests exceeding this limit.
        /// </summary>
        public const int MAX_PAYLOAD_SIZE_BYTES = 384;

        private Socket socket;
        private byte[] buffer;
        private Thread udpThread;
        private bool receive;


        public UDPSocket()
        {
            this.receive = false;
            this.buffer = new byte[MAX_PAYLOAD_SIZE_BYTES];
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        /// <summary>
        /// Occurs when data is received
        /// </summary>
        public event EventHandler<ReceivedUDPPacketEventArgs> ReceivedPacket;

        /// <summary>
        /// Listen for incomming data
        /// </summary>
        /// <param name="remoteEP"></param>
        public void StartListen(IPEndPoint remoteEP)
        {
            this.socket.Bind(remoteEP);
            this.StartReceive();
        }

        /// <summary>
        /// Send data to an destination
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="remoteEP"></param>
        public void Send(byte[] buffer, EndPoint remoteEP)
        {
            this.SendTo(buffer, remoteEP);
            this.StartReceive();
        }

        /// <summary>
        /// Send data as broadcast.
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="port"></param>
        public void SendBroadcast(byte[] buffer, int port)
        {
            this.SendToBroadcast(buffer, port);
            this.StartReceive();
        }

        /// <summary>
        /// Stops receiving any data
        /// </summary>
        public void Close()
        {
            this.receive = false;

            if (this.udpThread != null)
            {
                int waitTimeout = 1000;

                if (!this.udpThread.Join(waitTimeout))
                {
                    Debug.WriteLine(string.Format("UDPBase::Stop: Exceeded maximum timeout of {0} ms, aborting thread...", waitTimeout));
                    this.udpThread.Abort();
                }
            }

            this.socket.Close();
        }

        private void StartReceive()
        {
            if (!this.receive)
            {
                this.receive = true;
                this.udpThread = new Thread(Receive);
                this.udpThread.Name = "UdpReceive";
                this.udpThread.Start();
            }
        }

        private void Receive()
        {
            while (this.receive)
            {
                if (socket.Poll(10, SelectMode.SelectRead))
                {
                    // Create EndPoint, Senders information will be written to this object.
                    IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                    EndPoint senderRemote = (EndPoint)sender;

                    int read = this.socket.ReceiveFrom(this.buffer, 0, this.buffer.Length, SocketFlags.None, ref senderRemote);

                    IPEndPoint remoteIPEndPoint = (IPEndPoint)senderRemote;
                    byte[] received = ByteBuffer.BlockCopy(this.buffer, read);
                    this.OnReceivedUDPPacket(read, received, remoteIPEndPoint);
                }
            }
        }

        private void SendTo(byte[] buffer, EndPoint remoteEP)
        {
            if (buffer.Length <= UDPSocket.MAX_PAYLOAD_SIZE_BYTES)
            {
                this.socket.SendTo(buffer, 0, buffer.Length, SocketFlags.None, remoteEP);
            }
            else
            {
                Debug.WriteLine(string.Format("UDPBase::SendTo: Exceeded maximum size of {0} byte", MAX_PAYLOAD_SIZE_BYTES));
            }
        }

        private void SendToBroadcast(byte[] buffer, int port)
        {
            this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            this.socket.SendTo(buffer, 0, buffer.Length, SocketFlags.None, new IPEndPoint(IPAddress.Broadcast, port));
            this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, false);
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