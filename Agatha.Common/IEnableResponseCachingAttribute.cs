using System;

namespace Agatha.Common
{
	public interface IEnableResponseCachingAttribute
	{
		string Region { get; set; }
		int Hours { get; set; }
		int Minutes { get; set; }
		int Seconds { get; set; }
		TimeSpan Expiration { get; }
	}
}