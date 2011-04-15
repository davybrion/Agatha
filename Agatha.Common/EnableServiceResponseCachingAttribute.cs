using System;

namespace Agatha.Common
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public class EnableServiceResponseCachingAttribute : Attribute, IEnableResponseCachingAttribute
	{
		public string Region { get; set; }
		public int Hours { get; set; }
		public int Minutes { get; set; }
		public int Seconds { get; set; }

		public TimeSpan Expiration
		{
			get
			{
				if (Hours == 0 && Minutes == 0 && Seconds == 0)
				{
					throw new InvalidOperationException("You need to specify at least an hour value, a minute value or a second value");
				}

				return new TimeSpan(0, Hours, Minutes, Seconds);
			}
		}
	}
}