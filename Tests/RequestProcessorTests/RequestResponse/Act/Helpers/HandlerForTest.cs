using Agatha.Common;
using Agatha.ServiceLayer;

namespace Tests.RequestProcessorTests.RequestResponse.Act.Helpers
{
    public class HandlerForTest<TRequest> : AbstractHandlerForTest, IRequestHandler<TRequest>
        where TRequest : Request
    {
        public Response Handle(TRequest request)
        {
            return Handle(request);
        }

        public void Dispose() { }

        public Response Handle(Request request)
        {
            HandledRequest++;
            HandleAction(ExceptionsThrown);
            return new Response();
        }

        public Response CreateDefaultResponse()
        {
            DefaultResponseReturned++;
            ExceptionsThrown.Add(null);
            return new Response();
        }
    }
}