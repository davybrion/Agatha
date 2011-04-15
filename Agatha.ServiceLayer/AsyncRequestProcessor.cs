using System;
using System.ComponentModel;
using System.Threading;
using Agatha.Common;

namespace Agatha.ServiceLayer
{
	public class AsyncRequestProcessor : Disposable, IAsyncRequestProcessor
	{
		private readonly IRequestProcessor requestProcessor;

		private readonly Func<Request[], Response[]> processFunc;
		private readonly Action<OneWayRequest[]> processOneWayRequestsAction;

		public AsyncRequestProcessor(IRequestProcessor requestProcessor)
		{
			this.requestProcessor = requestProcessor;
			processFunc = requestProcessor.Process;
			processOneWayRequestsAction = requestProcessor.ProcessOneWayRequests;
		}

		protected override void DisposeManagedResources()
		{
			if (requestProcessor != null) requestProcessor.Dispose();
		}

		public IAsyncResult BeginProcessRequests(Request[] requests, AsyncCallback callback, object asyncState)
		{
			return processFunc.BeginInvoke(requests, callback, asyncState);
		}

		public Response[] EndProcessRequests(IAsyncResult result)
		{
			return processFunc.EndInvoke(result);
		}

		public void ProcessRequestsAsync(Request[] requests, Action<ProcessRequestsAsyncCompletedArgs> callback)
		{
			var asyncResult = BeginProcessRequests(requests, null, null);
			ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle, 
				(state, timedout) => ProcessRequestsCompleted((IAsyncResult)state, callback), asyncResult, -1, true);
		}

		private void ProcessRequestsCompleted(IAsyncResult asyncResult, Action<ProcessRequestsAsyncCompletedArgs> callback)
		{
			try
			{
				var responses = EndProcessRequests(asyncResult);
				callback(new ProcessRequestsAsyncCompletedArgs(new object[] { responses }, null, false, null));
			}
			catch (Exception e)
			{
				callback(new ProcessRequestsAsyncCompletedArgs(null, e, false, null));	
			}
		}

		public IAsyncResult BeginProcessOneWayRequests(OneWayRequest[] requests, AsyncCallback callback, object asyncState)
		{
			return processOneWayRequestsAction.BeginInvoke(requests, callback, asyncState);
		}

		public void EndProcessOneWayRequests(IAsyncResult result)
		{
			processOneWayRequestsAction.EndInvoke(result);
		}

		public void ProcessOneWayRequestsAsync(OneWayRequest[] requests, Action<AsyncCompletedEventArgs> callback)
		{
			var asyncResult = BeginProcessOneWayRequests(requests, null, null);
			ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle,
				(state, timedout) => ProcessOneWayRequestsCompleted((IAsyncResult)state, callback), asyncResult, -1, true);
		}

		private void ProcessOneWayRequestsCompleted(IAsyncResult asyncResult, Action<AsyncCompletedEventArgs> callback)
		{
			try
			{
				EndProcessOneWayRequests(asyncResult);
				callback(new AsyncCompletedEventArgs(null, false, null));
			}
			catch (Exception e)
			{
				callback(new AsyncCompletedEventArgs(e, false, null));
			}
		}
	}
}