namespace Arrowgene.Services.Network
{
    using Arrowgene.Services.Network.Discovery;
    using Arrowgene.Services.Provider;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// Listen or Send a Broadcast
    /// </summary>
    public class Broadcast
    {
        /// <summary>
        /// Defines the maximum size to be received,
        /// drops send requests exceeding this limit.
        /// </summary>
        public const int MAX_PAYLOAD_SIZE_BYTES = 384;

        private int port;
        private Thread broadcastThread;
        private AGSocket socket;
        private bool isListening;

        /// <summary>
        /// Initialize with given port
        /// </summary>
        /// <param name="port"></param>
        public Broadcast(int port)
        {
            this.socket = new AGSocket();
            this.isListening = false;
            this.port = port;
        }

        /// <summary>
        /// Notifies broadcast received
        /// </summary>
        public event EventHandler<ReceivedBroadcastPacketEventArgs> ReceivedBroadcast;


        private void Read()
        {
            while (this.isListening)
            {
                if (socket.IsBound)
                {
                    ByteBuffer payload = new ByteBuffer();
                    byte[] buffer = new byte[MAX_PAYLOAD_SIZE_BYTES];
                    try
                    {
                        int received = 0;
                        if (socket.Poll(10, SelectMode.SelectRead) && (received = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None)) > 0)
                        {
                            payload.WriteBytes(buffer, 0, received);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(string.Format("Broadcast::Read: {0}", ex.ToString()));
                        this.isListening = false;
                    }

                    if (payload.Size > 0)
                    {
                        this.OnReceivedBroadcast(payload);
                    }

                    Thread.Sleep(10);
                }
                else
                {
                    this.isListening = false;
                }
            }
        }

        private void OnReceivedBroadcast(ByteBuffer payload)
        {
            EventHandler<ReceivedBroadcastPacketEventArgs> receivedBroadcast = this.ReceivedBroadcast;

            if (payload != null)
            {
                ReceivedBroadcastPacketEventArgs receivedProxyPacketEventArgs = new ReceivedBroadcastPacketEventArgs(payload);
                receivedBroadcast(this, receivedProxyPacketEventArgs);
            }
        }

        /// <summary>
        /// Listen for broadcast messages
        /// </summary>
        public void Listen()
        {

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.IPv6Any, this.port);
            socket.Bind(localEndPoint, SocketType.Dgram, ProtocolType.Udp, true);
            socket.EnableBroadcast = true;
            this.isListening = true;
            this.broadcastThread = new Thread(this.Read);
            this.broadcastThread.Name = "Broadcast";
            this.broadcastThread.Start();
        }

        /// <summary>
        /// Send a broadcast message, to a given <see cref="IPAddress"/>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ip"></param>
        public void Send(byte[] data, IPAddress ip)
        {
            if (data.Length <= Broadcast.MAX_PAYLOAD_SIZE_BYTES)
            {
                IPEndPoint broadcastEndPoint = new IPEndPoint(ip, this.port);

                this.socket.Connect(broadcastEndPoint, SocketType.Dgram, ProtocolType.Udp);
                this.socket.Send(data);
                this.socket.Close();
            }
            else
            {
                Debug.WriteLine(string.Format("Broadcast::Send: Exceeded maximum payload size of {0} byte", Broadcast.MAX_PAYLOAD_SIZE_BYTES));
            }
        }

        /// <summary>
        /// Send a broadcast message to <see cref="IPAddress.Broadcast"/>
        /// </summary>
        /// <param name="data"></param>
        public void Send(byte[] data)
        {
            this.Send(data, IPAddress.Broadcast);
        }

    }
}