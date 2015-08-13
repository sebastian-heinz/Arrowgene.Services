namespace Arrowgene.Services.Networking.Http.QuickHttp
{
    using System;
    using System.Net;

    public class QHttpHandleEventArgs : EventArgs
    {
        public QHttpHandleEventArgs(HttpListenerContext context)
        {
            this.Context = context;
        }

        public HttpListenerContext Context { get; private set; }
    }
}
