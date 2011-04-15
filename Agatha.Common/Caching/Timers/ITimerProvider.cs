namespace Agatha.Common.Caching.Timers
{
    public interface ITimerProvider
    {
        ITimer GetTimer(double interval);    
    }
}
