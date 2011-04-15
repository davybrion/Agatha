using System;
using System.Windows.Threading;

namespace Agatha.Common.Caching.Timers
{
	public class TimerWrapper : ITimer
	{
		public event EventHandler Elapsed;

		private readonly double interval;
		private DispatcherTimer timer;

		public TimerWrapper(double interval)
		{
			this.interval = interval;
		}

		public void Start()
		{
			timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(interval)};
			timer.Tick += timer_Tick;
			timer.Start();
		}

		void timer_Tick(object sender, EventArgs e)
		{
			if (Elapsed != null)
			{
				Elapsed(this, new EventArgs());
			}
		}

		public void Stop()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (timer != null)
			{
				timer.Stop();
			}
			timer = null;
		}
	}
}