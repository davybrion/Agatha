using System;

namespace Agatha.Common.Caching
{
	public interface ICache
	{
		Response GetCachedResponseFor(Request request);
		void Store(Request request, Response response, TimeSpan expiration);
		void Clear();
	}
}