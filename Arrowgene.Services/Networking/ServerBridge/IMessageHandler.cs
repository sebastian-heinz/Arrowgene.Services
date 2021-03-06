﻿using Arrowgene.Services.Networking.ServerBridge.Messages;

namespace Arrowgene.Services.Networking.ServerBridge
{
    public interface IMessageHandler<TRequest, TResponse>
    {
        Response<TResponse> Handle(Request<TRequest> request);
        string HandlerId { get; }
    }
}