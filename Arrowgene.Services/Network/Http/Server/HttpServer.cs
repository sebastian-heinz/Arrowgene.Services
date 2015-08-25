/*
 *  Copyright 2015 Sebastian Heinz <sebastian.heinz.gt@googlemail.com>
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 */
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
