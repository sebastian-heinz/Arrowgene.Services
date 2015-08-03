﻿namespace ArrowgeneServices.Networking.Proxy
{
    using ArrowgeneServices.Logging;
    using ArrowgeneServices.Networking.MarrySocket.MBase;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class ProxyServer : ProxyBase
    {
        private AGSocket serverSocket;
        private ProxyClient proxyClient;
        private bool isListening;
        private Logger logger;


        public ProxyServer(ProxyConfig proxyConfig)
            : base(proxyConfig)
        {
            this.logger = new Logger();
            this.proxyClient = new ProxyClient(proxyConfig);
            this.proxyClient.ReceivedPacket += proxyClient_ReceivedPacket;
        }

        public bool IsListening { get { return this.isListening; } }

        public override void Start()
        {
            Thread thread = new Thread(_Start);
            thread.Name = "ProxyServer";
            thread.Start();
        }

        public override void Stop()
        {
            this.isListening = false;
            base.IsConnected = false;
        }

        protected override void ReceivePacket(ProxyPacket proxyPacket)
        {
            byte[] forward = proxyPacket.Payload.GetBytes();
            this.proxyClient.Write(forward);

            proxyPacket.Traffic = ProxyPacket.TrafficType.SERVER;
            base.ReceivePacket(proxyPacket);
        }

        private void proxyClient_ReceivedPacket(object sender, ReceivedProxyPacketEventArgs e)
        {
            byte[] forward = e.ProxyPacket.Payload.GetBytes();
            base.Write(forward);
        }

        private void _Start()
        {
            try
            {
                this.serverSocket = new AGSocket();

                if (this.serverSocket != null)
                {
                    this.serverSocket.Bind(this.ProxyConfig.ProxyEndPoint);
                    this.serverSocket.Listen(10);

                    this.isListening = true;

                    while (this.isListening)
                    {
                        if (this.serverSocket.Poll(100, SelectMode.SelectRead))
                        {
                            base.socket = this.serverSocket.Accept();
                            this.IsConnected = true;

                            this.proxyClient.Start();

                            while (!proxyClient.IsConnected)
                            {
                                Thread.Sleep(100);
                            }

                            base.Read();
                        }
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }



    }
}