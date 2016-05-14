namespace Arrowgene.Services.Playground
{
    using Arrowgene.Services.Playground.Demo;
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            // new HttpServerDemo();
            // new PortScanDemo();
            // new ProxyDemo();
            // new UdpDemoServer();
            // new UdpDemoClient();
            // new TcpConnection();
            new ManagedTcpConnection();
            // new GetMacAddress();

            Console.WriteLine("Press any key to exit..");
            Console.ReadKey();
        }
    }
}
