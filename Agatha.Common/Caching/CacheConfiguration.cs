using System;
using System.Collections.Generic;
using System.Linq;

namespace Agatha.Common.Caching
{
	public abstract class CacheConfiguration
	{
		private Dictionary<Type, RequestCacheConfiguration> requestCacheConfigurations;
		private readonly Type enableResponseCachingAttributeType;

		protected CacheConfiguration(IEnumerable<Type> knownRequestTypes, Type enableResponseCachingAttributeType)
		{
			this.enableResponseCachingAttributeType = enableResponseCachingAttributeType;
			BuildMapOfConfigurationsForRequestsThatEnabledResponseCaching(knownRequestTypes);
		}

		private void BuildMapOfConfigurationsForRequestsThatEnabledResponseCaching(IEnumerable<Type> knownRequestTypes)
		{
			requestCacheConfigurations = new Dictionary<Type, RequestCacheConfiguration>();

			foreach (var requestType in knownRequestTypes)
			{
				dynamic attribute = Attribute.GetCustomAttribute(requestType, enableResponseCachingAttributeType);

				if (attribute != null)
				{
					requestCacheConfigurations.Add(requestType, new RequestCacheConfiguration(requestType, attribute.Expiration, attribute.Region));
				}
			}
		}

		public bool IsCachingEnabledFor(Type requestType)
		{
			return requestCacheConfigurations.ContainsKey(requestType);
		}

		public RequestCacheConfiguration GetConfigurationFor(Type requestType)
		{
			return requestCacheConfigurations[requestType];
		}

		public IEnumerable<string> GetRegionNames()
		{
			return new HashSet<string>(requestCacheConfigurations.Values.Where(v => !string.IsNullOrEmpty(v.Region)).Select(v => v.Region));
		}

		public string GetRegionNameFor(Type requestType)
		{
			return requestCacheConfigurations[requestType].Region;
		}
	}

	public class ServiceCacheConfiguration : CacheConfiguration
	{
		public ServiceCacheConfiguration(IEnumerable<Type> knownRequestTypes) : base(knownRequestTypes, typeof(EnableServiceResponseCachingAttribute)) {}
	}

	public class ClientCacheConfiguration : CacheConfiguration
	{
		public ClientCacheConfiguration(IEnumerable<Type> knownRequestTypes) : base(knownRequestTypes, typeof(EnableClientResponseCachingAttribute)) {}
	}
}