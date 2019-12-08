using System;
using System.Collections.Generic;
using Arrowgene.Services.Logging;
using Arrowgene.Services.Networking.ServerBridge.Messages;

namespace Arrowgene.Services.Networking.ServerBridge
{
    public abstract class Bridge<TIdentity> : IBridge<TIdentity>
    {
        private readonly Dictionary<Guid, Action<Response>> _subscriber;
        private readonly Dictionary<string, Func<Request, Response>> _handler;
        protected readonly ILogger Logger;

        protected Bridge()
        {
            Logger = LogProvider.Logger(this);
            _subscriber = new Dictionary<Guid, Action<Response>>();
            _handler = new Dictionary<string, Func<Request, Response>>();
        }

        public abstract void Start();

        public abstract void Stop();

        public abstract void Send(TIdentity receiver, Message message);

        public void Request<TResponse, T2, T3>(TIdentity receiver, Request request,
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
                        Logger.Error($"Error on remote: ({error.Result})");
                    }
                    else
                    {
                        Logger.Error($"Could not cast parameter ({response}) to ({typeof(Response<TResponse>)})");
                    }
                }
            });
            Send(receiver, request);
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

        public void AddHandler<TRequest, TResponse>(IMessageHandler<TRequest, TResponse> handler, TIdentity owner)
        {
            _handler.Add(handler.HandlerId, request =>
            {
                if (request is Request<TRequest>)
                {
                    return handler.Handle((Request<TRequest>) request);
                }

                Logger.Error(
                    $"Could not cast parameter ({request}) to ({typeof(Request<TRequest>)}) for handler ({handler.HandlerId})");
                return new Response<ResponseError>(request, ResponseError.RequestTypeCastingFailed);
            });
        }

        protected void HandleMessage(TIdentity sender, Message message)
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
                        Logger.Error($"Handler ({request.HandlerId}) produced null response");
                        Send(sender, new Response<ResponseError>(request, ResponseError.NullResponse));
                    }
                }
                else
                {
                    Logger.Error($"Could not find handler (HandlerId: {request.HandlerId})");
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
                    Logger.Error($"Could not find subscriber for message (Id: {message.Id})");
                }
            }
            else
            {
                Logger.Error($"Could not handle message ({message})");
            }
        }
    }
}