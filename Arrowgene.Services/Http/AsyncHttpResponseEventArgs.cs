using System;

namespace Arrowgene.Services.Http
{
    public class AsyncHttpResponseEventArgs : EventArgs
    {
        public AsyncHttpResponseEventArgs(byte[] response)
        {
            Response = response;
        }
        public byte[] Response { get; private set; }
    }
}