namespace Arrowgene.Services.PingPong
{
    public interface IHandler
    {
        int Id { get; }
        void Handle(Connection connection, Packet packet);
    }
}