namespace Arrowgene.Services.Network
{
    using Arrowgene.Services.Network.Discovery;
    using Arrowgene.Services.Provider;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class Broadcast
    {
        private int port;
        private Thread broadcastThread;
        private Socket socket;
        private bool isListening;

        public Broadcast(int port)
        {
            this.isListening = false;
            this.port = port;
        }


        public event EventHandler<ReceivedBroadcastPacketEventArgs> ReceivedBroadcast;


        private void CreateSocket()
        {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
        }

        public void Send(byte[] data)
        {
            this.Send(data, IPAddress.Broadcast);
        }

        public void Send(byte[] data, IPAddress ip)
        {
            IPEndPoint broadcastEndPoint = new IPEndPoint(ip, this.port);
            this.CreateSocket();
            this.socket.Connect(broadcastEndPoint);
            this.socket.Send(data);
            this.socket.Close();
        }

        public void Listen()
        {
            this.CreateSocket();
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, this.port);
            socket.Bind(localEndPoint);
          
            this.isListening = true;
            this.broadcastThread = new Thread(this.Read);
            this.broadcastThread.Name = "Broadcast";
            this.broadcastThread.Start();
        }


        protected void Read()
        {
            while (this.isListening)
            {
                if (socket.IsBound)
                {
                    ByteBuffer payload = new ByteBuffer();
                    byte[] buffer = new byte[1024];
                    try
                    {
                        int received = 0;

                        while (socket.Poll(10, SelectMode.SelectRead) && (received = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None)) > 0)
                        {
                            payload.WriteBytes(buffer, 0, received);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
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

        protected void OnReceivedBroadcast(ByteBuffer payload)
        {
            EventHandler<ReceivedBroadcastPacketEventArgs> receivedBroadcast = this.ReceivedBroadcast;

            if (payload != null)
            {
                ReceivedBroadcastPacketEventArgs receivedProxyPacketEventArgs = new ReceivedBroadcastPacketEventArgs(payload);
                receivedBroadcast(this, receivedProxyPacketEventArgs);
            }
        }

    }
}