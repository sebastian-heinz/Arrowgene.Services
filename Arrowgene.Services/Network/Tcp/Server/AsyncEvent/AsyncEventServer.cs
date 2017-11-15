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
        private volatile bool _isRunning;
        private int _numConnections;
        private int _bufferSize;
        private int _numConnectedSockets;
        private Thread _thread;
        private BufferManager _bufferManager;
        private Socket _listenSocket;
        private Pool<AsyncEventClient> _readPool;
        private Pool<SocketAsyncEventArgs> _writePool;
        private Semaphore _maxNumberAcceptedClients;

        public IBufferProvider BufferProvider { get; set; }

        public AsyncEventServer(IPAddress ipAddress, int port, ILogger logger) : base(ipAddress, port, logger)
        {
            BufferProvider = new BBuffer();
            _bufferSize = 10;
            _numConnectedSockets = 0;
            _numConnections = 10;
            _bufferManager = new BufferManager(_bufferSize * _numConnections * 2, _bufferSize);
            _readPool = new Pool<AsyncEventClient>(_numConnections);
            _writePool = new Pool<SocketAsyncEventArgs>(_numConnections);
            _maxNumberAcceptedClients = new Semaphore(_numConnections, _numConnections);
            _bufferManager.InitBuffer();
            for (int i = 0; i < _numConnections; i++)
            {
                AsyncEventClient client = new AsyncEventClient();
                client.ReadEventArg.Completed += Receive_Completed;
                _bufferManager.SetBuffer(client.ReadEventArg);
                _readPool.Push(client);

                SocketAsyncEventArgs writeEventArg = new SocketAsyncEventArgs();
                writeEventArg.Completed += Send_Completed;
                writeEventArg.UserToken = new WriteToken();
                _bufferManager.SetBuffer(writeEventArg);
                _writePool.Push(writeEventArg);
            }
        }
        // https://codereview.stackexchange.com/questions/8361/socketasynceventargs-send-and-receive
        // check SocketError in process methods
        // check last OP? - uneccasry because hard defined
        // close client socket mngmt

        public override void Start()
        {
            _thread = new Thread(Run);
            _thread.Name = "AsyncEventServer";
            _thread.Start();
        }

        public override void Stop()
        {
            _isRunning = false;
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
            _listenSocket.Listen(100);
            StartAccept(null);
        }

        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += Accept_Completed;
            }
            else
            {
                acceptEventArg.AcceptSocket = null;
            }
            _maxNumberAcceptedClients.WaitOne();
            bool willRaiseEvent = _listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArg);
            }
        }

        private void Accept_Completed(object sender, SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg.SocketError != SocketError.OperationAborted)
            {
                Console.WriteLine(acceptEventArg.SocketError);
                // Server shut down.
            }
            if (acceptEventArg.SocketError != SocketError.Success)
            {
                Console.WriteLine(acceptEventArg.SocketError);
            }
            else
            {
                ProcessAccept(acceptEventArg);
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs acceptEventArg)
        {
            Interlocked.Increment(ref _numConnectedSockets);
            Console.WriteLine("Client connection accepted. There are {0} clients connected to the server", _numConnectedSockets);

            AsyncEventClient client = _readPool.Pop();
            client.Accept(acceptEventArg.AcceptSocket);
            OnClientConnected(client);

            StartReceive(client.ReadEventArg);
            StartAccept(acceptEventArg);
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
                    _writePool.Push(writeEventArgs);
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
                _writePool.Push(writeEventArgs);
            }
        }

        private void CloseClientSocket(AsyncEventClient client)
        {
            try
            {
                client.Socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception)
            {
            }
            client.Socket.Close();
            Interlocked.Decrement(ref _numConnectedSockets);
            _maxNumberAcceptedClients.Release();
            OnClientDisconnected(client);
            Console.WriteLine("A client has been disconnected from the server. There are {0} clients connected to the server", _numConnectedSockets);
            _readPool.Push(client);
        }
    }
}