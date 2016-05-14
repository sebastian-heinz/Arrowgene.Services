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
namespace Arrowgene.Services.Network.Http.Tunnel
{
    using Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class HttpTunnelServer
    {
        private const int THREAD_CLOSE_TIMEOUT_MILLIS = 1000;
        private const int BUFFER_SIZE = 1024;
        private const int POLL_TIMEOUT_MICROS = 1000;
        private const int CYCLE_IDLE_MILLIS = 100;

        private Thread lineUpServerTask;
        private HttpListener httpServer;
        private IPEndPoint localIpEndPoint;
        private bool isRunning;
        private bool isReadingPayload;
        private Socket destinationSocket;
        private Thread readPayload;
        private List<byte[]> payloadQueue;
        private object padLock;
        private Logger logger;

        public HttpTunnelServer()
        {
            this.payloadQueue = new List<byte[]>();
            this.padLock = new object();
            this.isRunning = false;
            this.isReadingPayload = false;
        }

        public HttpTunnelServer(Logger logger) : this()
        {
            this.logger = logger;
        }

        public void Start(IPEndPoint localIpEndPoint)
        {
            this.Stop();

            this.localIpEndPoint = localIpEndPoint;

            this.isRunning = true;
            this.lineUpServerTask = new Thread(this.LineUpServerTask);
            this.lineUpServerTask.Name = "LineUpServerTask";
            this.lineUpServerTask.Start();
        }

        public void Stop()
        {
            this.StopReadPayload();

            if (this.httpServer != null)
            {
                this.httpServer.Abort();
            }

            if (this.isRunning)
            {
                this.isRunning = false;

                if (this.lineUpServerTask.Join(THREAD_CLOSE_TIMEOUT_MILLIS))
                {
                    this.Write("LineUpServer thread gracefully stopped");
                }
                else
                {
                    this.lineUpServerTask.Abort();
                    this.Write("LineUpServer thread aborted");
                }
            }

        }

        private void LineUpServerTask()
        {
            string httpPrefix = string.Format("http://{0}:{1}/", "*", this.localIpEndPoint.Port);

            this.httpServer = new HttpListener();
            this.httpServer.Prefixes.Add(httpPrefix);
            this.httpServer.Start();

            while (this.isRunning)
            {
                HttpListenerContext context = this.httpServer.GetContext();

                string[] commands = context.Request.Url.Segments;

                if (commands.Length > 1)
                {
                    string cmd = this.RemoveLastSlash(commands[1]);

                    if (cmd == "init")
                    {
                        if (commands.Length > 3)
                        {
                            string ip = this.RemoveLastSlash(commands[2]);
                            string port = this.RemoveLastSlash(commands[3]);

                            IPAddress destinationIPAddress = null;
                            destinationIPAddress = IPAddress.Parse(ip);

                            int destinationPort = -1;
                            int.TryParse(port, out destinationPort);

                            if (destinationPort > 0
                                && destinationIPAddress != null)
                            {
                                IPEndPoint destinationIpEndPoint = new IPEndPoint(destinationIPAddress, destinationPort);
                                this.StartReadPayload(destinationIpEndPoint);
                                this.SendHttpResponse("accepted", context);
                            }
                        }
                    }
                    else if (cmd == "incData")
                    {
                        byte[] payload = this.ReadHttpResponse(context);

                        if (payload != null)
                        {
                            this.destinationSocket.Send(payload);
                        }
                    }
                    else if (cmd == "outData")
                    {
                        List<byte[]> payloads = new List<byte[]>();

                        lock (this.padLock)
                        {
                            foreach (byte[] payload in this.payloadQueue)
                            {
                                payloads.Add(payload);
                            }
                            this.payloadQueue.Clear();
                        }

                        foreach (byte[] payload in payloads)
                        {
                            this.SendHttpResponse(new MemoryStream(payload), context);
                        }
                    }
                }
            }
        }

        private void StartReadPayload(IPEndPoint destinationIpEndPoint)
        {
            this.StopReadPayload();

            this.destinationSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.destinationSocket.Connect(destinationIpEndPoint);

            this.isReadingPayload = true;
            this.readPayload = new Thread(this.ReadPayload);
            this.readPayload.Name = "ReadPayload";
            this.readPayload.Start();
        }

        private void StopReadPayload()
        {
            if (this.isReadingPayload)
            {
                this.isReadingPayload = false;

                if (this.readPayload.Join(THREAD_CLOSE_TIMEOUT_MILLIS))
                {
                    this.Write("ReadPayload thread gracefully stopped");
                }
                else
                {
                    this.readPayload.Abort();
                    this.Write("ReadPayload thread aborted");
                }
            }
        }

        private void ReadPayload()
        {
            while (this.isReadingPayload)
            {
                if (this.destinationSocket.Poll(POLL_TIMEOUT_MICROS, SelectMode.SelectRead))
                {
                    byte[] response = null;
                    byte[] buffer = new byte[BUFFER_SIZE];
                    int read = 0;


                    while (this.destinationSocket.Poll(POLL_TIMEOUT_MICROS, SelectMode.SelectRead))
                    {
                        read = this.destinationSocket.Receive(buffer, 0, BUFFER_SIZE, SocketFlags.None);

                        if (response == null)
                        {
                            response = new byte[read];
                            Buffer.BlockCopy(buffer, 0, response, 0, read);
                        }
                        else
                        {
                            int newSize = response.Length + read;
                            byte[] new_response = new byte[newSize];

                            Buffer.BlockCopy(response, 0, new_response, 0, response.Length);
                            Buffer.BlockCopy(buffer, 0, new_response, response.Length, read);

                            response = new_response;
                        }
                    }

                    lock (this.padLock)
                    {
                        this.payloadQueue.Add(response);
                    }
                }

                Thread.Sleep(CYCLE_IDLE_MILLIS);
            }
        }

        private void SendHttpResponse(string input, HttpListenerContext context)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(input);
            writer.Flush();
            stream.Position = 0;
            this.SendHttpResponse(stream, context);
        }

