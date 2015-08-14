namespace Arrowgene.Services.Network.Http.QuickHttp
{
    using System;
    public class QHttpRequestAsyncResponse : EventArgs
    {
        public QHttpRequestAsyncResponse(string response)
        {
            this.Response = response;
        }
        public string Response { get; set; }
    }
}