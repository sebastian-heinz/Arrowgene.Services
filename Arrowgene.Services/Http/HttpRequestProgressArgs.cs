using System;

namespace Arrowgene.Services.Http
{
    public class HttpRequestProgressArgs : EventArgs
    {
        public HttpRequestProgressArgs(long current, long total)
        {
            Total = total;
            Current = current;
        }
        public long Total { get; private set; }
        public long Current { get; private set; }
    }
}