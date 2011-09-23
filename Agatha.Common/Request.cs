using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Agatha.Common
{
	[DataContract]
	public abstract class Request
	{
		public Request()
		{
			this.Headers = new Dictionary<string, string>();
		}

		public Dictionary<string, string> Headers { get; private set; }
	}
}