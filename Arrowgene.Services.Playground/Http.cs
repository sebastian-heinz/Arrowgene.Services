namespace Arrowgene.Services.Playground
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Arrowgene.Services.Network.Http.Server;

    public class Http
    {

        HttpServer server;

        public Http()
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
