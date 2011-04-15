namespace Agatha.Common.Caching.Timers
{
    public class TimerProvider : ITimerProvider
    {
        public ITimer GetTimer(double interval)
        {
            return new TimerWrapper(interval);
        }
    }
}
