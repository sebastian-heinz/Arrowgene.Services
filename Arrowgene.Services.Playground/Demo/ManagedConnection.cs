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
            svr.Logger.LogWrite += Logger_LogWrite_Server;
            svr.ClientConnected += Svr_ClientConnected;
            svr.ClientDisconnected += Svr_ClientDisconnected;
            svr.ReceivedPacket += Svr_ReceivedPacket;
            svr.Start();

            ManagedClient cli = new ManagedClient(IPAddress.Parse("192.168.178.20"), 2345);
            cli.Logger.LogWrite += Logger_LogWrite_Client;
            cli.Connected += Cli_Connected;
            cli.Disconnected += Cli_Disconnected;
            cli.ReceivedPacket += Cli_ReceivedPacket;
            cli.Connect();

            cli.SendObject(1, "hello server");

            Console.WriteLine("Press any key to disconnect.");
            Console.ReadKey();

            cli.Disconnect();
            svr.Stop();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private void Logger_LogWrite_Client(object sender, Logging.LogWriteEventArgs e)
        {
            Console.WriteLine(string.Format("Client Log: {0}", e.Log.Text));
        }

        private void Logger_LogWrite_Server(object sender, Logging.LogWriteEventArgs e)
        {
            Console.WriteLine(string.Format("Server Log: {0}", e.Log.Text));
        }

        private void Cli_ReceivedPacket(object sender, Network.ManagedConnection.Event.ReceivedPacketEventArgs e)
        {
            string message = e.Packet.GetObject<string>();
            Console.WriteLine(string.Format("Client: received packetID: {0} with message: {1}", e.Packet.Id, message));

            string answer = string.Empty;
            switch (e.PacketId)
            {
                case 2: e.ClientSocket.SendObject(3, "i want to know how you are today"); break;
                case 4: e.ClientSocket.SendObject(5, "good to know bye!"); break;
            }
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
            string message = e.Packet.GetObject<string>();
            Console.WriteLine(string.Format("Server: received packetID: {0} from clientID: {1} with message: {2}", e.Packet.Id, e.ClientSocket.Id, message));

            string answer = string.Empty;
            switch (e.PacketId)
            {
                case 1: e.ClientSocket.SendObject(2, "hello client how can i help you?"); break;
                case 3: e.ClientSocket.SendObject(4, "thanks for asking i am fine"); break;
            }
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
