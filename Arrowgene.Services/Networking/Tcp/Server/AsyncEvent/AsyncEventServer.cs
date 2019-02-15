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
using Arrowgene.Services.Logging;
using Arrowgene.Services.Networking.Tcp.Consumer;

namespace Arrowgene.Services.Networking.Tcp.Server.AsyncEvent
{
    public class AsyncEventServer : TcpServer
    {
        private const string ThreadName = "AsyncEventServer";

        private Thread _thread;
        private BufferManager _bufferManager;
        private Socket _listenSocket;
        private Pool<SocketAsyncEventArgs> _readPool;
        private Pool<SocketAsyncEventArgs> _writePool;
        private SemaphoreSlim _maxNumberAcceptedClients;
        private SemaphoreSlim _maxNumberWriteOperations;
        private SocketAsyncEventArgs _acceptEventArg;
        private readonly Logger _logger;
        private readonly AsyncEventSettings _settings;
        private readonly int[] _unitOfOrders;
        private volatile uint _acceptedConnections;
        private volatile uint _currentConnections;
        private volatile bool _isRunning;
        private readonly object _lock;
        private readonly string _identity;

        public AsyncEventServer(IPAddress ipAddress, ushort port, IConsumer consumer, AsyncEventSettings settings)
            : base(ipAddress, port, consumer)
        {
            _settings = new AsyncEventSettings(settings);
            _logger = LogProvider<Logger>.GetLogger(this);
            _isRunning = false;
            _acceptedConnections = 0;
            _currentConnections = 0;
            _lock = new object();
            _identity = "";
            _unitOfOrders = new int[settings.MaxUnitOfOrder];
            if (!string.IsNullOrEmpty(_settings.Identity))
            {
                _identity = $"[{_settings.Identity}] ";
            }
        }

        public AsyncEventServer(IPAddress ipAddress, ushort port, IConsumer consumer)
            : this(ipAddress, port, consumer, new AsyncEventSettings())
        {
        }

        public override void Send(ITcpSocket socket, byte[] data)
        {
            if (socket == null)
            {
                _logger.Error($"{_identity}called send with null-socket");
                return;
            }

            if (!(socket is AsyncEventClient client))
            {
                _logger.Error($"{_identity}called send with wrong instance");
                return;
            }

            Send(client, data);
        }

        public void Send(AsyncEventClient client, byte[] data)
        {
            if (!_isRunning)
            {
                _logger.Debug($"{_identity}Server stopped, not sending anymore.");
                return;
            }

            if (!client.Socket.Connected)
            {
                _logger.Error(
                    $"{_identity}AsyncEventClient not connected during send, closing socket. ({client.Identity})");
                client.Close();
                return;
            }

            _maxNumberWriteOperations.Wait();
            SocketAsyncEventArgs writeEventArgs = _writePool.Pop();
            WriteToken token = (WriteToken) writeEventArgs.UserToken;
            token.Assign(client, data);
            StartSend(writeEventArgs);
        }

        internal void NotifyDisconnected(AsyncEventClient client)
        {
            if (client.ReadEventArgs == null)
            {
                _logger.Error($"{_identity}Already returned AsyncEventArgs to poll ({client.Identity})");
                return;
            }

            FreeUnitOfOrder(client.UnitOfOrder);
            ReleaseAccept(client.ReadEventArgs);
            _currentConnections--;
            _logger.Debug($"{_identity}Free Accepted: {_maxNumberAcceptedClients.CurrentCount}");
            _logger.Debug($"{_identity}NotifyDisconnected::Current Connections: {_currentConnections}");
            try
            {
                OnClientDisconnected(client);
            }
            catch (Exception ex)
            {
                _logger.Error($"{_identity}Error during OnClientDisconnected() user code ({client.Identity})");
                _logger.Exception(ex);
            }
        }

