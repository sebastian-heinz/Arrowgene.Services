using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Arrowgene.Services.Logging;
using Arrowgene.Services.Networking;
using Arrowgene.Services.Networking.ServerBridge;
using Arrowgene.Services.Networking.ServerBridge.Messages;
using Arrowgene.Services.Networking.ServerBridge.Tcp;
using Xunit;

namespace Arrowgene.Services.Test.Bridge
{
    public class TcpBridgeTest
    {
        [Fact]
        public void TestDataExchange()
        {
            LogProvider.GlobalLogWrite += (sender, args) => { Debug.WriteLine(args.Log); };

            TcpBridgeSettings conf1 = GetBridgeConfiguration(0, 1);
            TcpBridgeSettings conf2 = GetBridgeConfiguration(1, 1);

            IBridge<IPEndPoint> bridge1 = new TcpBridge(conf1);
            IBridge<IPEndPoint> bridge2 = new TcpBridge(conf2);

            bridge2.AddHandler(new TestMessageHandler());

            bridge1.Start();
            bridge2.Start();

            bool wait = true;

            Request<TestRequest> request =
                new Request<TestRequest>(TestMessageHandler.Id, new TestRequest {RequestText = "REQUEST"});

            bool success = false;
            bridge1.Request<TestResponse>(conf2.PublicEndPoint.ToIpEndPoint(), request, response =>
            {
                success = true;
                Assert.Equal("RESPONSE", response.Result.ResponseText);
            });

            int cycles = 0;
            while (!success && wait)
            {
                Thread.Sleep(1000);
                cycles++;
                if (cycles > 10)
                {
                    wait = false;
                }
            }

            Assert.True(success);
        }

        private TcpBridgeSettings GetBridgeConfiguration(int num, int total)
        {
            List<NetworkPoint> allowedEndPoints = new List<NetworkPoint>();
            NetworkPoint me = null;

            for (int i = 0; i <= total; i++)
            {
                NetworkPoint endPoint = new NetworkPoint(IPAddress.Loopback, (ushort) (9250 + i));
                allowedEndPoints.Add(endPoint);
                if (i == num)
                {
                    me = endPoint;
                }
            }

            if (me == null)
            {
                throw new Exception("Invalid num provided");
            }

            TcpBridgeSettings sets = new TcpBridgeSettings(me, me, allowedEndPoints);
            sets.ServerSettings.BufferSize = 10;
            return sets;
        }
    }
}