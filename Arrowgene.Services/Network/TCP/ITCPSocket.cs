namespace Arrowgene.Services.Network.TCP
{
    public interface ITCPSocket
    {
        void Send(byte[] payload);
        void Close();
    }
}