using Arrowgene.Services.Networking.Tcp.Client;
using Arrowgene.Services.Networking.Tcp.Client.Consumer;
using Arrowgene.Services.Networking.Tcp.Server;
using Arrowgene.Services.Networking.Tcp.Server.Consumer;

namespace Arrowgene.Services.Networking.Tcp.Protocols
{
    public interface IProtocol : IServerProducer, IServerConsumer, IClientProducer, IClientConsumer
    {
    }
}