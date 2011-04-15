using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agatha.ServiceLayer;
using Sample.Common.RequestsAndResponses;

namespace Sample.ServiceLayer.Handlers
{
    public class GetCacheHandler : RequestHandler<GetCacheRequest,GetCacheResponse>
    {
        public override Agatha.Common.Response Handle(GetCacheRequest request)
        {
            var response = new GetCacheResponse();
            response.CacheValue = FakeCacheStore.Get(request.CacheKey);
            return response;
        }
    }
}
