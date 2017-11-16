/*
 * MIT License
 * 
 * Copyright (c) 2018 Sebastian Heinz <sebastian.heinz.gt@googlemail.com>
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace Arrowgene.Services.Network.Udp
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using Buffers;

    /// <summary>
    /// Class for handling udp sending and receiving of packets.
    /// Call <see cref="StartReceive"/> before sending any data, to be able to receive a response.
    /// If you act as a server with <see cref="StartListen(IPEndPoint)"/>, there is no need to call <see cref="StartReceive"/>
    /// </summary>
    public class UdpSocket
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
        private bool isBound;

        /// <summary>
        /// Creates a new instance of <see cref="UdpSocket"/>
        /// </summary>
        public UdpSocket()
        {
            this.isBound = false;
            this.receive = false;
            this.buffer = new byte[MAX_PAYLOAD_SIZE_BYTES];
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        /// <summary>
        /// Occurs when data is received
        /// </summary>
        public event EventHandler<ReceivedUdpPacketEventArgs> ReceivedPacket;

        /// <summary>
        /// Listen for incomming data and start receiving
        /// </summary>
        /// <param name="remoteEP"></param>
        public void StartListen(IPEndPoint remoteEP)
        {
            if (!this.isBound)
            {
                this.socket.Bind(remoteEP);
                this.isBound = true;
            }
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
        }

        /// <summary>
        /// Send data as broadcast.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="port"></param>
        public void SendBroadcast(byte[] buffer, int port)
        {
            this.SendToBroadcast(buffer, port);
        }

        /// <summary>
        /// Starts receiving data
        /// </summary>
        public void StartReceive()
        {
            this.StartReceiveThread();
        }

        /// <summary>
        /// Stops receiving any data
        /// </summary>
        public void StopReceive()
        {
            this.receive = false;

            if (this.udpThread != null)
            {
                if (Thread.CurrentThread != this.udpThread)
                {
                    int waitTimeout = 1000;

                    if (this.udpThread.Join(waitTimeout))
                    {
                        Debug.WriteLine(string.Format("UDPBase::Stop: Udp thread ended clean.", waitTimeout));
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("UDPBase::Stop: Exceeded maximum timeout of {0} ms, aborting thread...", waitTimeout));
                        this.udpThread.Abort();
                    }
                }
                else
                {
                    Debug.WriteLine("UDPBase::Stop: Tried to join udp thread from within udp thread, letting udp thread run out..");
                }
            }
        }

        /// <summary>
        /// Releases all ressources
        /// </summary>
        public void Dispose()
        {
            this.StopReceive();
            this.socket.Close();
        }

        private void StartReceiveThread()
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
                    EndPoint senderRemote = (EndPoint) sender;

                    int read = this.socket.ReceiveFrom(this.buffer, 0, this.buffer.Length, SocketFlags.None, ref senderRemote);

                    IPEndPoint remoteIPEndPoint = (IPEndPoint) senderRemote;

                    IBuffer received = new ByteBuffer(this.buffer, 0, read);
                    this.OnReceivedUDPPacket(read, received.GetAllBytes(), remoteIPEndPoint);
                }
            }
        }

        private void SendTo(byte[] buffer, EndPoint remoteEP)
        {
            if (buffer.Length <= UdpSocket.MAX_PAYLOAD_SIZE_BYTES)
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
            EventHandler<ReceivedUdpPacketEventArgs> receivedBroadcast = this.ReceivedPacket;

            if (received != null)
            {
                ReceivedUdpPacketEventArgs receivedProxyPacketEventArgs = new ReceivedUdpPacketEventArgs(receivedBytesCount, received, remoteIPEndPoint);
                receivedBroadcast(this, receivedProxyPacketEventArgs);
            }
        }
    }
}