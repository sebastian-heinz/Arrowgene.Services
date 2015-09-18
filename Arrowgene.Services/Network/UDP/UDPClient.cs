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
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// Send a Broadcast
    /// </summary>
    public class UDPClient : UDPBase
    {

        /// <summary>
        /// Send a broadcast message, to a given <see cref="IPAddress"/>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public static void SendBroadcast(byte[] data, IPAddress ip, int port)
        {
            if (data.Length <= UDPServer.MAX_PAYLOAD_SIZE_BYTES)
            {
                IPEndPoint broadcastEndPoint = new IPEndPoint(ip, port);

                Socket socket = AGSocket.CreateSocket(broadcastEndPoint, SocketType.Dgram, ProtocolType.Udp);
                socket.EnableBroadcast = true;
                socket.Connect(broadcastEndPoint);
                socket.Send(data);
                socket.Close();
            }
            else
            {
                Debug.WriteLine(string.Format("UDPClient::SendBroadcast: Exceeded maximum size of {0} byte", UDPServer.MAX_PAYLOAD_SIZE_BYTES));
            }
        }

        public static void SendBroadcast(byte[] data, int port)
        {
            SendBroadcast(data, IPAddress.Broadcast, port);
        }


        /// <summary>
        /// Initialize BroadcastClient
        /// </summary>
        public UDPClient(int port) : base (port)
        {
            this.socket = AGSocket.CreateSocket(this.IPEndPoint, SocketType.Dgram, ProtocolType.Udp);
        }

        public void Connect(IPAddress ipAddress, int port)
        {
            this.socket.Connect(ipAddress, port);
            this.Receive();
        }

        public override void SendTo(byte[] buffer, EndPoint remoteEP)
        {
            base.SendTo(buffer, remoteEP);
        }




    }
}