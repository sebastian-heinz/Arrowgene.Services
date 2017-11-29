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
using System.Threading;
using Arrowgene.Services.Buffers;
using Arrowgene.Services.Exceptions;
using Arrowgene.Services.Logging;
using Arrowgene.Services.Networking.Tcp.Client.EventConsumer;

namespace Arrowgene.Services.Networking.Tcp.Client.SyncReceive
{
    public class SyncReceiveTcpClient : TcpClient
    {
        private const string DefaultName = "Tcp Client";

        private volatile bool _isConnected;
        private readonly int _pollTimeout;
        private readonly int _bufferSize;
        private readonly ILogger _logger;
        private Socket _socket;
        private Thread _readThread;

        public IBufferProvider BufferProvider { get; }
        public int SocketPollTimeout { get; }
        public int ThreadJoinTimeout { get; }
        public string Name { get; }
        public IPAddress RemoteIpAddress { get; private set; }
        public int Port { get; private set; }

        public SyncReceiveTcpClient(IClientEventConsumer eventConsumer) : base(eventConsumer)
        {
            BufferProvider = new ArrayBuffer();
            _logger = LogProvider.GetLogger(this);
            SocketPollTimeout = 100;
            Name = DefaultName;
            ThreadJoinTimeout = 1000;
            _pollTimeout = 10;
            _bufferSize = 1024;
        }

        public void Connect(string remoteIpAddress, int serverPort, TimeSpan timeout)
        {
            Connect(IPAddress.Parse(remoteIpAddress), serverPort, timeout);
        }

        public override void Connect(IPAddress remoteIpAddress, int serverPort, TimeSpan timeout)
        {
            if (!_isConnected)
            {
                if (remoteIpAddress == null || serverPort <= 0)
                {
                    throw new InvalidParameterException(string.Format("IpAddress({0}) or Port({1}) invalid", remoteIpAddress, serverPort));
                }
                RemoteIpAddress = remoteIpAddress;
                Port = serverPort;
                try
                {
                    Socket socket = CreateSocket();
                    if (socket != null)
                    {
                        if (timeout != TimeSpan.Zero)
                        {
                            IAsyncResult result = socket.BeginConnect(RemoteIpAddress, Port, null, null);
                            bool success = result.AsyncWaitHandle.WaitOne(timeout, true);
                            if (socket.Connected && success)
                            {
                                socket.EndConnect(result);
                                ConnectionEstablished(socket);
                            }
                            else
                            {
                                const string errTimeout = "Client connection timed out.";
                                _logger.Error(errTimeout);
                                socket.Close();
                                EventConsumer.OnConnectError(this, errTimeout, RemoteIpAddress, Port, timeout);
                            }
                        }
                        else
                        {
                            socket.Connect(RemoteIpAddress, Port);
                            ConnectionEstablished(socket);
                        }
                    }
                    else
                    {
                        const string errConnect = "Client could not connect.";
                        _logger.Error(errConnect);
                        EventConsumer.OnConnectError(this, errConnect, RemoteIpAddress, Port, timeout);
                    }
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                    EventConsumer.OnConnectError(this, exception.Message, RemoteIpAddress, Port, timeout);
                }
            }
            else
            {
                const string errConnected = "Client is already connected.";
                _logger.Error(errConnected);
                EventConsumer.OnConnectError(this, errConnected, RemoteIpAddress, Port, timeout);
            }
        }

        public override void Send(byte[] payload)
        {
            _socket.Send(payload);
        }

        public override void Disconnect()
        {
            _isConnected = false;
            if (_readThread != null)
            {
                _logger.Info("Shutting {0} down...", Name);
                if (Thread.CurrentThread != _readThread)
                {
                    if (_readThread.Join(ThreadJoinTimeout))
                    {
                        _logger.Info("{0} ended.", Name);
                    }
                    else
                    {
                        _logger.Error("Exceeded thread join timeout of {0}ms, could not close {1}.", 1000, Name);
                        _readThread.Abort();
                    }
                }
                else
                {
                    _logger.Debug("Tried to join thread from within thread, letting thread run out..");
                }
            }
            if (_socket != null)
            {
                _socket.Close();
            }
            EventConsumer.OnClientConnected(this);
        }

        private Socket CreateSocket()
        {
            Socket socket;
            _logger.Info("{0} Creating Socket...", Name);
            if (RemoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                _logger.Info("{0} Created Socket (IPv6)", Name);
            }
            else
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _logger.Info("{0} Created Socket (IPv4)", Name);
            }
            return socket;
        }

        private void ConnectionEstablished(Socket socket)
        {
            _socket = socket;
            _readThread = new Thread(ReadProcess);
            _readThread.Name = Name;
            _readThread.Start();
            _logger.Info("{0} connected", Name);
            EventConsumer.OnClientConnected(this);
        }

        private void ReadProcess()
        {
            _logger.Info("{0} started.", Name);
            _isConnected = true;
            while (_isConnected)
            {
                if (_socket.Poll(_pollTimeout, SelectMode.SelectRead))
                {
                    byte[] buffer = new byte[_bufferSize];
                    IBuffer payload = BufferProvider.Provide();
                    try
                    {
                        int bytesReceived;
                        while (_socket.Available > 0 && (bytesReceived = _socket.Receive(buffer, 0, _bufferSize, SocketFlags.None)) > 0)
                        {
                            payload.WriteBytes(buffer, 0, bytesReceived);
                        }
                    }
                    catch (Exception e)
                    {
                        if (!_socket.Connected)
                        {
                            _logger.Error("{0} {1}", Name, e.Message);
                        }
                        else
                        {
                            _logger.Exception(e);
                        }
                        Disconnect();
                    }
                    payload.SetPositionStart();
                    EventConsumer.OnReceivedPacket(this, payload.GetAllBytes());
                }
                Thread.Sleep(SocketPollTimeout);
            }
            _logger.Info("{0} ended.", Name);
        }
    }
}