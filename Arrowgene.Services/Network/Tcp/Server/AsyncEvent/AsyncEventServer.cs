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

namespace Arrowgene.Services.Network.TCP.Server.AsyncEvent
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using Logging;
    using Tcp.Server;
    using Buffers;

    public class AsyncEventServer : TcpServer
    {
        private int _numConnections;
        private int _numSimultaneouslyWriteOperations;
        private int _bufferSize;
        private Thread _thread;
        private BufferManager _bufferManager;
        private Socket _listenSocket;
        private Pool<AsyncEventClient> _clientPool;
        private Pool<SocketAsyncEventArgs> _writePool;
        private Semaphore _maxNumberAcceptedClients;
        private Semaphore _maxNumberWriteOperations;
        private SocketAsyncEventArgs _acceptEventArg;

        public IBufferProvider BufferProvider { get; set; }

        public AsyncEventServer(IPAddress ipAddress, int port, ILogger logger) : base(ipAddress, port, logger)
        {
            BufferProvider = new BBuffer();
            _bufferSize = 10;
            _numConnections = 10;
            _numSimultaneouslyWriteOperations = 10;
        }

        public override void Start()
        {
            _acceptEventArg = new SocketAsyncEventArgs();
            _acceptEventArg.Completed += Accept_Completed;
            _bufferManager = new BufferManager(_bufferSize * _numConnections * _numSimultaneouslyWriteOperations, _bufferSize);
            _clientPool = new Pool<AsyncEventClient>(_numConnections);
            _writePool = new Pool<SocketAsyncEventArgs>(_numConnections);
            _maxNumberAcceptedClients = new Semaphore(_numConnections, _numConnections);
            _maxNumberWriteOperations = new Semaphore(_numSimultaneouslyWriteOperations, _numSimultaneouslyWriteOperations);
            _bufferManager.InitBuffer();
            for (int i = 0; i < _numConnections; i++)
            {
                AsyncEventClient client = new AsyncEventClient(this);
                client.ReadEventArg.Completed += Receive_Completed;
                _bufferManager.SetBuffer(client.ReadEventArg);
                _clientPool.Push(client);
            }
            for (int i = 0; i < _numSimultaneouslyWriteOperations; i++)
            {
                SocketAsyncEventArgs writeEventArg = new SocketAsyncEventArgs();
                writeEventArg.Completed += Send_Completed;
                writeEventArg.UserToken = new WriteToken();
                _bufferManager.SetBuffer(writeEventArg);
                _writePool.Push(writeEventArg);
            }
            _thread = new Thread(Run);
            _thread.Name = "AsyncEventServer";
            _thread.Start();
        }

        public override void Stop()
        {
            if (_thread != null)
            {
                Logger.Write("Shutting server thread down...", LogType.CLIENT);
                if (Thread.CurrentThread != _thread)
                {
                    if (_thread.Join(1000))
                    {
                        Logger.Write("server thread ended clean.", LogType.CLIENT);
                    }
                    else
                    {
                        Logger.Write(String.Format("Exceeded thread join timeout of {0}ms, aborting thread...", 1000), LogType.CLIENT);
                        _thread.Abort();
                    }
                }
                else
                {
                    Logger.Write("Tried to join thread from within thread, letting thread run out..", LogType.CLIENT);
                    _thread.Abort();
                }
            }
            if (_listenSocket != null)
            {
                _listenSocket.Close();
            }
        }

        public void SendData(AsyncEventClient client, byte[] data)
        {
            SocketAsyncEventArgs writeEventArgs = AcquireWrite();
            WriteToken token = (WriteToken) writeEventArgs.UserToken;
            token.Assign(client, data);
            StartSend(writeEventArgs);
        }

        private void Run()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IpAddress, Port);
            _listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(localEndPoint);
            _listenSocket.Listen(100);
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
                AsyncEventClient client = _clientPool.Pop();
                client.Accept(acceptEventArg.AcceptSocket);
                OnClientConnected(client);
                StartReceive(client.ReadEventArg);
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
            bool willRaiseEvent = client.Socket.ReceiveAsync(readEventArgs);
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
                IBuffer buffer = BufferProvider.Provide();
                buffer.WriteBytes(readEventArgs.Buffer, readEventArgs.Offset, readEventArgs.BytesTransferred);
                OnReceivedPacket(client, buffer);
                StartReceive(readEventArgs);
            }
            else
            {
                CloseClientSocket(client);
            }
        }

        private void StartSend(SocketAsyncEventArgs writeEventArgs)
        {
            WriteToken token = (WriteToken) writeEventArgs.UserToken;
            if (token.OutstandingCount <= _bufferSize)
            {
                writeEventArgs.SetBuffer(writeEventArgs.Offset, token.OutstandingCount);
                System.Buffer.BlockCopy(token.Data, token.TransferredCount, writeEventArgs.Buffer, writeEventArgs.Offset, token.OutstandingCount);
            }
            else
            {
                writeEventArgs.SetBuffer(writeEventArgs.Offset, _bufferSize);
                System.Buffer.BlockCopy(token.Data, token.TransferredCount, writeEventArgs.Buffer, writeEventArgs.Offset, _bufferSize);
            }
            bool willRaiseEvent = token.Client.Socket.SendAsync(writeEventArgs);
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
                    token.Reset();
                    ReleaseWrite(writeEventArgs);
                }
                else
                {
                    StartSend(writeEventArgs);
                }
            }
            else
            {
                token.Reset();
                CloseClientSocket(token.Client);
                ReleaseWrite(writeEventArgs);
            }
        }

        private void ReleaseWrite(SocketAsyncEventArgs writeEventArgs)
        {
            _writePool.Push(writeEventArgs);
            _maxNumberWriteOperations.Release();
        }

        private SocketAsyncEventArgs AcquireWrite()
        {
            _maxNumberWriteOperations.WaitOne();
            return _writePool.Pop();
        }

        private void CloseClientSocket(AsyncEventClient client)
        {
            client.Close();
            _clientPool.Push(client);
            _maxNumberAcceptedClients.Release();
            OnClientDisconnected(client);
        }
    }
}