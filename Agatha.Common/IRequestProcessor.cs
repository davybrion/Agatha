using System;

namespace Agatha.Common
{
	public interface IRequestProcessor : IDisposable
	{
		Response[] Process(params Request[] requests);
		void ProcessOneWayRequests(params OneWayRequest[] requests);
	}
}