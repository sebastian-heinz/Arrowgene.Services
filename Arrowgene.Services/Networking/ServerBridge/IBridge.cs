using System;
using System.Net;
using Arrowgene.Services.Networking.ServerBridge.Messages;

namespace Arrowgene.Services.Networking.ServerBridge
{
    /// <summary>
    /// Responsible for EzServer-to-EzServer communication.
    /// </summary>
    /// <example>
    /// This sample shows how to define a handler and request a response.
    /// <code>
    /// // Define a handler class:
    /// public class TestMessageHandler : IMessageHandler&lt;TestMessage>
    /// {
    ///    public Response&lt;TestMessage> Handle(Request request)
    ///    {
    ///       return new Response&lt;TestMessage>(request.Id, new TestMessage {Test = "Test"});
    ///    }
    /// }
    /// // Register it with the bridge:
    /// bridge2.AddHandler(new TestMessageHandler())
    /// 
    /// // Send a request:
    /// bridge1.Request&lt;TestMessage>(bridge2.PublicEndPoint, response =>
    /// {
    ///    // Access response here.
    /// });
    /// </code>
    /// </example>
    public interface IBridge<in TIdentity>
    {
        void AddHandler<TRequest, TResponse>(IMessageHandler<TRequest, TResponse> handler,
            TIdentity owner = default(TIdentity));

        void Send(TIdentity receiver, Message message);

        void Request<TResponse, T2, T3>(TIdentity receiver, Request request,
            Action<Response<TResponse>, T2, T3> result,
            T2 parameter1, T3 parameter2);

        void Request<TResponse, T2>(TIdentity receiver, Request request, Action<Response<TResponse>, T2> result,
            T2 parameter);

        void Request<TResponse>(TIdentity receiver, Request request, Action<Response<TResponse>> result);

        void Start();
        void Stop();
    }
}