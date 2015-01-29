namespace ConsoleClient
{
    using ConsoleClient.Packets;
    using MarrySocket.MClient;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class HandlePacket
    {

        public HandlePacket()
        {

        }

        public void Handle(int packetId, object receivedClass, ServerSocket serverSocket)
        {

        }

        public void Send(ServerSocket serverSocket, ISendPacket iSendPacket)
        {
            iSendPacket.Send(serverSocket);
        }


    }
}
