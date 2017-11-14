namespace Arrowgene.Services.Buffers
{
    public interface IBufferProvider
    {
        IBuffer Provide();
        IBuffer Provide(byte[] buffer);
    }
}