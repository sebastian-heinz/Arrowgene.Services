namespace Arrowgene.Services.Network.Tcp.Server.AsyncEvent
{
    using System;
    using System.Net.Sockets;

    public class AsyncEventSocket : ITcpSocket
    {
        public Socket Socket { get; }
        private AsyncEventServer _server;

        public AsyncEventSocket(Socket socket, AsyncEventServer server)
        {
            Socket = socket;
            _server = server;
        }

        public void Send(byte[] data)
        {
            _server.SendData(this, data);
        }

        public void Close()
        {
            try
            {
                Socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception)
            {
            }
            Socket.Close();
        }
    }
}