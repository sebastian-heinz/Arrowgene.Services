using System;

namespace Arrowgene.Services.Networking.ServerBridge.Messages
{
    [Serializable]
    public abstract class Request : Message
    {
        /// <summary>
        /// Identifies which handler to call.
        /// </summary>
        public string HandlerId { get; set; }

        public Request(string handlerId)
        {
            HandlerId = handlerId;
        }
    }

    [Serializable]
    public class Request<TRequest> : Request
    {
        public TRequest Content { get; }

        public Request(string handlerId, TRequest content) : base(handlerId)
        {
            Content = content;
        }
    }
}