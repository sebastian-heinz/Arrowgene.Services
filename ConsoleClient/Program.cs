namespace ConsoleClient
{
    using MarrySocket.MExtra;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public static class Program
    {
        private const string SERVER_HOST = "localhost";
        private const int SERVER_PORT = 2345;
        private const int RECONNECT_TIMEOUT_MS = 1000;

        static void Main(string[] args)
        {
            Client client = new Client();
            while (true)
            {
                if (!client.IsConnected)
                {
                    IPAddress ipAdress = Maid.IPAddressLookup(SERVER_HOST, AddressFamily.InterNetworkV6);

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
