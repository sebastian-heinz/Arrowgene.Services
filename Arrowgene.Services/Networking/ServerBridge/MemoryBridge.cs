using System;
using System.Collections.Generic;
using Arrowgene.Logging;
using Arrowgene.Services.Networking.ServerBridge.Messages;

namespace Arrowgene.Services.Networking.ServerBridge
{
    public class MemoryBridge<TIdentity> : IBridge<TIdentity>
    {
        private readonly Dictionary<string, Dictionary<string, Func<Request, Response>>> _handlesLookup;
        private readonly Func<TIdentity, string> _identityResolver;
        protected readonly ILogger Logger;

        public MemoryBridge(Func<TIdentity, string> identityResolver = null)
        {
            Logger = LogProvider.Logger(this);
            _handlesLookup = new Dictionary<string, Dictionary<string, Func<Request, Response>>>();
            if (identityResolver != null)
            {
                _identityResolver = identityResolver;
            }
            else
            {
                _identityResolver = ResolveIdentity;
            }
        }

        protected virtual string ResolveIdentity(TIdentity identity)
        {
            if (identity == null)
            {
                return "";
            }

            return identity.GetHashCode().ToString();
        }

        public void AddHandler<TRequest, TResponse>(IMessageHandler<TRequest, TResponse> handler, TIdentity owner)
        {
            string identityOwner = _identityResolver(owner);
            Dictionary<string, Func<Request, Response>> handles;
            if (!_handlesLookup.ContainsKey(identityOwner))
            {
                handles = new Dictionary<string, Func<Request, Response>>();
                _handlesLookup.Add(identityOwner, handles);
            }
            else
            {
                handles = _handlesLookup[identityOwner];
            }

            handles.Add(handler.HandlerId, request =>
            {
                if (request is Request<TRequest>)
                    return (Response) handler.Handle((Request<TRequest>) request);
                Logger.Error(
                    $"Could not cast parameter ({request}) to ({typeof(Request<TRequest>)}) for handler ({handler.HandlerId})");
                return (Response) new Response<ResponseError>(request, ResponseError.RequestTypeCastingFailed);
            });
        }

        public void Send(TIdentity receiver, Message message)
        {
        }

        public void Request<TResponse, T1, T2>(TIdentity receiver, Request request,
            Action<Response<TResponse>, T1, T2> result, T1 parameter1, T2 parameter2)
        {
            string identityReceiver = _identityResolver(receiver);
            if (!_handlesLookup.ContainsKey(identityReceiver))
            {
                Logger.Error($"Could not find receiver (Receiver: {receiver})");
                return;
            }

            Dictionary<string, Func<Request, Response>> handles = _handlesLookup[identityReceiver];
            if (!handles.ContainsKey(request.HandlerId))
            {
                Logger.Error($"Could not find handler (HandlerId: {request.HandlerId})");
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
                Logger.Error($"Could not cast parameter ({response}) to ({typeof(Response<TResponse>)})");
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