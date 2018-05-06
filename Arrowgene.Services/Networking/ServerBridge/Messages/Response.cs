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
    public class Response<TResponse> : Response
    {
        public TResponse Result { get; }

        public Response(Request request, TResponse result) : base(request.Id)
        {
            Result = result;
        }
    }

}