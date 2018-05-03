using System;
using Arrowgene.Services.Bridging.Messages;

namespace Arrowgene.Services.Bridging
{
    public interface IMessageHandler<T>
    {
        Response<T> Handle(Request request);
        Guid HandlerId { get; }
    }
}