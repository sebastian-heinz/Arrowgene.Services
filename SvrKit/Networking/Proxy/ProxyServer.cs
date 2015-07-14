namespace SvrKit.Networking.Proxy
{
    using System;
    using System.Net.Sockets;
    using System.Threading;

    public class ProxyServer : ProxyBase
    {
        private ProxyClient proxyClient;
        private bool isListening;


        public ProxyServer(ProxyConfig proxyConfig)
            : base(proxyConfig)
        {
            this.proxyClient = new ProxyClient(proxyConfig);
        }

        public void Start()
        {
            Thread thread = new Thread(_Start);
            thread.Start();
        }

        private void _Start()
        {
            this.isListening = true;

            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(base.ProxyConfig.ProxyEndPoint);
                serverSocket.Listen(10);
                this.proxyHost.WriteOutput("Listening on port: " + base.ProxyConfig.ProxyPort.ToString());
                this.proxyHost.WriteOutput("Proxy Online!");
                while (this.isListening)
                {
                    if (serverSocket.Poll(100, SelectMode.SelectRead))
                    {
                        this.clientSocket = serverSocket.Accept();
                        this.proxyClient.Connect(this.clientSocket);

                        while(!proxyClient.isClientConnected)
                        Thread.Sleep(100);

                        this.isClientConnected = true;
                        this.proxyHost.WriteOutput("Client Connected to Proxy");

                        this.Listen();
                    }
                }
            }
            catch (Exception exception)
            {
                this.proxyHost.WriteOutput(exception.Message);
            }

        }

        public void Stop()
        {
            this.isListening = false;
        }

        protected override void ReceivedPacket(ProxyPacket proxyPacket)
        {
            proxyPacket.Traffic = ProxyPacket.TrafficType.CLIENT;
            this.proxyHost.WriteLog(proxyPacket);
            this.proxyClient.Send(proxyPacket.RawPacket);
        }

    }
}
