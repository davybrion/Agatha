using System;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

namespace Agatha.Common.WCF
{
	/// <summary>
	/// Originally written by Tom Ceulemans
	/// </summary>
	public class AsyncRequestProcessorProxy : ClientBase<IAsyncWcfRequestProcessor>, IAsyncRequestProcessor
	{
		public event EventHandler<AsyncCompletedEventArgs> OpenCompleted;

		public AsyncRequestProcessorProxy() {}

		public AsyncRequestProcessorProxy(string endpointConfigurationName, string remoteAddress)
			: base(endpointConfigurationName, remoteAddress) {}

		IAsyncResult IAsyncRequestProcessor.BeginProcessRequests(Request[] requests, AsyncCallback callback, object asyncState)
		{
			return Channel.BeginProcessRequests(requests, callback, asyncState);
		}

		Response[] IAsyncRequestProcessor.EndProcessRequests(IAsyncResult result)
		{
			return Channel.EndProcessRequests(result);
		}

		private IAsyncResult OnBeginProcessRequests(object[] inValues, AsyncCallback callback, object asyncState)
		{
			var requests = ((Request[])(inValues[0]));
			return ((IAsyncRequestProcessor)(this)).BeginProcessRequests(requests, callback, asyncState);
		}

		private object[] OnEndProcessRequests(IAsyncResult result)
		{
			Response[] retVal = ((IAsyncRequestProcessor)(this)).EndProcessRequests(result);
			return new object[] { retVal };
		}

		private void OnProcessRequestsCompleted(object state)
		{
			var e = ((InvokeAsyncCompletedEventArgs)(state));
			((Action<ProcessRequestsAsyncCompletedArgs>)(e.UserState)).Invoke(new ProcessRequestsAsyncCompletedArgs(e.Results, e.Error, e.Cancelled, e.UserState));
		}

		public void ProcessRequestsAsync(Request[] requests, Action<ProcessRequestsAsyncCompletedArgs> processCompleted)
		{
			InvokeAsync(OnBeginProcessRequests, new object[] { requests },
						OnEndProcessRequests, OnProcessRequestsCompleted, processCompleted);
		}

		IAsyncResult IAsyncRequestProcessor.BeginProcessOneWayRequests(OneWayRequest[] oneWayRequests, AsyncCallback callback, object asyncState)
		{
			return Channel.BeginProcessOneWayRequests(oneWayRequests, callback, asyncState);
		}

		public void EndProcessOneWayRequests(IAsyncResult result)
		{
			Channel.EndProcessOneWayRequests(result);
		}

		public void ProcessOneWayRequestsAsync(OneWayRequest[] oneWayRequests, Action<AsyncCompletedEventArgs> processCompleted)
		{
			InvokeAsync(OnBeginProcessOneWayRequests, new object[] { oneWayRequests }, OnEndProcessOneWayRequests,
						OnProcessOneWayRequestsCompleted, processCompleted);
		}

		private IAsyncResult OnBeginProcessOneWayRequests(object[] inValues, AsyncCallback callback, object asyncState)
		{
			var requests = ((OneWayRequest[])(inValues[0]));
			return ((IAsyncRequestProcessor)(this)).BeginProcessOneWayRequests(requests, callback, asyncState);
		}

		private object[] OnEndProcessOneWayRequests(IAsyncResult result)
		{
			((IAsyncRequestProcessor)(this)).EndProcessOneWayRequests(result);
			return new object[0];
		}

		private void OnProcessOneWayRequestsCompleted(object state)
		{
			var e = ((InvokeAsyncCompletedEventArgs)(state));
			((Action<AsyncCompletedEventArgs>)(e.UserState)).Invoke(new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
		}

		private IAsyncResult OnBeginOpen(object[] inValues, AsyncCallback callback, object asyncState)
		{
			return ((ICommunicationObject)(this)).BeginOpen(callback, asyncState);
		}

		private object[] OnEndOpen(IAsyncResult result)
		{
			((ICommunicationObject)(this)).EndOpen(result);
			return null;
		}

		private void OnOpenCompleted(object state)
		{
			if ((OpenCompleted != null))
			{
				var e = ((InvokeAsyncCompletedEventArgs)(state));
				OpenCompleted(this, new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
			}
		}

		public void OpenAsync()
		{
			OpenAsync(null);
		}

		public void OpenAsync(object userState)
		{
			InvokeAsync(OnBeginOpen, null, OnEndOpen, OnOpenCompleted, userState);
		}

		private IAsyncResult OnBeginClose(object[] inValues, AsyncCallback callback, object asyncState)
		{
			return ((ICommunicationObject)(this)).BeginClose(callback, asyncState);
		}

		private object[] OnEndClose(IAsyncResult result)
		{
			((ICommunicationObject)(this)).EndClose(result);
			return null;
		}

		private void OnCloseCompleted(object state)
		{
			var e = ((InvokeAsyncCompletedEventArgs)(state));
			CloseCompleted(new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
		}

		public void CloseCompleted(AsyncCompletedEventArgs args)
		{
			if (args.Error != null)
			{
				Abort();
			}
		}

		public void CloseAsync()
		{
			CloseAsync(null);
		}

		public void CloseAsync(object userState)
		{
			InvokeAsync(OnBeginClose, null, OnEndClose, OnCloseCompleted, userState);
		}

		protected override IAsyncWcfRequestProcessor CreateChannel()
		{
			return new WcfRequestProcessorClientChannel(this);
		}

		private class WcfRequestProcessorClientChannel : ChannelBase<IAsyncWcfRequestProcessor>, IAsyncWcfRequestProcessor
		{
			public WcfRequestProcessorClientChannel(ClientBase<IAsyncWcfRequestProcessor> client) :
				base(client)
			{
			}

			public IAsyncResult BeginProcessRequests(Request[] requests, AsyncCallback callback, object asyncState)
			{
				var args = new object[1];
				args[0] = requests;
				IAsyncResult result = BeginInvoke("ProcessRequests", args, callback, asyncState);
				return result;
			}

			public Response[] EndProcessRequests(IAsyncResult result)
			{
				var args = new object[0];
				var theResult = ((Response[])(EndInvoke("ProcessRequests", args, result)));
				result.AsyncWaitHandle.Close();
				return theResult;
			}

			public void ProcessRequestsAsync(Request[] requests, Action<ProcessRequestsAsyncCompletedArgs> processCompleted)
			{
				throw new NotImplementedException();
			}

			public IAsyncResult BeginProcessOneWayRequests(OneWayRequest[] oneWayRequests, AsyncCallback callback, object asyncState)
			{
				var args = new object[1];
				args[0] = oneWayRequests;
				IAsyncResult result = BeginInvoke("ProcessOneWayRequests", args, callback, asyncState);
				return result;
			}

			public void EndProcessOneWayRequests(IAsyncResult result)
			{
				var args = new object[0];
				EndInvoke("ProcessOneWayRequests", args, result);
				result.AsyncWaitHandle.Close();
			}

			public void ProcessOneWayRequestsAsync(OneWayRequest[] oneWayRequests, Action<AsyncCompletedEventArgs> processCompleted)
			{
				throw new NotImplementedException();
			}
		}

		public void Dispose()
		{
			CloseAsync();
		}
	}
}