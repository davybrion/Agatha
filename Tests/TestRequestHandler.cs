using System;
using System.Threading.Tasks;
using Agatha.Common;
using Agatha.ServiceLayer;

namespace Tests
{
    public class TestRequestHandler : RequestHandler<SomeRequest, SomeResponse>
    {
        public override Response Handle(SomeRequest request)
        {
            throw new System.NotImplementedException();
        }

        public override Task<Response> HandleAsync(SomeRequest request)
        {
            throw new NotImplementedException();
        }
    }

    public class SomeRequest : Request
    {
    }

    public class SomeResponse : Response
    {
    }
}