        protected override void OnStart()
        {
            _acceptEventArg = new SocketAsyncEventArgs();
            _acceptEventArg.Completed += Accept_Completed;
            _bufferManager =
                new BufferManager(
                    _settings.BufferSize * _settings.MaxConnections +
                    _settings.BufferSize * _settings.NumSimultaneouslyWriteOperations, _settings.BufferSize);
            _readPool = new Pool<SocketAsyncEventArgs>(_settings.MaxConnections);
            _writePool = new Pool<SocketAsyncEventArgs>(_settings.NumSimultaneouslyWriteOperations);
            _maxNumberAcceptedClients = new SemaphoreSlim(_settings.MaxConnections, _settings.MaxConnections);
            _maxNumberWriteOperations = new SemaphoreSlim(_settings.NumSimultaneouslyWriteOperations,
                _settings.NumSimultaneouslyWriteOperations);
            _bufferManager.InitBuffer();
            for (int i = 0; i < _settings.MaxConnections; i++)
            {
                SocketAsyncEventArgs readEventArgs = new SocketAsyncEventArgs();
                readEventArgs.Completed += Receive_Completed;
                readEventArgs.UserToken = new WriteToken();
                _bufferManager.SetBuffer(readEventArgs);
                _readPool.Push(readEventArgs);
            }

            for (int i = 0; i < _settings.NumSimultaneouslyWriteOperations; i++)
            {
                SocketAsyncEventArgs writeEventArgs = new SocketAsyncEventArgs();
                writeEventArgs.Completed += Send_Completed;
                writeEventArgs.UserToken = new WriteToken();
                _bufferManager.SetBuffer(writeEventArgs);
                _writePool.Push(writeEventArgs);
            }

            lock (_lock)
            {
                for (int i = 0; i < _unitOfOrders.Length; i++)
                {
                    _unitOfOrders[i] = 0;
                }
            }

            _thread = new Thread(Run);
            _thread.Name = $"{_identity}{ThreadName}";
            _thread.IsBackground = true;
            _thread.Start();
        }

        protected override void OnStop()
        {
            _isRunning = false;
            if (_thread != null)
            {
                _logger.Info($"{_identity}Shutting {ThreadName} down...");
                if (Thread.CurrentThread != _thread)
                {
                    if (_thread.Join(1000))
                    {
                        _logger.Info($"{_identity}{ThreadName} ended.");
                    }
                    else
                    {
                        _logger.Error(
                            $"{_identity}Exceeded thread join timeout of {1000}ms, could not close {ThreadName}.");
                    }
                }
                else
                {
                    _logger.Debug($"{_identity}Tried to join thread from within thread, letting thread run out..");
                }
            }

            if (_listenSocket != null)
            {
                _listenSocket.Close();
            }

            try
            {
                OnStopped();
            }
            catch (Exception ex)
            {
                _logger.Error($"{_identity}Error during OnStopped() user code");
                _logger.Exception(ex);
            }
        }

        private int ClaimUnitOfOrder()
        {
            lock (_lock)
            {
                int minNumber = Int32.MaxValue;
                int unitOfOrder = 0;
                for (int i = 0; i < _unitOfOrders.Length; i++)
                {
                    if (_unitOfOrders[i] < minNumber)
                    {
                        minNumber = _unitOfOrders[i];
                        unitOfOrder = i;
                    }
                }

                _unitOfOrders[unitOfOrder]++;
                return unitOfOrder;
            }
        }

        private void FreeUnitOfOrder(int unitOfOrder)
        {
            lock (_lock)
            {
                _unitOfOrders[unitOfOrder]--;
            }
        }

        private void Run()
        {
            if (Startup())
            {
                _isRunning = true;
                try
                {
                    OnStarted();
                }
                catch (Exception ex)
                {
                    _logger.Error($"{_identity}Error during OnStarted() user code");
                    _logger.Exception(ex);
                }

                StartAccept();
            }
            else
            {
                _logger.Error($"{_identity}Stopping server due to error...");
                Stop();
            }
        }

        private bool Startup()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IpAddress, Port);
            _listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _settings.SocketSettings.ConfigureSocket(_listenSocket, _logger);
            bool success = false;
            int retries = 0;
            while (!success && _settings.Retries >= retries)
            {
                try
                {
                    _listenSocket.Bind(localEndPoint);
                    _listenSocket.Listen(_settings.SocketSettings.Backlog);
                    success = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                    if (exception is SocketException socketException &&
                        socketException.SocketErrorCode == SocketError.AddressAlreadyInUse)
                    {
                        _logger.Error(
                            $"{_identity}Address is already in use ({IpAddress}:{Port}), try another IP/Port");
                    }

                    _logger.Error($"{_identity}Retrying in 1 Minute");
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                }
            }

