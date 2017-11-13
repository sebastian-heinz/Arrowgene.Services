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
        private Thread _thread;
        private volatile bool _isRunning;

        private int m_numConnections;
        private int m_receiveBufferSize;
        BufferManager m_bufferManager;
        const int opsToPreAlloc = 2;

        Socket listenSocket;
        SocketAsyncEventArgsPool m_readWritePool;

        int m_totalBytesRead;
        int m_numConnectedSockets;
        Semaphore m_maxNumberAcceptedClients;

        public AsyncEventServer(IPAddress ipAddress, int port, ILogger logger) : base(ipAddress, port, logger)
        {
            int numConnections = 10;
            int receiveBufferSize = 10;

            m_totalBytesRead = 0;
            m_numConnectedSockets = 0;
            m_numConnections = numConnections;
            m_receiveBufferSize = receiveBufferSize;
            m_bufferManager = new BufferManager(receiveBufferSize * numConnections * opsToPreAlloc,
                receiveBufferSize);

            m_readWritePool = new SocketAsyncEventArgsPool(numConnections);
            m_maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);
            Init();
        }

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
            if (listenSocket != null)
            {
                listenSocket.Close();
            }
        }

        public void Run()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IpAddress, Port);
            listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(localEndPoint);
            listenSocket.Listen(100);
            StartAccept(null);
        }

        public void Init()
        {
            m_bufferManager.InitBuffer();
            SocketAsyncEventArgs readWriteEventArg;

            for (int i = 0; i < m_numConnections; i++)
            {
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                readWriteEventArg.UserToken = new AsyncEventClient();
                m_bufferManager.SetBuffer(readWriteEventArg);
                m_readWritePool.Push(readWriteEventArg);
            }
        }

        public void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                acceptEventArg.AcceptSocket = null;
            }

            m_maxNumberAcceptedClients.WaitOne();
            bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArg);
            }
        }

        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            Interlocked.Increment(ref m_numConnectedSockets);
            Console.WriteLine("Client connection accepted. There are {0} clients connected to the server",
                m_numConnectedSockets);

            SocketAsyncEventArgs readEventArgs = m_readWritePool.Pop();

            AsyncEventClient client = (AsyncEventClient) readEventArgs.UserToken;
            client.Accept(e.AcceptSocket);
            OnClientConnected(client);

            bool willRaiseEvent = e.AcceptSocket.ReceiveAsync(readEventArgs);
            if (!willRaiseEvent)
            {
                ProcessReceive(readEventArgs);
            }

            StartAccept(e);
        }

        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            AsyncEventClient client = (AsyncEventClient) e.UserToken;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                Interlocked.Add(ref m_totalBytesRead, e.BytesTransferred);
                Console.WriteLine("The server has read a total of {0} bytes", m_totalBytesRead);


                IBuffer buffer = new BBuffer();
                buffer.WriteBytes(e.Buffer, e.Offset, e.BytesTransferred);
                OnReceivedPacket(client, buffer);


                e.SetBuffer(e.Offset, e.BytesTransferred);
                bool willRaiseEvent = client.Socket.SendAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessSend(e);
                }
            }
            else
            {
                CloseClientSocket(e);
            }
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                AsyncEventClient token = (AsyncEventClient) e.UserToken;
                bool willRaiseEvent = token.Socket.ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                CloseClientSocket(e);
            }
        }

        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            AsyncEventClient client = e.UserToken as AsyncEventClient;
            try
            {
                client.Socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception)
            {
            }
            client.Socket.Close();
            Interlocked.Decrement(ref m_numConnectedSockets);
            m_maxNumberAcceptedClients.Release();
            OnClientDisconnected(client);
            Console.WriteLine("A client has been disconnected from the server. There are {0} clients connected to the server", m_numConnectedSockets);
            m_readWritePool.Push(e);
        }
    }
}