        private void SendHttpResponse(Stream input, HttpListenerContext context)
        {
            try
            {
                context.Response.ContentType = "text/xml";
                context.Response.ContentLength64 = input.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));

                byte[] buffer = new byte[1024 * 16];
                int nbytes;
                while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    context.Response.OutputStream.Write(buffer, 0, nbytes);
                }
                input.Close();
                context.Response.OutputStream.Flush();
                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                this.Write("LineUpServer::SendResponse" + ex.ToString());
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            context.Response.OutputStream.Close();
        }

        private byte[] ReadHttpResponse(HttpListenerContext context)
        {
            byte[] response = null;
            Stream responseStream = context.Request.InputStream;

            byte[] buffer = new byte[BUFFER_SIZE];
            int read = 0;
            using (responseStream)
            {
                while ((read = responseStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    if (response == null)
                    {
                        response = new byte[read];
                        Buffer.BlockCopy(buffer, 0, response, 0, read);
                    }
                    else
                    {
                        int newSize = response.Length + read;
                        byte[] new_response = new byte[newSize];

                        Buffer.BlockCopy(response, 0, new_response, 0, response.Length);
                        Buffer.BlockCopy(buffer, 0, new_response, response.Length, read);

                        response = new_response;
                    }
                }
            }
            return response;
        }

        private void Write(string msg)
        {
            if (this.logger != null)
            {
                this.logger.Write(msg);
            }
        }

        private string RemoveLastSlash(string text)
        {
            if (text.Length > 0)
            {
                char textChar = text[text.Length - 1];
                if (textChar == '/')
                {
                    text = text.Substring(0, text.Length - 1);
                }
            }

            return text;
        }

    }
}