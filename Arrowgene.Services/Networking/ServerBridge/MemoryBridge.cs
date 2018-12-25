using System;
using System.Collections.Generic;
using Arrowgene.Services.Logging;
using Arrowgene.Services.Networking.ServerBridge.Messages;

namespace Arrowgene.Services.Networking.ServerBridge
{
    public class MemoryBridge<TIdentity> : IBridge<TIdentity>
    {
        private readonly Dictionary<TIdentity, Dictionary<string, Func<Request, Response>>> _handlesLookup;
        protected readonly Logger Logger;

        protected MemoryBridge()
        {
            Logger = LogProvider<Logger>.GetLogger(this);
            _handlesLookup = new Dictionary<TIdentity, Dictionary<string, Func<Request, Response>>>();
        }

        public void AddHandler<TRequest, TResponse>(IMessageHandler<TRequest, TResponse> handler, TIdentity owner)
        {
            Dictionary<string, Func<Request, Response>> handles;
            if (!_handlesLookup.ContainsKey(owner))
            {
                handles = new Dictionary<string, Func<Request, Response>>();
                _handlesLookup.Add(owner, handles);
            }
            else
            {
                handles = _handlesLookup[owner];
            }

            handles.Add(handler.HandlerId, request =>
            {
                if (request is Request<TRequest>)
                    return (Response) handler.Handle((Request<TRequest>) request);
                Logger.Error("Could not cast parameter ({0}) to ({1}) for handler ({2})", (object) request,
                    (object) typeof(Request<TRequest>), (object) handler.HandlerId);
                return (Response) new Response<ResponseError>(request, ResponseError.RequestTypeCastingFailed);
            });
        }

        public void Send(TIdentity receiver, Message message)
        {
        }

        public void Request<TResponse, T1, T2>(TIdentity receiver, Request request,
            Action<Response<TResponse>, T1, T2> result, T1 parameter1, T2 parameter2)
        {
            if (!_handlesLookup.ContainsKey(receiver))
            {
                Logger.Error("Could not find receiver (Receiver: {0})", receiver);
                return;
            }

            Dictionary<string, Func<Request, Response>> handles = _handlesLookup[receiver];
            if (!handles.ContainsKey(request.HandlerId))
            {
                Logger.Error("Could not find handler (HandlerId: {0})", request.HandlerId);
                return;
            }

            Func<Request, Response> handler = handles[request.HandlerId];
            Response response = handler(request);

            if (response is Response<TResponse>)
            {
                result((Response<TResponse>) response, parameter1, parameter2);
            }
            else
            {
                Logger.Error("Could not cast parameter ({0}) to ({1})", response, typeof(Response<TResponse>));
            }
        }

        public void Request<TResponse, T2>(TIdentity receiver, Request request, Action<Response<TResponse>, T2> result,
            T2 parameter)
        {
            Request(receiver, request, (Response<TResponse> response, T2 p1, object p2) => result(response, p1),
                parameter, null);
        }

        public void Request<TResponse>(TIdentity receiver, Request request, Action<Response<TResponse>> result)
        {
            Request(receiver, request, (Response<TResponse> response, object p1, object p2) => result(response),
                null, null);
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}