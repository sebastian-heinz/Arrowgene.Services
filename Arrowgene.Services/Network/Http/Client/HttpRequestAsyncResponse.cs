namespace Arrowgene.Services.Network.Http.Client
{
    using System;
    public class HttpRequestAsyncResponse : EventArgs
    {
        public HttpRequestAsyncResponse(string response)
        {
            this.Response = response;
        }
        public string Response { get; set; }
    }
}