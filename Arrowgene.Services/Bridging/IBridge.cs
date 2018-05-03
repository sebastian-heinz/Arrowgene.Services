using System;
using System.Net;
using Arrowgene.Services.Bridging.Messages;

namespace Arrowgene.Services.Bridging
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
    public interface IBridge
    {
        /// <summary>
        /// Specifies a handler method for incomming messages.
        /// </summary>
        /// <param name="handler"></param>
        /// <typeparam name="T"></typeparam>
        void AddHandler<T>(IMessageHandler<T> handler);

        /// <summary>
        /// Sends a message to a remote bridge.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="message"></param>
        void Send(IPEndPoint receiver, Message message);

        /// <summary>
        /// Requests a resource from a remote bride.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="request"></param>
        /// <param name="result"></param>
        /// <param name="parameter1"></param>
        /// <param name="parameter2"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        void Request<T1, T2, T3>(IPEndPoint receiver, Request request, Action<Response<T1>, T2, T3> result,
            T2 parameter1, T3 parameter2);

        /// <summary>
        /// Requests a resource from a remote bride.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="handlerId"></param>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <param name="parameter1"></param>
        /// <param name="parameter2"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        void Request<T1, T2, T3>(IPEndPoint receiver, Guid handlerId, object context, Action<Response<T1>, T2, T3> result,
            T2 parameter1, T3 parameter2);

        /// <summary>
        /// Requests a resource from a remote bride.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="handlerId"></param>
        /// <param name="result"></param>
        /// <param name="parameter1"></param>
        /// <param name="parameter2"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        void Request<T1, T2, T3>(IPEndPoint receiver, Guid handlerId, Action<Response<T1>, T2, T3> result, T2 parameter1,
            T3 parameter2);

        /// <summary>
        /// Requests a resource from a remote bride.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="request"></param>
        /// <param name="result"></param>
        /// <param name="parameter"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        void Request<T1, T2>(IPEndPoint receiver, Request request, Action<Response<T1>, T2> result, T2 parameter);

        /// <summary>
        /// Requests a resource from a remote bride.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="handlerId"></param>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <param name="parameter"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        void Request<T1, T2>(IPEndPoint receiver, Guid handlerId, object context, Action<Response<T1>, T2> result, T2 parameter);

        /// <summary>
        /// Requests a resource from a remote bride.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="handlerId"></param>
        /// <param name="result"></param>
        /// <param name="parameter"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        void Request<T1, T2>(IPEndPoint receiver, Guid handlerId, Action<Response<T1>, T2> result, T2 parameter);

        /// <summary>
        /// Requests a resource from a remote bride.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="request"></param>
        /// <param name="result"></param>
        /// <typeparam name="T"></typeparam>
        void Request<T>(IPEndPoint receiver, Request request, Action<Response<T>> result);

        /// <summary>
        /// Requests a resource from a remote bride.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="handlerId"></param>
        /// <param name="context">Can be used to transmit user specific porperties to the handler.
        /// The object and all its properties must be serializable.</param>
        /// <param name="result"></param>
        /// <typeparam name="T"></typeparam>
        void Request<T>(IPEndPoint receiver, Guid handlerId, object context, Action<Response<T>> result);

        /// <summary>
        /// Requests a resource from a remote bride.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="handlerId"></param>
        /// <param name="result"></param>
        /// <typeparam name="T"></typeparam>
        void Request<T>(IPEndPoint receiver, Guid handlerId, Action<Response<T>> result);

        void Start();
        void Stop();
    }
}