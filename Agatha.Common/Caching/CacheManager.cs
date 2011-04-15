using System;
using System.Collections.Generic;
using System.Linq;

namespace Agatha.Common.Caching
{
	public interface ICacheManager
	{
		bool IsCachingEnabledFor(Type requestType);
		Response GetCachedResponseFor(Request request);
		void StoreInCache(Request request, Response response);
		void Clear(string region);
	}

	public class CacheManager : ICacheManager
	{
		private const string DefaultRegionName = "_defaultRegion";

		private readonly CacheConfiguration configuration;
		private readonly ICacheProvider cacheProvider;
		private Dictionary<string, ICache> caches;

		public CacheManager(CacheConfiguration configuration, ICacheProvider cacheProvider)
		{
			this.configuration = configuration;
			this.cacheProvider = cacheProvider;
			BuildCaches();
		}

		private void BuildCaches()
		{
			var regions = configuration.GetRegionNames();
			caches = new Dictionary<string, ICache>(regions.Count() + 1);
			caches.Add(DefaultRegionName, cacheProvider.BuildCache(DefaultRegionName));

			foreach (var region in regions)
			{
				caches.Add(region, cacheProvider.BuildCache(region));
			}
		}

		public virtual bool IsCachingEnabledFor(Type requestType)
		{
			return configuration.IsCachingEnabledFor(requestType);
		}

		public virtual Response GetCachedResponseFor(Request request)
		{
			return GetCachedResponseFor(request, configuration.GetRegionNameFor(request.GetType()) ?? DefaultRegionName);
		}

		protected virtual Response GetCachedResponseFor(Request request, string region)
		{
			return caches[region].GetCachedResponseFor(request);
		}

		public virtual void StoreInCache(Request request, Response response)
		{
			var config = configuration.GetConfigurationFor(request.GetType());
			StoreInCache(request, response, config.Expiration, config.Region ?? DefaultRegionName);
		}

		protected virtual void StoreInCache(Request request, Response response, TimeSpan expiration, string region)
		{
			var clone = response.GetShallowCopy(); // shallow copy is sufficient
			clone.IsCached = true;
			caches[region].Store(request, clone, expiration);
		}

		public virtual void Clear(string region)
		{
			caches[region].Clear();
		}
	}
}