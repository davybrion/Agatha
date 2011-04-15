using System;
using System.Collections.Generic;
using System.Linq;
using Agatha.Common.Caching.Timers;

namespace Agatha.Common.Caching
{
	public class InMemoryCache : ICache
	{
		private readonly object monitor = new object();
		private Dictionary<Request, Response> cachedResponses;
		private Dictionary<ITimer, Request> timersToRequests;

        private readonly ITimerProvider timerProvider;

        public InMemoryCache(ITimerProvider timerProvider)
        {
            this.timerProvider = timerProvider;
            Initialize();
        }

		private void Initialize()
		{
			cachedResponses = new Dictionary<Request, Response>();
			timersToRequests = new Dictionary<ITimer, Request>();
		}

		public Response GetCachedResponseFor(Request request)
		{
			lock(monitor)
			{
				if (!cachedResponses.ContainsKey(request))
				{
					return null;
				}

				return cachedResponses[request];
			}
		}

		public void Store(Request request, Response response, TimeSpan expiration)
		{
			// TODO: deal with the fact that there might not be an expiration (should we even support this?)

			var timer = timerProvider.GetTimer(expiration.TotalMilliseconds);
			 
			lock (monitor)
			{
				if (cachedResponses.ContainsKey(request))
				{
					// another request handler stored a cached response already during the processing of this request...
					// the last one wins, so get rid of the previous one, including its timer
					cachedResponses.Remove(request);
					var otherTimer = timersToRequests.First(keyValuePair => keyValuePair.Value.Equals(request)).Key;
					GetRidOfTimer(otherTimer);
					timersToRequests.Remove(otherTimer);
				}

				cachedResponses[request] = response;
				timersToRequests[timer] = request;
			}

			timer.Elapsed += timer_Elapsed;
			timer.Start();
		}

		void timer_Elapsed(object sender, EventArgs eventArgs)
		{
			var timer = (ITimer)sender;

			lock (monitor)
			{
				// if the Clear method has been called before a timer has elapsed, it won't be in the timersToRequest dictionary anymore
				if (timersToRequests.ContainsKey(timer))
				{
					var request = timersToRequests[timer];
					cachedResponses.Remove(request);
					timersToRequests.Remove(timer);
				}
			}

			timer.Dispose();
		}

		public void Clear()
		{
			lock(monitor)
			{
				foreach (var timer in timersToRequests.Keys)
				{
					GetRidOfTimer(timer);
				}

				Initialize();
			}
		}

		private void GetRidOfTimer(ITimer timer) 
		{
			timer.Elapsed -= timer_Elapsed;
			timer.Stop();
			timer.Dispose();
		}
	}
}