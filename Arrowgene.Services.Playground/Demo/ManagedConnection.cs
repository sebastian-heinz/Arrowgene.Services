

namespace Arrowgene.Services.Playground.Demo
{
    using Network.ManagedConnection.Client;
    using Network.ManagedConnection.Server;
    using System;
    using System.Net;

    public class ManagedConnection
    {

        public ManagedConnection()
        {

            ManagedServer svr = new ManagedServer(IPAddress.Any, 2345);
            svr.ClientConnected += Svr_ClientConnected;
            svr.ClientDisconnected += Svr_ClientDisconnected;
            svr.ReceivedPacket += Svr_ReceivedPacket;
            svr.Start();

            ManagedClient cli = new ManagedClient(IPAddress.Parse("192.168.10.39"), 2345);
            cli.Connected += Cli_Connected;
            cli.Disconnected += Cli_Disconnected;
            cli.ReceivedPacket += Cli_ReceivedPacket;
            cli.Connect();

           

            Console.WriteLine("Press any key to exit.");

            Console.ReadKey();
            cli.Disconnect();
            svr.Stop();
        }

        private void Cli_ReceivedPacket(object sender, Network.ManagedConnection.Event.ReceivedPacketEventArgs e)
        {
            Console.WriteLine(string.Format("Client: received packet {0} from {1}", e.Packet.Id, e.ClientSocket.Id));
        }

        private void Cli_Disconnected(object sender, Network.ManagedConnection.Event.DisconnectedEventArgs e)
        {
            Console.WriteLine("Client Disconnected");
        }

        private void Cli_Connected(object sender, Network.ManagedConnection.Event.ConnectedEventArgs e)
        {
            Console.WriteLine("Client Connected");
        }

        private void Svr_ReceivedPacket(object sender, Network.ManagedConnection.Event.ReceivedPacketEventArgs e)
        {
            Console.WriteLine(string.Format("Server: received packet {0} from {1}", e.Packet.Id, e.ClientSocket.Id));
        }

        private void Svr_ClientDisconnected(object sender, Network.ManagedConnection.Event.DisconnectedEventArgs e)
        {
            Console.WriteLine(string.Format("Server: Client Disconnected ({0})", e.ClientSocket.Id));
        }

        private void Svr_ClientConnected(object sender, Network.ManagedConnection.Event.ConnectedEventArgs e)
        {
            Console.WriteLine(string.Format("Server: Client Connected ({0})", e.ClientSocket.Id));
        }
    }
}
