using System;
using System.Timers;

namespace Agatha.Common.Caching.Timers
{
    public class TimerWrapper : ITimer
    {
        public event EventHandler Elapsed;

        private readonly Timer timer;

        public TimerWrapper(double interval)
        {
            timer = new Timer(interval);
        }

        public void Start()
        {
			timer.Elapsed += timer_Elapsed;
			timer.Start();
        }

		void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (Elapsed != null)
			{
				Elapsed(this, new EventArgs());
			}
		}

        public void Stop()
        {
        	timer.Elapsed -= timer_Elapsed;
            timer.Stop();
        }

        public void Dispose()
        {
			timer.Elapsed -= timer_Elapsed;
			timer.Dispose();
        }
    }
}