using System;
using System.Collections.Generic;
using System.Net;
using Arrowgene.Services.Logging;
using Arrowgene.Services.Networking.ServerBridge.Messages;

namespace Arrowgene.Services.Networking.ServerBridge
{
    public abstract class Bridge : IBridge
    {
        private readonly Dictionary<Guid, Action<Response>> _subscriber;
        private readonly Dictionary<Guid, Func<Request, Response>> _handler;
        protected readonly Logger Logger;

        protected Bridge()
        {
            Logger = LogProvider<Logger>.GetLogger(this);
            _subscriber = new Dictionary<Guid, Action<Response>>();
            _handler = new Dictionary<Guid, Func<Request, Response>>();
        }

        public abstract void Start();

        public abstract void Stop();

        public abstract void Send(IPEndPoint receiver, Message message);

        public void Request<TResponse, T2, T3>(IPEndPoint receiver, Request request,
            Action<Response<TResponse>, T2, T3> result,
            T2 parameter1, T3 parameter2)
        {
            _subscriber.Add(request.Id, response =>
            {
                if (response is Response<TResponse>)
                {
                    result((Response<TResponse>) response, parameter1, parameter2);
                }
                else
                {
                    if (response is Response<ResponseError>)
                    {
                        Response<ResponseError> error = (Response<ResponseError>) response;
                        Logger.Error("Error on remote: ({0})", error.Result);
                    }
                    else
                    {
                        Logger.Error("Could not cast parameter ({0}) to ({1})", response, typeof(Response<TResponse>));
                    }
                }
            });
            Send(receiver, request);
        }

        public void Request<TResponse, T2>(IPEndPoint receiver, Request request, Action<Response<TResponse>, T2> result,
            T2 parameter)
        {
            Request(receiver, request, (Response<TResponse> response, T2 p1, object p2) => result(response, p1),
                parameter, null);
        }

        public void Request<TResponse>(IPEndPoint receiver, Request request, Action<Response<TResponse>> result)
        {
            Request(receiver, request, (Response<TResponse> response, object p1, object p2) => result(response),
                null, null);
        }

        public void AddHandler<TRequest, TResponse>(IMessageHandler<TRequest, TResponse> handler)
        {
            _handler.Add(handler.HandlerId, request =>
            {
                if (request is Request<TRequest>)
                {
                    return handler.Handle((Request<TRequest>) request);
                }

                Logger.Error("Could not cast parameter ({0}) to ({1}) for handler ({2})", request,
                    typeof(Request<TRequest>), handler.HandlerId);
                return new Response<ResponseError>(request, ResponseError.RequestTypeCastingFailed);
            });
        }

        protected void HandleMessage(IPEndPoint sender, Message message)
        {
            if (message is Request)
            {
                Request request = (Request) message;
                if (_handler.ContainsKey(request.HandlerId))
                {
                    Response response = _handler[request.HandlerId](request);
                    if (response != null)
                    {
                        Send(sender, response);
                    }
                    else
                    {
                        Logger.Error("Handler ({0}) produced null response", request.HandlerId);
                        Send(sender, new Response<ResponseError>(request, ResponseError.NullResponse));
                    }
                }
                else
                {
                    Logger.Error("Could not find handler (HandlerId: {0})", request.HandlerId);
                    Send(sender, new Response<ResponseError>(request, ResponseError.NoHandler));
                }
            }
            else if (message is Response)
            {
                if (_subscriber.ContainsKey(message.Id))
                {
                    _subscriber[message.Id]((Response) message);
                    _subscriber.Remove(message.Id);
                }
                else
                {
                    Logger.Error("Could not find subscriber for message (Id: {0})", message.Id);
                }
            }
            else
            {
                Logger.Error("Could not handle message ({0})", message);
            }
        }
    }
}