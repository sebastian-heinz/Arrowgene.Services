using System;

namespace Arrowgene.Services.Networking.ServerBridge.Messages
{
    [Serializable]
    public abstract class Response : Message
    {
        public Response(Guid id) : base(id)
        {
        }
    }

    [Serializable]
    public class Response<T> : Response
    {
        public T Result { get; }

        public Response(Guid id, T result) : base(id)
        {
            Result = result;
        }

        public Response(Request request, T result) : base(request.Id)
        {
            Result = result;
        }
    }
}