namespace ConsoleClient
{
    using ConsoleClient.Packets;
    using MarrySocket.MClient;
    using System;

    public class HandlePacket
    {

        public HandlePacket()
        {

        }

        public void Handle(int packetId, object receivedClass, ServerSocket serverSocket)
        {
            switch (packetId)
            {
                case 1111:
                    if (receivedClass is Int64)
                    {
                        long quality = (long)receivedClass;
                        this.Send(serverSocket, new SendScreenShot(quality));
                    }
                    break;
            }
        }

        public void Send(ServerSocket serverSocket, ISendPacket iSendPacket)
        {
            iSendPacket.Send(serverSocket);
        }


    }
}
