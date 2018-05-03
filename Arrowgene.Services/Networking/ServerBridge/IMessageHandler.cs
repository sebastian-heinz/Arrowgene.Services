using System;
using Arrowgene.Services.Networking.ServerBridge.Messages;

namespace Arrowgene.Services.Networking.ServerBridge
{
    public interface IMessageHandler<T>
    {
        Response<T> Handle(Request request);
        Guid HandlerId { get; }
    }
}