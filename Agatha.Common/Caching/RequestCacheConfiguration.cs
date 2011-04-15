using System;

namespace Agatha.Common.Caching
{
	public class RequestCacheConfiguration
	{
		public Type RequestType { get; private set; }
		public TimeSpan Expiration { get; private set; }
		public string Region { get; private set; }

		public RequestCacheConfiguration(Type requestType, TimeSpan expiration, string region)
		{
			RequestType = requestType;
			Expiration = expiration;
			Region = region;
		}
	}
}