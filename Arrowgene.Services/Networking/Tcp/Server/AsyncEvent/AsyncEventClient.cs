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


using System;
using System.Net;
using System.Net.Sockets;

namespace Arrowgene.Services.Networking.Tcp.Server.AsyncEvent
{
    public class AsyncEventClient : ITcpSocket
    {
        public string Identity { get; }
        public IPAddress RemoteIpAddress { get; }
        public ushort Port { get; }
        public int UnitOfOrder { get; }

        public bool IsAlive
        {
            get
            {
                lock (_lock)
                {
                    return _isAlive;
                }
            }
        }

        public Socket Socket { get; }
        public SocketAsyncEventArgs ReadEventArgs { get; private set; }
        public DateTime LastActive { get; set; }

        private bool _isAlive;
        private readonly AsyncEventServer _server;
        private readonly object _lock;

        public AsyncEventClient(Socket socket, SocketAsyncEventArgs readEventArgs, AsyncEventServer server, int uoo)
        {
            _lock = new object();
            _isAlive = true;
            Socket = socket;
            ReadEventArgs = readEventArgs;
            _server = server;
            UnitOfOrder = uoo;
            LastActive = DateTime.Now;
            if (Socket.RemoteEndPoint is IPEndPoint ipEndPoint)
            {
                RemoteIpAddress = ipEndPoint.Address;
                Port = (ushort) ipEndPoint.Port;
            }

            Identity = $"{RemoteIpAddress}:{Port}";
        }

        public void Send(byte[] data)
        {
            _server.Send(this, data);
        }

        public void Close()
        {
            lock (_lock)
            {
                if (!_isAlive)
                {
                    return;
                }

                _isAlive = false;
            }

            try
            {
                Socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                // ignored
            }

            Socket.Close();
            _server.NotifyDisconnected(this);
            ReadEventArgs = null;
        }
    }
}