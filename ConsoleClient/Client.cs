namespace ConsoleClient
{
    using ConsoleClient.Packets;
    using MarrySocket.MClient;
    using MarrySocket.MExtra.Layout;
    using MarrySocket.MExtra.Logging;
    using System;

    public class Client : ClientLayout
    {
        private HandlePacket handlePacket;

        public Client()
        {
            this.handlePacket = new HandlePacket();
            base.logger.OnLogWrite = OnLogWrite;
        }

        private void OnLogWrite(Log log)
        {
            Console.WriteLine(log.Text);
        }

        protected override void Handle(int packetId, object receivedClass, ServerSocket serverSocket)
        {
            this.handlePacket.Handle(packetId, receivedClass, serverSocket);
        }

        protected override void OnConnected(string reason)
        {

            base.OnConnected(reason);
            this.handlePacket.Send(base.entitiesContainer.ServerSocket, new SendComputerInfo());
        }


        protected override void OnDisconnected(string reason)
        {
            base.OnDisconnected(reason);
        }

    }
}