            _logger.Info($"{_identity}Startup Result: {success}");
            return success;
        }

        private void StartAccept()
        {
            if (!_isRunning)
            {
                _logger.Debug($"{_identity}Server stopped, not accepting new connections anymore.");
                return;
            }

            _maxNumberAcceptedClients.Wait();
            _acceptEventArg.AcceptSocket = null;
            bool willRaiseEvent = _listenSocket.AcceptAsync(_acceptEventArg);
            if (!willRaiseEvent)
            {
                ProcessAccept(_acceptEventArg);
            }
        }

        private void Accept_Completed(object sender, SocketAsyncEventArgs acceptEventArg)
        {
            ProcessAccept(acceptEventArg);
        }

        private void ProcessAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg.SocketError == SocketError.Success)
            {
                SocketAsyncEventArgs readEventArgs = _readPool.Pop();
                int unitOfOrder = ClaimUnitOfOrder();
                _logger.Debug($"{_identity}ProcessAccept::Claimed UnitOfOrder: {unitOfOrder}");
                AsyncEventClient client =
                    new AsyncEventClient(acceptEventArg.AcceptSocket, readEventArgs, this, unitOfOrder);
                readEventArgs.UserToken = client;
                _acceptedConnections++;
                _currentConnections++;
                _logger.Debug($"{_identity}ProcessAccept::Current Connections: {_currentConnections}");
                _logger.Debug($"{_identity}Accepted Connections: {_acceptedConnections}");
                try
                {
                    OnClientConnected(client);
                }
                catch (Exception ex)
                {
                    _logger.Error($"{_identity}Error during OnClientConnected() user code ({client.Identity})");
                    _logger.Exception(ex);
                }

                StartReceive(readEventArgs);
            }
            else
            {
                _logger.Error($"{_identity}Accept Socket Error: {acceptEventArg.SocketError}");
            }

            StartAccept();
        }

        private void StartReceive(SocketAsyncEventArgs readEventArgs)
        {
            AsyncEventClient client = (AsyncEventClient) readEventArgs.UserToken;
            bool willRaiseEvent;
            try
            {
                willRaiseEvent = client.Socket.ReceiveAsync(readEventArgs);
            }
            catch (ObjectDisposedException)
            {
                client.Close();
                return;
            }

            if (!willRaiseEvent)
            {
                ProcessReceive(readEventArgs);
            }
        }

        private void Receive_Completed(object sender, SocketAsyncEventArgs readEventArgs)
        {
            ProcessReceive(readEventArgs);
        }

        private void ProcessReceive(SocketAsyncEventArgs readEventArgs)
        {
            AsyncEventClient client = (AsyncEventClient) readEventArgs.UserToken;
            if (readEventArgs.BytesTransferred > 0 && readEventArgs.SocketError == SocketError.Success)
            {
                byte[] data = new byte[readEventArgs.BytesTransferred];
                Buffer.BlockCopy(readEventArgs.Buffer, readEventArgs.Offset, data, 0, readEventArgs.BytesTransferred);
                try
                {
                    OnReceivedData(client, data);
                }
                catch (Exception ex)
                {
                    _logger.Error($"{_identity}Error during OnReceivedData() user code ({client.Identity})");
                    _logger.Exception(ex);
                }

                StartReceive(readEventArgs);
            }
            else
            {
                client.Close();
            }
        }

        private void StartSend(SocketAsyncEventArgs writeEventArgs)
        {
            WriteToken token = (WriteToken) writeEventArgs.UserToken;
            if (token.OutstandingCount <= _settings.BufferSize)
            {
                writeEventArgs.SetBuffer(writeEventArgs.Offset, token.OutstandingCount);
                Buffer.BlockCopy(token.Data, token.TransferredCount, writeEventArgs.Buffer, writeEventArgs.Offset,
                    token.OutstandingCount);
            }
            else
            {
                writeEventArgs.SetBuffer(writeEventArgs.Offset, _settings.BufferSize);
                Buffer.BlockCopy(token.Data, token.TransferredCount, writeEventArgs.Buffer, writeEventArgs.Offset,
                    _settings.BufferSize);
            }

            bool willRaiseEvent;
            try
            {
                willRaiseEvent = token.Client.Socket.SendAsync(writeEventArgs);
            }
            catch (ObjectDisposedException)
            {
                token.Client.Close();
                ReleaseWrite(writeEventArgs);
                return;
            }

            if (!willRaiseEvent)
            {
                ProcessSend(writeEventArgs);
            }
        }

        private void Send_Completed(object sender, SocketAsyncEventArgs writeEventArgs)
        {
            ProcessSend(writeEventArgs);
        }

        private void ProcessSend(SocketAsyncEventArgs writeEventArgs)
        {
            WriteToken token = (WriteToken) writeEventArgs.UserToken;
            if (writeEventArgs.SocketError == SocketError.Success)
            {
                token.Update(writeEventArgs.BytesTransferred);
                if (token.OutstandingCount == 0)
                {
                    ReleaseWrite(writeEventArgs);
                }
                else
                {
                    StartSend(writeEventArgs);
                }
            }
            else
            {
                token.Client.Close();
                ReleaseWrite(writeEventArgs);
            }
        }

        private void ReleaseWrite(SocketAsyncEventArgs writeEventArgs)
        {
            WriteToken token = (WriteToken) writeEventArgs.UserToken;
            token.Reset();
            _writePool.Push(writeEventArgs);
            _maxNumberWriteOperations.Release();
        }

        private void ReleaseAccept(SocketAsyncEventArgs readEventArgs)
        {
            _readPool.Push(readEventArgs);
            _maxNumberAcceptedClients.Release();
        }
    }
}