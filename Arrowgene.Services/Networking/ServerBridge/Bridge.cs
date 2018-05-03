using System;
using System.Collections.Generic;
using System.Net;
using Arrowgene.Services.Logging;
using Arrowgene.Services.Networking.ServerBridge.Messages;

namespace Arrowgene.Services.Networking.ServerBridge
{
    public abstract class Bridge : IBridge
    {
        private Dictionary<Guid, Action<Response>> _subscriber;
        private Dictionary<Guid, Func<Request, Response>> _handler;
        protected readonly Logger Logger;

        public Bridge()
        {
            Logger = LogProvider<Logger>.GetLogger(this);
            _subscriber = new Dictionary<Guid, Action<Response>>();
            _handler = new Dictionary<Guid, Func<Request, Response>>();
        }

        public abstract void Start();

        public abstract void Stop();

        public abstract void Send(IPEndPoint receiver, Message message);

        public void Request<T1, T2, T3>(IPEndPoint receiver, Request request, Action<Response<T1>, T2, T3> result,
            T2 parameter1, T3 parameter2)
        {
            CheckHandlerId(request.HandlerId);
            _subscriber.Add(request.Id, response =>
            {
                if (response is Response<T1>)
                {
                    result((Response<T1>) response, parameter1, parameter2);
                }
                else
                {
                    Logger.Error("Could not cast parameter ({0}) to ({1})", response, typeof(T1));
                }
            });
            Send(receiver, request);
        }

        public void Request<T1, T2, T3>(IPEndPoint receiver, Guid handlerId, object context, Action<Response<T1>, T2, T3> result,
            T2 parameter1, T3 parameter2)
        {
            Request(receiver, new Request {Context = context, HandlerId = handlerId}, result, parameter1, parameter2);
        }

        public void Request<T1, T2, T3>(IPEndPoint receiver, Guid handlerId, Action<Response<T1>, T2, T3> result, T2 parameter1,
            T3 parameter2)
        {
            Request(receiver, new Request(handlerId), result, parameter1, parameter2);
        }

        public void Request<T1, T2>(IPEndPoint receiver, Request request, Action<Response<T1>, T2> result, T2 parameter)
        {
            CheckHandlerId(request.HandlerId);
            _subscriber.Add(request.Id, response =>
            {
                if (response is Response<T1>)
                {
                    result((Response<T1>) response, parameter);
                }
                else
                {
                    Logger.Error("Could not cast parameter ({0}) to ({1})", response, typeof(T1));
                }
            });
            Send(receiver, request);
        }

        public void Request<T1, T2>(IPEndPoint receiver, Guid handlerId, object context, Action<Response<T1>, T2> result, T2 parameter)
        {
            Request(receiver, new Request {Context = context, HandlerId = handlerId}, result, parameter);
        }

        public void Request<T1, T2>(IPEndPoint receiver, Guid handlerId, Action<Response<T1>, T2> result, T2 parameter)
        {
            Request(receiver, new Request(handlerId), result, parameter);
        }

        public void Request<T>(IPEndPoint receiver, Request request, Action<Response<T>> result)
        {
            CheckHandlerId(request.HandlerId);
            _subscriber.Add(request.Id, response =>
            {
                if (response is Response<T>)
                {
                    result((Response<T>) response);
                }
                else
                {
                    Logger.Error("Could not cast parameter ({0}) to ({1})", response, typeof(T));
                }
            });
            Send(receiver, request);
        }

        public void Request<T>(IPEndPoint receiver, Guid handlerId, object context, Action<Response<T>> result)
        {
            Request(receiver, new Request {Context = context, HandlerId = handlerId}, result);
        }

        public void Request<T>(IPEndPoint receiver, Guid handlerId, Action<Response<T>> result)
        {
            Request(receiver, new Request(handlerId), result);
        }

        public void AddHandler<T>(IMessageHandler<T> handler)
        {
            _handler.Add(handler.HandlerId, request =>
            {
                if (request != null)
                {
                    return handler.Handle(request);
                }

                Logger.Error("Request is null for handler ({0})", handler.HandlerId);
                return null;
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
                    }
                }
                else
                {
                    Logger.Error("Could not find handler (HandlerId: {0})", request.HandlerId);
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

        private bool CheckHandlerId(Guid handlerId)
        {
            if (handlerId == Guid.Empty)
            {
                Logger.Error("Request with empty guid");
                return false;
            }

            if (!_handler.ContainsKey(handlerId))
            {
                Logger.Error("Request without handler ({0})", handlerId);
                return false;
            }

            return true;
        }
    }
}