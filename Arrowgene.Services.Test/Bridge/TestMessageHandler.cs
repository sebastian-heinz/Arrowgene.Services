using Arrowgene.Services.Networking.ServerBridge;
using Arrowgene.Services.Networking.ServerBridge.Messages;

namespace Arrowgene.Services.Test.Bridge
{
    public class TestMessageHandler : IMessageHandler<TestRequest, TestResponse>
    {
        public static string Id = "TestMessageHandler";

        public Response<TestResponse> Handle(Request<TestRequest> request)
        {
            return new Response<TestResponse>(request, new TestResponse {ResponseText = "RESPONSE"});
        }

        public string HandlerId => Id;
    }
}