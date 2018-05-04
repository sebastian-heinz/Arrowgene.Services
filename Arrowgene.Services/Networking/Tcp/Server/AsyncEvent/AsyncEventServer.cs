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
        private Semaphore _maxNumberAcceptedClients;
        private Semaphore _maxNumberWriteOperations;
        private SocketAsyncEventArgs _acceptEventArg;
        private readonly Logger _logger;
        private readonly AsyncEventSettings _settings;

        public AsyncEventServer(IPAddress ipAddress, ushort port, IConsumer consumer, AsyncEventSettings settings)
            : base(ipAddress, port, consumer)
        {
            _settings = new AsyncEventSettings(settings);
            _logger = LogProvider<Logger>.GetLogger(this);
        }

        public AsyncEventServer(IPAddress ipAddress, ushort port, IConsumer consumer)
            : this(ipAddress, port, consumer, new AsyncEventSettings())
        {
        }

        public override void Send(ITcpSocket socket, byte[] data)
        {
            if (socket is AsyncEventClient client)
            {
                Send(client, data);
            }
        }

        public void Send(AsyncEventClient client, byte[] data)
        {
            if (!client.Socket.Connected)
            {
                return;
            }

            _maxNumberWriteOperations.WaitOne();
            SocketAsyncEventArgs writeEventArgs = _writePool.Pop();
            WriteToken token = (WriteToken) writeEventArgs.UserToken;
            token.Assign(client, data);
            StartSend(writeEventArgs);
        }

        protected override void OnStart()
        {
            _acceptEventArg = new SocketAsyncEventArgs();
            _acceptEventArg.Completed += Accept_Completed;
            _bufferManager = new BufferManager(_settings.BufferSize * _settings.MaxConnections + _settings.BufferSize * _settings.NumSimultaneouslyWriteOperations, _settings.BufferSize);
            _readPool = new Pool<SocketAsyncEventArgs>(_settings.MaxConnections);
            _writePool = new Pool<SocketAsyncEventArgs>(_settings.NumSimultaneouslyWriteOperations);
            _maxNumberAcceptedClients = new Semaphore(_settings.MaxConnections, _settings.MaxConnections);
            _maxNumberWriteOperations = new Semaphore(_settings.NumSimultaneouslyWriteOperations, _settings.NumSimultaneouslyWriteOperations);
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

            _thread = new Thread(Run);
            _thread.Name = ThreadName;
            _thread.IsBackground = true;
            _thread.Start();
        }

        protected override void OnStop()
        {
            if (_thread != null)
            {
                _logger.Info("Shutting {0} down...", ThreadName);
                if (Thread.CurrentThread != _thread)
                {
                    if (_thread.Join(1000))
                    {
                        _logger.Info("{0} ended.", ThreadName);
                    }
                    else
                    {
                        _logger.Error("Exceeded thread join timeout of {0}ms, could not close {1}.", 1000, ThreadName);
                    }
                }
                else
                {
                    _logger.Debug("Tried to join thread from within thread, letting thread run out..");
                }
            }

            if (_listenSocket != null)
            {
                _listenSocket.Close();
            }
        }

        private void Run()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IpAddress, Port);
            _listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _settings.SocketSettings.ConfigureSocket(_listenSocket, _logger);
            try
            {
                _listenSocket.Bind(localEndPoint);
                _listenSocket.Listen(_settings.SocketSettings.Backlog);
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
                if (exception is SocketException socketException && socketException.SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    _logger.Error("Address is already in use ({0}:{1}), try another IP/Port", IpAddress, Port);
                }

                _logger.Error("Stopping server due to error...");
                Stop();
                return;
            }

            StartAccept();
        }

        private void StartAccept()
        {
            _maxNumberAcceptedClients.WaitOne();
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
                AsyncEventClient client = new AsyncEventClient(acceptEventArg.AcceptSocket, this);
                readEventArgs.UserToken = client;
                OnClientConnected(client);
                StartReceive(readEventArgs);
                StartAccept();
            }
            else
            {
                switch (acceptEventArg.SocketError)
                {
                    case SocketError.OperationAborted:
                        // Server shut down.
                        break;
                    default:
                        _logger.Error(acceptEventArg.SocketError.ToString());
                        break;
                }
            }
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
                CloseClientSocket(client);
                ReleaseRead(readEventArgs);
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
                OnReceivedData(client, data);
                StartReceive(readEventArgs);
            }
            else
            {
                CloseClientSocket(client);
                ReleaseRead(readEventArgs);
            }
        }

        private void StartSend(SocketAsyncEventArgs writeEventArgs)
        {
            WriteToken token = (WriteToken) writeEventArgs.UserToken;
            if (token.OutstandingCount <= _settings.BufferSize)
            {
                writeEventArgs.SetBuffer(writeEventArgs.Offset, token.OutstandingCount);
                Buffer.BlockCopy(token.Data, token.TransferredCount, writeEventArgs.Buffer, writeEventArgs.Offset, token.OutstandingCount);
            }
            else
            {
                writeEventArgs.SetBuffer(writeEventArgs.Offset, _settings.BufferSize);
                Buffer.BlockCopy(token.Data, token.TransferredCount, writeEventArgs.Buffer, writeEventArgs.Offset, _settings.BufferSize);
            }

            bool willRaiseEvent;
            try
            {
                willRaiseEvent = token.Client.Socket.SendAsync(writeEventArgs);
            }
            catch (ObjectDisposedException)
            {
                CloseClientSocket(token.Client);
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
                CloseClientSocket(token.Client);
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

        private void ReleaseRead(SocketAsyncEventArgs readEventArgs)
        {
            _readPool.Push(readEventArgs);
            _maxNumberAcceptedClients.Release();
        }

        private void CloseClientSocket(AsyncEventClient client)
        {
            if (client.IsAlive)
            {
                client.Close();
                OnClientDisconnected(client);
            }
        }
    }
}