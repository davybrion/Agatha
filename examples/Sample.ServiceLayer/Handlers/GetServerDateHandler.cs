using Agatha.Common;
using Agatha.ServiceLayer;
using Sample.Common.RequestsAndResponses;
using System;

namespace Sample.ServiceLayer.Handlers
{
    public class GetServerDateHandler : RequestHandler<GetServerDateRequest, GetServerDateResponse>
    {
        public override Response Handle(GetServerDateRequest request)
        {
            var response = CreateTypedResponse();
            response.Date = DateTime.Now;
            return response;
        }
    }
}
