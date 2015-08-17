namespace Arrowgene.Services.Network.Http.Server
{

    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;

    /// <summary>
    /// Http Server needs admin priveleges
    /// </summary>
    public class HttpServer
    {
        private Thread serverThread;
        private HttpListener listener;
        private int port;

        /// <summary>
        /// 
        /// </summary>
        public HttpServer()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<HttpHandleEventArgs> HttpHandleEvent;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        public void Initialize(int port)
        {
            this.port = port;
            serverThread = new Thread(this.Listen);
            serverThread.Name = "QHttpServer";
            serverThread.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if (serverThread != null)
            {
                serverThread.Abort();
            }

            if (listener != null)
            {
                this.listener.Stop();
            }
        }

        private void Listen()
        {
            this.listener = new HttpListener();
            this.listener.Prefixes.Add("http://*:" + this.port.ToString() + "/");
            this.listener.Start();
            while (true)
            {
                try
                {
                    HttpListenerContext context = this.listener.GetContext();
                    this.OnHttpHandle(context);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("QHttpServer::Listen" + ex.ToString());
                }
            }
        }

        private void OnHttpHandle(HttpListenerContext context)
        {
            if (HttpHandleEvent != null)
            {
                HttpHandleEventArgs httpHandleEventArgs = new HttpHandleEventArgs(context);
                HttpHandleEvent(this, httpHandleEventArgs);
            }
        }

    }
}
