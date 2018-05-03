using System;

namespace Arrowgene.Services.Networking.ServerBridge.Messages
{
    [Serializable]
    public class Request : Message
    {
        /// <summary>
        /// Identifies which handler to call.
        /// </summary>
        public Guid HandlerId { get; set; }

        /// <summary>
        /// Can be used to transmit user specific porperties to the handler.
        /// The object an all its properties must be serializable.
        /// </summary>
        public object Context { get; set; }

        public Request()
        {
        }
        
        public Request(Guid handlerId)
        {
            HandlerId = handlerId;
        }

        public T GetContext<T>()
        {
            if (Context is T)
            {
                return (T) Context;
            }
            return default(T);
        }
    }
}