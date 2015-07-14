namespace SvrKit.Networking.Proxy
{
    using System;
    using System.Net.Sockets;

    public class ProxyBase
    {
        public bool isClientConnected;
        public ProxyConfig ProxyConfig { get; private set; }

        protected Socket clientSocket;
        protected Socket serverSocket;
        protected byte[] buffer;

        protected ProxyBase(ProxyConfig proxyConfig)
        {
            this.ProxyConfig = proxyConfig;
            this.buffer = new byte[this.ProxyConfig.BufferSize];
        }

        protected void Disconnected(Socket socket, string reason)
        {
            this.isClientConnected = false;
            this.ProxyConfig.Logger.Write(reason);
        }

        protected virtual void ReceivedPacket(ProxyPacket proxyPacket) { }

        public void Send(PacketCrafter packetCrafter)
        {
            packetCrafter.Finish();
            this.Send(packetCrafter.Buffer);
        }

        public void Send(byte[] data)
        {
            this.clientSocket.Send(data);
        }

        protected void Listen()
        {
            while (this.isClientConnected)
            {
                if (!this.clientSocket.Connected)
                {
                    this.Disconnected(this.clientSocket, "Client Not Connected");
                }

                if (this.clientSocket.Poll(100, SelectMode.SelectRead))
                {
                    try
                    {
                        if (this.clientSocket.Receive(this.headerBuffer, 0, this.ProxyConfig.HeaderSize, SocketFlags.None) < 1)
                        {
                            this.Disconnected(this.clientSocket, "Nothing To Receive");
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        if (!this.clientSocket.Connected)
                        {
                            this.Disconnected(clientSocket, "Client Error");
                        }
                        else
                        {
                            this.Disconnected(clientSocket, "failed to receive packet: " + e.Message);
                        }
                        continue;
                    }

                    Int32 packetLength = BitConverter.ToInt16(this.headerBuffer, 0) - this.ProxyConfig.HeaderSize;
                    Int32 packetId = BitConverter.ToInt16(this.headerBuffer, 2);

                    this.proxyHost.WriteOutput("Length: " + packetLength.ToString());
                    this.proxyHost.WriteOutput("ID: " + packetId.ToString());
                    if (packetLength < 0)
                    {
                        this.Disconnected(this.clientSocket, "Message length is less than zero");
                        continue;
                    }

                    if (this.ProxyConfig.BufferSize > 0 && packetLength > this.ProxyConfig.BufferSize)
                    {
                        this.Disconnected(clientSocket, "Message length " + packetLength + " is larger than maximum message size " + this.ProxyConfig.BufferSize);
                        continue;
                    }

                    if (packetLength == 0)
                    {
                        this.Disconnected(this.clientSocket, "empty packet");
                        continue;
                    }

                    dataBuffer = new byte[packetLength];
                    int bytesReceived = 0;

                    while (bytesReceived < packetLength)
                    {
                        if (clientSocket.Poll(100, SelectMode.SelectRead))
                            bytesReceived += clientSocket.Receive(dataBuffer, bytesReceived, packetLength - bytesReceived, SocketFlags.None);
                    }

                    ProxyPacket proxyPacket = new ProxyPacket(packetLength);
                    proxyPacket.PacketId = packetId;
                    proxyPacket.PacketSize = packetLength;
                    proxyPacket.Buffer = dataBuffer;
                    proxyPacket.BufferPosition = 0;

                    byte[] rawPacket = new byte[this.ProxyConfig.HeaderSize + packetLength];
                    Buffer.BlockCopy(headerBuffer, 0, rawPacket, 0, this.ProxyConfig.HeaderSize);
                    Buffer.BlockCopy(dataBuffer, 0, rawPacket, this.ProxyConfig.HeaderSize, packetLength);
                    proxyPacket.RawPacket = rawPacket;

                    this.ReceivedPacket(proxyPacket);
                }




            }
        }



    }
}

