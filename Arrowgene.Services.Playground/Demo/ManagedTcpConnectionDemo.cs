namespace Arrowgene.Services.Playground.Demo
{
    using Network.TCP.Managed;
    using Common;
    using System;
    using System.Net;

    public class ManagedTCPConnectionDemo
    {

        public ManagedTCPConnectionDemo()
        {
            ManagedServer svr = new ManagedServer(IPAddress.Any, 2345);
            svr.Logger.LogWrite += Logger_LogWrite_Server;
            svr.ManagedConnected += Svr_ManagedConnected;
            svr.ManagedDisconnected += Svr_ManagedDisconnected;
            svr.ManagedReceivedPacket += Svr_ManagedReceivedPacket;
            svr.Start();

            ManagedClient cli = new ManagedClient();
            cli.Logger.LogWrite += Logger_LogWrite_Client;
            cli.ManagedConnected += Cli_ManagedConnected;
            cli.ManagedDisconnected += Cli_ManagedDisconnected;
            cli.ManagedReceivedPacket += Cli_ManagedReceivedPacket;
            cli.Connect("192.168.178.20", 2345);

            Console.WriteLine("Press any key to send.");
            Console.ReadKey();

            cli.Send(1, "hi");

            Console.WriteLine("Press any key to disconnect.");
            Console.ReadKey();

            cli.Disconnect();
            svr.Stop();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private void Cli_ManagedReceivedPacket(object sender, ManagedClientReceivedPacketEventArgs e)
        {
            string obj = e.Packet.GetObject<string>();
            Console.WriteLine(string.Format("Client: received packet: {0}", obj));

        }

        private void Svr_ManagedReceivedPacket(object sender, ManagedReceivedPacketEventArgs e)
        {
            string obj = e.Packet.GetObject<string>();
            Console.WriteLine(string.Format("Server: received packet: {0}", obj));
            e.ManagedClientSocket.Send(2, "Yo");
        }

        private void Logger_LogWrite_Client(object sender, Logging.LogWriteEventArgs e)
        {
            Console.WriteLine(string.Format("Client Log: {0}", e.Log.Text));
        }

        private void Logger_LogWrite_Server(object sender, Logging.LogWriteEventArgs e)
        {
            Console.WriteLine(string.Format("Server Log: {0}", e.Log.Text));
        }

        private void Cli_ManagedDisconnected(object sender, ManagedClientDisconnectedEventArgs e)
        {
            Console.WriteLine("Client Disconnected");
        }

        private void Cli_ManagedConnected(object sender, ManagedClientConnectedEventArgs e)
        {
            Console.WriteLine("Client Connected");
        }

        private void Svr_ManagedDisconnected(object sender, Network.TCP.Managed.ManagedDisconnectedEventArgs e)
        {
            Console.WriteLine(string.Format("Server: Client Disconnected ({0})", e.ManagedClientSocket.Id));
        }

        private void Svr_ManagedConnected(object sender, Network.TCP.Managed.ManagedConnectedEventArgs e)
        {
            Console.WriteLine(string.Format("Server: Client Connected ({0})", e.ManagedClientSocket.Id));
        }
    }
}
