using System;

namespace Agatha.Common.Caching.Timers
{
    public interface ITimer : IDisposable
    {
    	event EventHandler Elapsed;
        void Start();
        void Stop();
    }
}