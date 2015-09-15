﻿namespace Arrowgene.Services.Test.Network.MarrySocket
{
    using Arrowgene.Services.Network;
    using Arrowgene.Services.Network.MarrySocket.MClient;
    using Arrowgene.Services.Network.MarrySocket.MServer;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    [TestClass]
    public class MarrySocketConnection
    {
        private MarryServer server;
        private MarryClient client;


        [TestInitialize]
        public void Initialize()
        {
            this.server = new MarryServer(new ServerConfig(IPAddress.IPv6Any, 2345));
            this.client = new MarryClient(new ClientConfig(IP.AddressLocalhost(AddressFamily.InterNetworkV6), 2345));
        }


        [TestMethod]
        public void Connectivity()
        {
            this.Connect();

            Assert.IsTrue(server.IsListening);
            Assert.IsTrue(client.IsConnected);

            this.Disconnect();

            Assert.IsFalse(server.IsListening);
            Assert.IsFalse(client.IsConnected);
        }

        [TestMethod]
        public void Sending()
        {
            int myId = 1000;
            string myMsg = "hello";
            bool wait = true;
            string clientReceived = null;
            string serverReceived = null;

            this.server.ReceivedPacket += (sender, e) =>
            {
                serverReceived = e.MyObject as string;
                e.ClientSocket.SendObject(myId, myMsg);
            };

            this.client.ReceivedPacket += (sender, e) =>
            {
                clientReceived = e.MyObject as string;
                wait = false;
            };

            this.Connect();
            this.client.ServerSocket.SendObject(myId, myMsg);

            while (wait)
            {
                Thread.Sleep(10);
            }

            Assert.IsTrue(clientReceived == myMsg);
            Assert.IsTrue(serverReceived == myMsg);
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.Disconnect();
        }

        private void Connect()
        {
            this.server.Start();
            this.client.Connect();
        }

        private void Disconnect()
        {
            this.client.Disconnect();
            this.server.Stop();
        }

    }
}
