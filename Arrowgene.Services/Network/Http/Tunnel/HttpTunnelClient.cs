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
    using Client;
    using Logging;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class HttpTunnelClient
    {
        private const int THREAD_CLOSE_TIMEOUT_MILLIS = 1000;
        private const int BACKLOG = 10;
        private const int POLL_TIMEOUT_MICROS = 1000;
        private const int BUFFER_SIZE = 1024;
        private const int CYCLE_IDLE_MILLIS = 100;

        private Thread lineUpClientTask;
        private bool isRunning;
        private IPEndPoint localIpEndPoint;
        private IPEndPoint tunnelServerIpEndPoint;
        private IPEndPoint destinationIpEndPoint;
        private string tunnelServerRootUrl;
        private Logger logger;

        public HttpTunnelClient()
        {
            this.isRunning = false;
        }

        public HttpTunnelClient(Logger logger) : this()
        {
            this.logger = logger;
        }

        public void Start(IPEndPoint localIpEndPoint, IPEndPoint tunnelServerIpEndPoint, IPEndPoint destinationIpEndPoint)
        {
            this.Stop();

            this.localIpEndPoint = localIpEndPoint;
            this.tunnelServerIpEndPoint = tunnelServerIpEndPoint;
            this.destinationIpEndPoint = destinationIpEndPoint;
            this.tunnelServerRootUrl = string.Format("http://{0}:{1}", this.tunnelServerIpEndPoint.Address, this.tunnelServerIpEndPoint.Port);

            this.isRunning = true;
            this.lineUpClientTask = new Thread(this.LineUpClientTask);
            this.lineUpClientTask.Name = "LineUpClientTask";
            this.lineUpClientTask.Start();
        }

        public void Stop()
        {
            if (this.isRunning)
            {
                this.isRunning = false;

                if (this.lineUpClientTask.Join(THREAD_CLOSE_TIMEOUT_MILLIS))
                {
                    this.Write("LineUpClient thread gracefully stopped");
                }
                else
                {
                    this.lineUpClientTask.Abort();
                    this.Write("LineUpClient thread aborted");
                }
            }
        }

        private void LineUpClientTask()
        {
            Socket localServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.Write("Created LocalServerSocket (IPv4)...");

            localServerSocket.Bind(this.localIpEndPoint);
            this.Write(string.Format("Bound LocalServerSocket to {0}", this.localIpEndPoint));

            localServerSocket.Listen(BACKLOG);
            this.Write("LocalServerSocket is listening");

            string connectionInitUrl = string.Format("{0}/{1}/{2}/{3}", this.tunnelServerRootUrl, "init", this.destinationIpEndPoint.Address, this.destinationIpEndPoint.Port);
            string incDataUrl = string.Format("{0}/{1}/", this.tunnelServerRootUrl, "incData");
            string outDataUrl = string.Format("{0}/{1}/", this.tunnelServerRootUrl, "outData");

            while (this.isRunning)
            {
                Socket localApplicationSocket = localServerSocket.Accept();
                this.Write("LocalApplicationSocket connection accepted");

                HttpRequest httpRequest = new HttpRequest();
                string connectionInitResponse = httpRequest.RequestContent(connectionInitUrl);

                if (connectionInitResponse == "accepted")
                {
                    bool isConnected = true;

                    while (isConnected)
                    {
                        byte[] lineServerResponse = null;

                        if (localApplicationSocket.Poll(POLL_TIMEOUT_MICROS, SelectMode.SelectRead))
                        {
                            byte[] response = null;
                            byte[] buffer = new byte[BUFFER_SIZE];
                            int read = 0;

                            while (localApplicationSocket.Poll(POLL_TIMEOUT_MICROS, SelectMode.SelectRead))
                            {
                                read = localApplicationSocket.Receive(buffer, 0, BUFFER_SIZE, SocketFlags.None);

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

                            httpRequest.Reset();
                            httpRequest.PostPayload = response;
                            httpRequest.Method = HttpRequest.POST_METHOD;
                            httpRequest.Request(incDataUrl);
                        }


                        httpRequest.Reset();
                        lineServerResponse = httpRequest.Request(outDataUrl);

                        if (lineServerResponse != null)
                        {
                            localApplicationSocket.Send(lineServerResponse);
                        }

                        Thread.Sleep(CYCLE_IDLE_MILLIS);
                    }
                }
                else
                {
                    if (localApplicationSocket.Connected)
                    {
                        try
                        {
                            localApplicationSocket.Shutdown(SocketShutdown.Both);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    localApplicationSocket.Close();

                    this.Write(string.Format("TunnelServer couldnt establish connection with the destination, got following response: {0}", connectionInitResponse));
                }
            }
        }

        private void Write(string msg)
        {
            if (this.logger != null)
            {
                this.logger.Write(msg);
            }
        }
    }
}