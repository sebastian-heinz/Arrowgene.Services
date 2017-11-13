namespace Arrowgene.Services.Playground
{
    using Arrowgene.Services.Playground.Demo;
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            // new GetMacAddressDemo();
            // new HttpServerDemo();
            // new HttpTunneClientDemo();
            // new HttpTunnelServerDemo();
            // new PortScanDemo();
            // new ProxyDemo();
             new TcpConnectionDemo();
            // new UdpServerDemo();
            // new UdpClientDemo();

            Console.WriteLine("Press any key to exit..");
            Console.ReadKey();
        }
    }
}
