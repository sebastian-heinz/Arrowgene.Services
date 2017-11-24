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


namespace Arrowgene.Services.Network.Tcp.Server
{
    using System.Net;
    using Logging;
    using Exceptions;
    using EventConsumer;

    public abstract class TcpServer : ITcpServer
    {
        protected TcpServer(IPAddress ipAddress, int port, IClientEventConsumer clientEventConsumer, ILogger logger)
        {
            if (ipAddress == null)
                throw new InvalidParameterException("IPAddress is null");

            if (port <= 0 || port > 65535)
                throw new InvalidParameterException(string.Format("Port({0}) invalid", port));

            if (logger == null)
                throw new InvalidParameterException("Logger is null");

            IpAddress = ipAddress;
            Port = port;
            Logger = logger;
            ClientEventConsumer = clientEventConsumer;
        }

        public ILogger Logger { get; }
        public IPAddress IpAddress { get; }
        public int Port { get; }

        protected IClientEventConsumer ClientEventConsumer { get; }

        public abstract void Start();
        public abstract void Stop();
    }
}