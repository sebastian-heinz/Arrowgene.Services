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
            svr.ClientConnected += Svr_ClientConnected;
            svr.ClientDisconnected += Svr_ClientDisconnected;
            svr.ServerReceivedManagedPacket += Svr_ServerReceivedManagedPacket;
            svr.Start();

            ManagedClient cli = new ManagedClient();
            cli.Logger.LogWrite += Logger_LogWrite_Client;
            cli.Connected += Cli_Connected;
            cli.Disconnected += Cli_Disconnected;
            cli.ClientReceivedManagedPacket += Cli_ClientReceivedManagedPacket;
            cli.Connect(IPAddress.Parse("192.168.178.20"), 2345);

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

        private void Cli_ClientReceivedManagedPacket(object sender, ClientReceivedManagedPacketEventArgs e)
        {
            string obj = e.Packet.GetObject<string>();
            Console.WriteLine(string.Format("Client: received packet: {0}", obj));

        }

        private void Svr_ServerReceivedManagedPacket(object sender, ServerReceivedManagedPacketEventArgs e)
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

        private void Cli_Disconnected(object sender, Network.TCP.Event.DisconnectedEventArgs e)
        {
            Console.WriteLine("Client Disconnected");
        }

        private void Cli_Connected(object sender, Network.TCP.Event.ConnectedEventArgs e)
        {
            Console.WriteLine("Client Connected");
        }

        private void Svr_ClientDisconnected(object sender, Network.TCP.Event.DisconnectedEventArgs e)
        {
            Console.WriteLine(string.Format("Server: Client Disconnected ({0})", e.ClientSocket.Id));
        }

        private void Svr_ClientConnected(object sender, Network.TCP.Event.ConnectedEventArgs e)
        {
            Console.WriteLine(string.Format("Server: Client Connected ({0})", e.ClientSocket.Id));
        }
    }
}
