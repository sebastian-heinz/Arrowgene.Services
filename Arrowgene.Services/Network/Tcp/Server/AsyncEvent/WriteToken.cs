using System.Net.Sockets;

namespace Arrowgene.Services.Network.TCP.Server.AsyncEvent
{
    public class WriteToken
    {
        public byte[] Data
        {
            get => _data;
        }

        public Socket Socket
        {
            get => _socket;
        }

        public int TransferredCount
        {
            get => _transferredCount;
        }

        public int OutstandingCount
        {
            get => _outstandingCount;
        }

        private byte[] _data;
        private Socket _socket;
        private int _transferredCount;
        private int _outstandingCount;

        public WriteToken()
        {
        }

        public void Assign(Socket socket, byte[] data)
        {
            _socket = Socket;
            _data = data;
        }

        public void Update(int transferredCount)
        {
            _transferredCount += transferredCount;
            _outstandingCount = _data.Length - _transferredCount;
        }

        public void Reset()
        {
            _socket = null;
            _data = null;
        }
    }
}