/*
 * MIT License
 * 
 * Copyright (c) 2018 Sebastian Heinz <sebastian.heinz.gt@googlemail.com>
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
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