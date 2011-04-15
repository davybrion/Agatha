using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agatha.Common;

namespace Sample.Common.RequestsAndResponses
{
    public class SetCacheCommand : OneWayRequest
    {
        public string CacheKey { get; set; }
        public string CacheValue { get; set; }
    }

    public class GetCacheRequest : Request
    {
        public string CacheKey { get; set; }
    }

    public class GetCacheResponse : Response
    {
        public string CacheValue { get; set; }
    }
}
