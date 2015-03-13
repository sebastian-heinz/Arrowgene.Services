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
namespace MarrySocket.MServer
{
    using MarrySocket.MBase;
    using MarrySocket.MExtra;
    using MarrySocket.MExtra.Logging;
    using MarrySocket.MExtra.Serialization;
    using System;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// TODO SUMMARY
    /// </summary>
    public class ClientSocket : BaseSocket
    {
        private IPEndPoint remoteIpEndPoint;

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public ClientSocket(Socket socket, Logger logger, ISerialization serializer)
            : base(socket, logger, serializer)
        {
            this.Id = Maid.Random.Next(9999);
            this.IsAlive = true;
            this.remoteIpEndPoint = socket.RemoteEndPoint as IPEndPoint;
            this.LastPing = DateTime.Now;
            this.IsBusy = false;
        }

        internal bool IsBusy { get; set; }
        internal bool IsAlive { get; set; }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public DateTime LastPing { get; private set; }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        public string Ip { get { return ((IPEndPoint)this.Socket.RemoteEndPoint).Address.ToString(); } }

        /// <summary>
        /// Close the socket connection.
        /// </summary>
        public void Close()
        {
            this.Disconnect();
        }

        /// <summary>
        /// Internal socket close handeling.
        /// </summary>
        protected override void Disconnect()
        {
            base.Disconnect();
            this.IsAlive = false;
        }

        /// <summary>
        /// TODO SUMMARY
        /// </summary>
        protected override void Error(string error)
        {
            base.logger.Write(error, LogType.ERROR);
        }
    }
}
