namespace Arrowgene.Services.Network.Http.QuickHttp
{

    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;

    /// <summary>
    /// 
    /// </summary>
    public class QHttpServer
    {
        private Thread serverThread;
        private HttpListener listener;
        private int port;

        /// <summary>
        /// 
        /// </summary>
        public QHttpServer()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<QHttpHandleEventArgs> HttpHandleEvent;

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
                QHttpHandleEventArgs httpHandleEventArgs = new QHttpHandleEventArgs(context);
                HttpHandleEvent(this, httpHandleEventArgs);
            }
        }

    }
}
