namespace Arrowgene.Services.Playground.Demo
{
    using Arrowgene.Services.Network.Http.Server;

    public class HttpServerDemo
    {
        HttpServer server;

        public HttpServerDemo()
        {
            server = new HttpServer();
            server.HttpHandleEvent += Server_HttpHandleEvent;
            server.Initialize(8888);
        }

        private void Server_HttpHandleEvent(object sender, HttpHandleEventArgs e)
        {
      
        }
    }
}
