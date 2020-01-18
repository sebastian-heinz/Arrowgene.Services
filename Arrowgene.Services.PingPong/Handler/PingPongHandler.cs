namespace Arrowgene.Services.PingPong.Handler
{
    public class PingPongHandler : IHandler
    {
        public int Id => 0;

        public void Handle(Connection connection, Packet packet)
        {
            connection.Send(packet.Data);
        }
    }
}