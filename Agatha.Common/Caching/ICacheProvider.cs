namespace Agatha.Common.Caching
{
	public interface ICacheProvider
	{
		ICache BuildCache(string region);
	}
}