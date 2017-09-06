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
namespace Arrowgene.Services.Network.TCP.Client
{
    using Common;
    using Logging;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System;

    public class ClientSocket
    {
        private Logger logger;

        internal Socket Socket { get; private set; }

        public int Id { get; private set; }
        public bool IsBusy { get; internal set; }
        public bool IsAlive { get; internal set; }
        public int InTraffic { get; internal set; }
        public int OutTraffic { get; internal set; }

        public IPEndPoint RemoteIpEndPoint { get { return this.Socket.RemoteEndPoint as IPEndPoint; } }

        public IPAddress RemoteIPAddress
        {
            get
            {
                IPAddress ipAddress = null;

                if (this.RemoteIpEndPoint != null)
                {
                    ipAddress = this.RemoteIpEndPoint.Address;
                }

                return ipAddress;
            }
        }

        public ClientSocket(int id, Socket socket, Logger logger)
        {
            this.Socket = socket;
            this.logger = logger;

            this.Id = id;
            this.IsAlive = true;
            this.IsBusy = false;
            this.InTraffic = 0;
            this.OutTraffic = 0;
        }

        public void Send(byte[] payload)
        {
            this.Socket.Send(payload);
            this.OutTraffic += payload.Length;
        }

        public void Close()
        {
            this.IsAlive = false;
            if (this.Socket.Connected)
            {
                try
                {
                    this.Socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception ex)
                {

                }
            }
            this.Socket.Close();
        }
    }


}
