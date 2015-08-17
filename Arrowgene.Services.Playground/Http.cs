namespace Arrowgene.Services.Playground
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Arrowgene.Services.Network.Http.QuickHttp;

    public class Http
    {

        QHttpServer server;

        public Http()
        {
            server = new QHttpServer();
            server.HttpHandleEvent += Server_HttpHandleEvent;
            server.Initialize(8888);

        }

        private void Server_HttpHandleEvent(object sender, QHttpHandleEventArgs e)
        {
      
        }
    }
}
