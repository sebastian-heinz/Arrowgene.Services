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
namespace Arrowgene.Services.Network.Broadcast
{
    using Arrowgene.Services.Network.Discovery;
    using Arrowgene.Services.Provider;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Listen or Send a Broadcast
    /// </summary>
    public class BroadcastServer
    {
        /// <summary>
        /// Defines the maximum size to be received,
        /// drops send requests exceeding this limit.
        /// </summary>
        public const int MAX_PAYLOAD_SIZE_BYTES = 384;

        private int port;
        private Thread broadcastThread;
        private Socket socket;
        private bool isListening;
        private byte[] buffer;

        /// <summary>
        /// Initialize with given port
        /// </summary>
        /// <param name="port"></param>
        public BroadcastServer(int port)
        {
            this.isListening = false;
            this.port = port;
            this.buffer = new byte[MAX_PAYLOAD_SIZE_BYTES];
        }

        /// <summary>
        /// Notifies broadcast received
        /// </summary>
        public event EventHandler<ReceivedBroadcastEventArgs> ReceivedBroadcast;


        private void Read()
        {

            while (this.isListening)
            {
                if (socket.IsBound)
                {
                    // ByteBuffer payload = new ByteBuffer();
                    //  byte[] buffer = new byte[MAX_PAYLOAD_SIZE_BYTES];
                    try
                    {
                        //  int received = 0;
                        // if (socket.Poll(10, SelectMode.SelectRead)) //&& (received = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None)) > 0)
                        //   {
                        IPEndPoint localIPEndPoint = new IPEndPoint(IPAddress.IPv6Any, this.port);
                        EndPoint localEndPoint = localIPEndPoint as EndPoint;
                        //diff ep?
                        this.socket.BeginReceiveMessageFrom(buffer, 0, buffer.Length, SocketFlags.None, ref localEndPoint, ReceiveCallback, this.socket);


                        // payload.WriteBytes(buffer, 0, received);
                        //  }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(string.Format("Broadcast::Read: {0}", ex.ToString()));
                        this.isListening = false;
                    }

                    //   if (payload.Size > 0)
                    //  {
                    //       this.OnReceivedBroadcast(payload);
                    //    }

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
            EventHandler<ReceivedBroadcastEventArgs> receivedBroadcast = this.ReceivedBroadcast;

            if (payload != null)
            {
                ReceivedBroadcastEventArgs receivedProxyPacketEventArgs = new ReceivedBroadcastEventArgs(payload, this.socket.RemoteEndPoint);
                receivedBroadcast(this, receivedProxyPacketEventArgs);
            }
        }

        private void ReceiveCallback(IAsyncResult iar)
        {
            IPPacketInformation packetInfo;
            EndPoint remoteEnd = new IPEndPoint(IPAddress.Any, this.port);
            SocketFlags flags = SocketFlags.None;
            Socket sock = (Socket)iar.AsyncState;
         
            int received = sock.EndReceiveMessageFrom(iar, ref flags, ref remoteEnd, out packetInfo);
            Debug.WriteLine(string.Format(
                "{0} bytes received from {1} to {2}",
                received,
                remoteEnd,
                packetInfo.Address)
            );
        }




        /// <summary>
        /// Listen for broadcast messages
        /// </summary>
        public void Listen()
        {
            IPEndPoint localIPEndPoint = new IPEndPoint(IPAddress.Any, this.port);
            EndPoint localEndPoint = localIPEndPoint as EndPoint;

            this.socket = AGSocket.CreateBoundServerSocket(localIPEndPoint, SocketType.Dgram, ProtocolType.Udp);
            this.socket.EnableBroadcast = true;
            this.isListening = true;

           

         //   IPEndPoint localIPEndPoint = new IPEndPoint(IPAddress.IPv6Any, this.port);
        //  //  EndPoint localEndPoint = localIPEndPoint as EndPoint;
    
           this.socket.BeginReceiveMessageFrom(buffer, 0, buffer.Length, SocketFlags.None, ref localEndPoint, ReceiveCallback, this.socket);



       //     this.broadcastThread = new Thread(this.Read);
       //     this.broadcastThread.Name = "Broadcast";
      //      this.broadcastThread.Start();
        }

        /// <summary>
        /// Stops Listening
        /// </summary>
        public void Stop()
        {
            this.isListening = false;
            this.socket.Close();
        }

    }
}