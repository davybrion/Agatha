using System;
using System.Collections.Generic;
using Agatha.Common;
using Agatha.Common.Caching;

namespace Tests
{
	public class CacheManagerSpy : CacheManager
	{
		private List<CacheEntry> cacheEntries;
        
        private List<Request> requestsReceived; 
        private List<Response> returnedCachedResponses;
        
        public List<Exception> ExceptionsThrown { get; set; }

		public IEnumerable<CacheEntry> CacheEntries
		{
			get { return cacheEntries; }
		}

        public IEnumerable<Request> RequestsReceived
        {
            get { return requestsReceived; }
        }

		public IEnumerable<Response> ReturnedCachedResponses
		{
			get { return returnedCachedResponses; }
		}

		public CacheManagerSpy(CacheConfiguration configuration, ICacheProvider cacheProvider) : base(configuration, cacheProvider)
		{
			Clear();
		}

		public void Clear() 
		{
			cacheEntries = new List<CacheEntry>();
            requestsReceived = new List<Request>();
			returnedCachedResponses = new List<Response>();
		}

		protected override Response GetCachedResponseFor(Request request, string region)
		{
			var cachedResponse = base.GetCachedResponseFor(request, region);
			requestsReceived.Add(request);
            returnedCachedResponses.Add(cachedResponse);

            // WORKAROUND : 
            // handler not called adding null to ExceptionsThrown here to avoid hard lookups later.
            
            if(cachedResponse != null)
                ExceptionsThrown.Add(null);
			
            return cachedResponse;
		}

		protected override void StoreInCache(Request request, Response response, TimeSpan expiration, string region)
		{
			cacheEntries.Add(new CacheEntry(request, response, expiration, region));
			base.StoreInCache(request, response, expiration, region);
		}

        public bool ReturnedCachedResponseFor(Request request)
        {
            int ix = requestsReceived.FindIndex(r => r == request);
            if (ix < 0)
                return false;
            return (returnedCachedResponses[ix] != null);
        }

		public class CacheEntry
		{
			public Request Request { get; private set; }
			public Response Response { get; private set; }
			public TimeSpan Expiration { get; private set; }
			public string Region { get; private set; }

			public CacheEntry(Request request, Response response, TimeSpan expiration, string region)
			{
				Request = request;
				Response = response;
				Expiration = expiration;
				Region = region;
			}
		}
	}
}