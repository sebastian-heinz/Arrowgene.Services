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

namespace Arrowgene.Services.Network.Tcp.Client
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using Buffers;
    using Logging;
    using System.Threading;
    using Exceptions;

    public class TcpClient : ITcpClient
    {
        private const String TCP_CLIENT = "Tcp Client";

        private volatile bool _isConnected;
        private int _pollTimeout;
        private int _bufferSize;
        private ILogger _logger;
        private Socket _socket;
        private Thread _readThread;

        public IBufferProvider BufferProvider { get; set; }
        public int SocketPollTimeout { get; set; }
        public int ThreadJoinTimeout { get; set; }
        public string Name { get; set; }
        public IPAddress RemoteIpAddress { get; private set; }
        public int Port { get; private set; }

        public event EventHandler<DisconnectedEventArgs> Disconnected;
        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<ReceivedPacketEventArgs> ReceivedPacket;
        public event EventHandler<ConnectErrorEventArgs> ConnectError;


        public TcpClient()
        {
            BufferProvider = new BBuffer();
            _logger = LogProvider.GetLogger(this);
            SocketPollTimeout = 100;
            Name = TCP_CLIENT;
            ThreadJoinTimeout = 1000;
            _pollTimeout = 10;
            _bufferSize = 1024;
        }

        public void Connect(String remoteIpAddress, int serverPort, TimeSpan timeout)
        {
            Connect(IPAddress.Parse(remoteIpAddress), serverPort, timeout);
        }

        public void Connect(IPAddress remoteIpAddress, int serverPort, TimeSpan timeout)
        {
            if (!_isConnected)
            {
                if (remoteIpAddress == null || serverPort <= 0)
                {
                    throw new InvalidParameterException(String.Format("IpAddress({0}) or Port({1}) invalid", remoteIpAddress, serverPort));
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
                                String errTimeout = "Client connection timed out.";
                                _logger.Error(errTimeout);
                                socket.Close();
                                OnConnectError(errTimeout, RemoteIpAddress, Port, timeout);
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
                        String errConnect = "Client could not connect.";
                        _logger.Error(errConnect);
                        OnConnectError(errConnect, RemoteIpAddress, Port, timeout);
                    }
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                    OnConnectError(exception.Message, RemoteIpAddress, Port, timeout);
                }
            }
            else
            {
                String errConnected = "Client is already connected.";
                _logger.Error(errConnected);
                OnConnectError(errConnected, RemoteIpAddress, Port, timeout);
            }
        }

        public void Send(byte[] payload)
        {
            _socket.Send(payload);
        }

        public void Disconnect()
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
            OnDisconnected();
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
            OnConnected();
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
                    int bytesReceived;
                    IBuffer payload = BufferProvider.Provide();
                    try
                    {
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
                    OnClientReceivedPacket(payload);
                }
                Thread.Sleep(SocketPollTimeout);
            }
            _logger.Info("{0} ended.", Name);
        }

        protected virtual void OnClientReceivedPacket(IBuffer payload)
        {
            EventHandler<ReceivedPacketEventArgs> receivedPacket = ReceivedPacket;
            if (receivedPacket != null)
            {
                ReceivedPacketEventArgs receivedPacketEventArgs = new ReceivedPacketEventArgs(this, payload);
                receivedPacket(this, receivedPacketEventArgs);
            }
        }

        protected virtual void OnDisconnected()
        {
            EventHandler<DisconnectedEventArgs> clientDisconnected = Disconnected;
            if (clientDisconnected != null)
            {
                DisconnectedEventArgs clientDisconnectedEventArgs = new DisconnectedEventArgs(this);
                clientDisconnected(this, clientDisconnectedEventArgs);
            }
        }

        protected virtual void OnConnected()
        {
            EventHandler<ConnectedEventArgs> clientConnected = Connected;
            if (clientConnected != null)
            {
                ConnectedEventArgs clientConnectedEventArgs = new ConnectedEventArgs(this);
                clientConnected(this, clientConnectedEventArgs);
            }
        }

        protected virtual void OnConnectError(string reason, IPAddress serverIpAddress, int serverPort, TimeSpan timeout)
        {
            EventHandler<ConnectErrorEventArgs> connectError = ConnectError;
            if (connectError != null)
            {
                ConnectErrorEventArgs connectErrorEventArgs = new ConnectErrorEventArgs(reason, serverIpAddress, serverPort, timeout);
                connectError(this, connectErrorEventArgs);
            }
        }
    }
}