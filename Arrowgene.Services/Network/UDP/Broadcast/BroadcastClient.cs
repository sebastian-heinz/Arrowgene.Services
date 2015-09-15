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
namespace Arrowgene.Services.Network.UDP.Broadcast
{
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// Send a Broadcast
    /// </summary>
    public class BroadcastClient
    {
        /// <summary>
        /// Initialize BroadcastClient
        /// </summary>
        public BroadcastClient()
        {

        }

        /// <summary>
        /// Send a broadcast message, to a given <see cref="IPAddress"/>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Send(byte[] data, IPAddress ip, int port)
        {
            if (data.Length <= UDPServer.MAX_PAYLOAD_SIZE_BYTES)
            {
                IPEndPoint broadcastEndPoint = new IPEndPoint(ip, port);

                Socket socket = AGSocket.CreateSocket(broadcastEndPoint, SocketType.Dgram, ProtocolType.Udp);
                socket.Connect(broadcastEndPoint);

                socket.Send(data);
                socket.Close();
            }
            else
            {
                Debug.WriteLine(string.Format("BroadcastClient::Send: Exceeded maximum size of {0} byte", UDPServer.MAX_PAYLOAD_SIZE_BYTES));
            }
        }

        /// <summary>
        /// Send a broadcast message to <see cref="IPAddress.Broadcast"/>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="port"></param>
        public void Send(byte[] data, int port)
        {
            this.Send(data, IPAddress.Broadcast, port);
        }

    }
}