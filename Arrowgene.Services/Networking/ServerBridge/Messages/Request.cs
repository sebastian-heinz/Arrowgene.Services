using System;

namespace Arrowgene.Services.Networking.ServerBridge.Messages
{
    [Serializable]
    public abstract class Request : Message
    {
        /// <summary>
        /// Identifies which handler to call.
        /// </summary>
        public Guid HandlerId { get; set; }

        public Request(Guid handlerId)
        {
            HandlerId = handlerId;
        }
    }

    [Serializable]
    public class Request<TRequest> : Request
    {
        public TRequest Content { get; }

        public Request(Guid handlerId, TRequest content) : base(handlerId)
        {
            Content = content;
        }
    }
}