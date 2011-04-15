using System;
using System.ComponentModel;

namespace Agatha.Common
{
	public interface IAsyncRequestProcessor : IDisposable
	{
		IAsyncResult BeginProcessRequests(Request[] requests, AsyncCallback callback, object asyncState);
		Response[] EndProcessRequests(IAsyncResult result);
		void ProcessRequestsAsync(Request[] requests, Action<ProcessRequestsAsyncCompletedArgs> processCompleted);

		IAsyncResult BeginProcessOneWayRequests(OneWayRequest[] requests, AsyncCallback callback, object asyncState);
		void EndProcessOneWayRequests(IAsyncResult result);
		void ProcessOneWayRequestsAsync(OneWayRequest[] requests, Action<AsyncCompletedEventArgs> processCompleted);
	}
}