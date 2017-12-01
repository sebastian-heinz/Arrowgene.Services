namespace Arrowgene.Services.Protocols
{
    public interface IProtocol<T1,T2>
    {
        T1 Serialize(T2 concrete);
        T2 Deserialize(T1 data);
    }
}