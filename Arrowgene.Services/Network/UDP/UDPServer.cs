/*
 *  Copyright 2015 Sebastian Heinz <sebastian.heinz.gt@googlemail.com>
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 */
namespace Arrowgene.Services.Network.UDP
{
    using Arrowgene.Services.Provider;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// Listen for UDP Packets
    /// </summary>
    public class UDPServer
    {
        /// <summary>
        /// Defines the maximum size to be received,
        /// drops send requests exceeding this limit.
        /// </summary>
        public const int MAX_PAYLOAD_SIZE_BYTES = 384;

        private int port;
        private IPAddress ipAddress;
        private Socket socket;
        private byte[] buffer;
        IAsyncResult asyncResult;

        /// <summary>
        /// Initialize with given port
        /// </summary>
        /// <param name="port"></param>
        public UDPServer(int port)
        {
            this.ipAddress = IPAddress.Any;
            this.port = port;
            this.buffer = new byte[MAX_PAYLOAD_SIZE_BYTES];
            this.IsListening = false;
        }

        /// <summary>
        /// Server IPEndPoint
        /// </summary>
        public IPEndPoint IPEndPoint { get { return new IPEndPoint(this.ipAddress, this.port); } }

        /// <summary>
        /// IsListening
        /// </summary>
        public bool IsListening { get; private set; }

        /// <summary>
        /// Notifies packet received
        /// </summary>
        public event EventHandler<ReceivedUDPPacketEventArgs> ReceivedPacket;

        /// <summary>
        /// Start Listening
        /// </summary>
        public void Listen()
        {
            this.socket = AGSocket.CreateBoundServerSocket(this.IPEndPoint, SocketType.Dgram, ProtocolType.Udp);
            this.socket.EnableBroadcast = true;
            this.IsListening = true;
            this.Receive();
        }

        /// <summary>
        /// Stops Listening
        /// </summary>
        public void Stop()
        {
            this.IsListening = false;
            this.socket.Close();
        }

        private void Receive()
        {
            if (this.IsListening)
            {
                EndPoint localEndPoint = this.IPEndPoint as EndPoint;
                this.asyncResult = this.socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref localEndPoint, ReceiveCallback, null);
            }
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
                Debug.WriteLine("UDPServer::ReceiveCallbackPacket: Socket Closed");
                return;
            }

            if (receivedBytesCount <= 0)
            {
                Debug.WriteLine(string.Format("UDPServer::ReceiveCallbackPacket: Invalid Packet size ({0} bytes)", receivedBytesCount));
                return;
            }

            if (receivedBytesCount >= UDPServer.MAX_PAYLOAD_SIZE_BYTES)
            {
                Debug.WriteLine(string.Format("UDPServer::ReceiveCallbackPacket: dropped packet({0} bytes), exceeded maximum size of {1} bytes", receivedBytesCount, UDPServer.MAX_PAYLOAD_SIZE_BYTES));
                return;
            }

            IPEndPoint remoteIPEndPoint = (IPEndPoint)remoteEnd;
            byte[] received = ByteBuffer.BlockCopy(this.buffer, receivedBytesCount);

            this.OnReceivedUDPPacket(receivedBytesCount, received, remoteIPEndPoint);
            this.Receive();
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