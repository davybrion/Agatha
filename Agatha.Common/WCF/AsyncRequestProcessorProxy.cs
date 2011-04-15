using System;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Agatha.Common.WCF
{
	public class AsyncRequestProcessorProxy : ClientBase<IAsyncWcfRequestProcessor>, IAsyncRequestProcessor
	{
		public event EventHandler<AsyncCompletedEventArgs> OpenCompleted;

		public AsyncRequestProcessorProxy() {}
		public AsyncRequestProcessorProxy(string endpointConfigurationName) : base(endpointConfigurationName) { }
		public AsyncRequestProcessorProxy(string endpointConfigurationName, string remoteAddress) : base(endpointConfigurationName, remoteAddress) { }
		public AsyncRequestProcessorProxy(string endpointConfigurationName, EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress) { }
		public AsyncRequestProcessorProxy(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress) { }

		IAsyncResult IAsyncRequestProcessor.BeginProcessRequests(Request[] requests, AsyncCallback callback, object asyncState)
		{
			return Channel.BeginProcessRequests(requests, callback, asyncState);
		}

		IAsyncResult IAsyncRequestProcessor.BeginProcessOneWayRequests(OneWayRequest[] oneWayRequests, AsyncCallback callback, object asyncState)
		{
			return Channel.BeginProcessOneWayRequests(oneWayRequests, callback, asyncState);
		}

		Response[] IAsyncRequestProcessor.EndProcessRequests(IAsyncResult result)
		{
			return Channel.EndProcessRequests(result);
		}

		void IAsyncRequestProcessor.EndProcessOneWayRequests(IAsyncResult result)
		{
			Channel.EndProcessOneWayRequests(result);
		}

		private IAsyncResult OnBeginProcessRequests(object[] inValues, AsyncCallback callback, object asyncState)
		{
			var requests = ((Request[])(inValues[0]));
			return ((IAsyncRequestProcessor)(this)).BeginProcessRequests(requests, callback, asyncState);
		}

		private IAsyncResult OnBeginProcessOneWayRequests(object[] inValues, AsyncCallback callback, object asyncState)
		{
			var requests = ((OneWayRequest[])(inValues[0]));
			return ((IAsyncRequestProcessor)(this)).BeginProcessOneWayRequests(requests, callback, asyncState);
		}

		private object[] OnEndProcessRequests(IAsyncResult result)
		{
			Response[] retVal = ((IAsyncRequestProcessor)(this)).EndProcessRequests(result);
			return new object[] { retVal };
		}

		private object[] OnEndProcessOneWayRequests(IAsyncResult result)
		{
			((IAsyncRequestProcessor)(this)).EndProcessOneWayRequests(result);
			return new object[0];
		}

		private void OnProcessRequestsCompleted(object state)
		{
			var e = ((InvokeAsyncCompletedEventArgs)(state));
			((Action<ProcessRequestsAsyncCompletedArgs>)(e.UserState)).Invoke(new ProcessRequestsAsyncCompletedArgs(e.Results, e.Error, e.Cancelled, e.UserState));
		}

		private void OnProcessOneWayRequestsCompleted(object state)
		{
			var e = ((InvokeAsyncCompletedEventArgs)(state));
			((Action<AsyncCompletedEventArgs>)(e.UserState)).Invoke(new ProcessRequestsAsyncCompletedArgs(e.Results, e.Error, e.Cancelled, e.UserState));
		}

		public void ProcessRequestsAsync(Request[] requests, Action<ProcessRequestsAsyncCompletedArgs> processCompleted)
		{
			InvokeAsync(OnBeginProcessRequests, new object[] { requests },
				OnEndProcessRequests, OnProcessRequestsCompleted, processCompleted);
		}

		public void ProcessOneWayRequestsAsync(OneWayRequest[] oneWayRequests, Action<AsyncCompletedEventArgs> processCompleted)
		{
			InvokeAsync(OnBeginProcessOneWayRequests, new object[] {oneWayRequests}, 
				OnEndProcessOneWayRequests, OnProcessOneWayRequestsCompleted, processCompleted);
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

		public void Dispose()
		{
			CloseAsync();
		}
	}
}