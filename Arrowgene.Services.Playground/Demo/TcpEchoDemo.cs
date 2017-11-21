namespace Arrowgene.Services.Playground.Demo
{
    using System;
    using System.Net;
    using System.Threading;
    using Logging;
    using Network.Tcp.Server;
    using Network.Tcp.Server.AsyncEvent;
    using Network.Tcp.Server.EventConsumer.BlockingQueue;

    public class TcpEchoDemo
    {
        private volatile bool _isRunning;
        private BlockingQueueEventConsumer _consumer;
        private Thread _consumerThread;
        private ITcpServer _server;

        public TcpEchoDemo()
        {
            Start();
            Console.WriteLine("Echo Demo: Press any key to exit...");
            Console.ReadKey();
            Stop();
        }

        public void Start()
        {
            _consumer = new BlockingQueueEventConsumer();
            _server = new AsyncEventServer(IPAddress.Any, 2345, _consumer, new Logger("ServerLogger"));
            _server.Logger.LogWrite += Logger_LogWrite_Server;
            _server.Start();

            _consumerThread = new Thread(HandleEvents);
            _consumerThread.Name = "ConsumerThread";
            _consumerThread.Start();
        }

        public void Stop()
        {
            _server.Stop();
            _isRunning = false;
            if (_consumerThread != null && Thread.CurrentThread != _consumerThread && !_consumerThread.Join(1000))
            {
                _consumerThread.Interrupt();
            }
        }

        private void HandleEvents()
        {
            _isRunning = true;
            ClientEvent clientEvent = null;
            while (_isRunning)
            {
                Console.WriteLine(string.Format("Client Events: {0}", _consumer.ClientEvents.Count));
                try
                {
                    clientEvent = _consumer.ClientEvents.Take();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    _isRunning = false;
                }
                if (clientEvent != null)
                {
                    switch (clientEvent.ClientEventType)
                    {
                        case ClientEventType.Connected:
                            Console.WriteLine(string.Format("Demo: Server: Client Connected ({0})", clientEvent.Socket));
                            break;
                        case ClientEventType.Disconnected:
                            Console.WriteLine(string.Format("Demo: Server: Client Disconnected ({0})", clientEvent.Socket));
                            break;
                        case ClientEventType.ReceivedData:
                            byte[] data = clientEvent.Data;
                            Console.WriteLine(string.Format("Demo: Server: received packet Size:{0}", data.Length));
                            clientEvent.Socket.Send(data);
                            break;
                    }
                }
            }
            Console.WriteLine("Handler exited");
        }

        private void Logger_LogWrite_Server(object sender, LogWriteEventArgs e)
        {
            Console.WriteLine(string.Format("Server Log: {0}", e.Log.Text));
        }
    }
}