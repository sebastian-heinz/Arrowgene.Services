using System.Net.Sockets;

namespace Arrowgene.Services.Network.TCP.Server.AsyncEvent
{
    public class WriteToken
    {
        public byte[] Data
        {
            get => _data;
        }

        public AsyncEventClient Client
        {
            get => _client;
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
        private AsyncEventClient _client;
        private int _transferredCount;
        private int _outstandingCount;

        public WriteToken()
        {
        }

        public void Assign(AsyncEventClient client, byte[] data)
        {
            _client = client;
            _data = data;
            _outstandingCount = data.Length;
            _transferredCount = 0;
        }

        public void Update(int transferredCount)
        {
            _transferredCount += transferredCount;
            _outstandingCount -= transferredCount;
        }

        public void Reset()
        {
            _client = null;
            _data = null;
            _outstandingCount = 0;
            _transferredCount = 0;
        }
    }
}