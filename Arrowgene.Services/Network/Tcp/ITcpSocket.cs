namespace Arrowgene.Services.Network.Tcp
{
    public interface ITcpSocket
    {
        void Send(byte[] payload);
        void Close();
    }
}