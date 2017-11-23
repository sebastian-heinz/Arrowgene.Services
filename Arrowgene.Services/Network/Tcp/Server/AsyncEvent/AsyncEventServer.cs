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
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using Logging;
    using Server;
    using EventConsumer;

    public class AsyncEventServer : TcpServer
    {
        private const string ThreadName = "AsyncEventServer";

        private int _maxConnections;
        private int _numSimultaneouslyWriteOperations;
        private int _bufferSize;
        private int _backlog;
        private Thread _thread;
        private BufferManager _bufferManager;
        private Socket _listenSocket;
        private Pool<SocketAsyncEventArgs> _readPool;
        private Pool<SocketAsyncEventArgs> _writePool;
        private Semaphore _maxNumberAcceptedClients;
        private Semaphore _maxNumberWriteOperations;
        private SocketAsyncEventArgs _acceptEventArg;
        private ILogger _logger;


        public AsyncEventServer(IPAddress ipAddress, int port, IClientEventConsumer clientEventConsumer, ILogger logger)
            : base(ipAddress, port, clientEventConsumer, logger)
        {
            _logger = LogProvider.GetLogger(this);
            Configure(new AsyncEventSettings());
        }

        public void Configure(AsyncEventSettings settings)
        {
            _bufferSize = settings.BufferSize;
            _maxConnections = settings.MaxConnections;
            _numSimultaneouslyWriteOperations = settings.NumSimultaneouslyWriteOperations;
            _backlog = settings.Backlog;
        }

        public override void Start()
        {
            ClientEventConsumer.OnStart();
            _acceptEventArg = new SocketAsyncEventArgs();
            _acceptEventArg.Completed += Accept_Completed;
            _bufferManager = new BufferManager((_bufferSize * _maxConnections) + (_bufferSize * _numSimultaneouslyWriteOperations), _bufferSize);
            _readPool = new Pool<SocketAsyncEventArgs>(_maxConnections);
            _writePool = new Pool<SocketAsyncEventArgs>(_numSimultaneouslyWriteOperations);
            _maxNumberAcceptedClients = new Semaphore(_maxConnections, _maxConnections);
            _maxNumberWriteOperations = new Semaphore(_numSimultaneouslyWriteOperations, _numSimultaneouslyWriteOperations);
            _bufferManager.InitBuffer();
            for (int i = 0; i < _maxConnections; i++)
            {
                SocketAsyncEventArgs readEventArgs = new SocketAsyncEventArgs();
                readEventArgs.Completed += Receive_Completed;
                readEventArgs.UserToken = new WriteToken();
                _bufferManager.SetBuffer(readEventArgs);
                _readPool.Push(readEventArgs);
            }
            for (int i = 0; i < _numSimultaneouslyWriteOperations; i++)
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

        public override void Stop()
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
            ClientEventConsumer.OnStop();
        }

        public void SendData(AsyncEventClient client, byte[] data)
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

        private void Run()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IpAddress, Port);
            _listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(localEndPoint);
            _listenSocket.Listen(_backlog);
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
                ClientEventConsumer.OnClientConnected(client);
                StartReceive(readEventArgs);
                StartAccept();
            }
            else
            {
                if (acceptEventArg.SocketError == SocketError.OperationAborted)
                {
                    // Server shut down.
                }
                Console.WriteLine(acceptEventArg.SocketError);
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
                ClientEventConsumer.OnReceivedPacket(client, data);
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
            if (token.OutstandingCount <= _bufferSize)
            {
                writeEventArgs.SetBuffer(writeEventArgs.Offset, token.OutstandingCount);
                Buffer.BlockCopy(token.Data, token.TransferredCount, writeEventArgs.Buffer, writeEventArgs.Offset, token.OutstandingCount);
            }
            else
            {
                writeEventArgs.SetBuffer(writeEventArgs.Offset, _bufferSize);
                Buffer.BlockCopy(token.Data, token.TransferredCount, writeEventArgs.Buffer, writeEventArgs.Offset, _bufferSize);
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
                ClientEventConsumer.OnClientDisconnected(client);
            }
        }
    }
}