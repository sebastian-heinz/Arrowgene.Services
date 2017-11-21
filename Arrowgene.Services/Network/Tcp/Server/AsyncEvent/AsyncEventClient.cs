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


namespace Arrowgene.Services.Network.Tcp.Server.AsyncEvent
{
    using System;
    using System.Net.Sockets;
    using System.Collections.Generic;
    using Tcp;


    public class AsyncEventClient : ITcpSocket
    {
        public Socket Socket { get; private set; }

        internal SocketAsyncEventArgs ReadEvent { get; }
        internal List<SocketAsyncEventArgs> WriteEvents { get; }

        private AsyncEventServer _server;

        public AsyncEventClient(AsyncEventServer server)
        {
            _server = server;
            WriteEvents = new List<SocketAsyncEventArgs>();
            ReadEvent = new SocketAsyncEventArgs();
            ReadEvent.UserToken = this;
        }

        public void Accept(Socket socket)
        {
            Socket = socket;
        }

        public void Send(byte[] data)
        {
            _server.SendData(this, data);
        }

        public void Close()
        {
            try
            {
                Socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception)
            {
            }
            Socket.Close();
        }
    }
}