namespace Arrowgene.Services.Network.Http.Server
{
    using System;
    using System.Net;

    public class HttpHandleEventArgs : EventArgs
    {
        public HttpHandleEventArgs(HttpListenerContext context)
        {
            this.Context = context;
        }

        public HttpListenerContext Context { get; private set; }
    }
}
