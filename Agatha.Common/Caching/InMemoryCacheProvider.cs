using Agatha.Common.Caching.Timers;

namespace Agatha.Common.Caching
{
	public class InMemoryCacheProvider : ICacheProvider
	{
	    private readonly ITimerProvider timerProvider;

        public InMemoryCacheProvider(ITimerProvider timerProvider)
        {
            this.timerProvider = timerProvider;
        }

	    public ICache BuildCache(string s)
		{
			return new InMemoryCache(timerProvider);	
		}
	}
}