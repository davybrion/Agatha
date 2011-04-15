using System;
using System.ComponentModel;
using Agatha.Common;

namespace Agatha.ServiceLayer
{
	public class AsyncRequestProcessor : Disposable, IAsyncRequestProcessor
	{
		private readonly IRequestProcessor requestProcessor;

		public AsyncRequestProcessor(IRequestProcessor requestProcessor)
		{
			this.requestProcessor = requestProcessor;
		}

		protected override void DisposeManagedResources()
		{
			if (requestProcessor != null) requestProcessor.Dispose();
		}

		public IAsyncResult BeginProcessRequests(Request[] requests, AsyncCallback callback, object asyncState)
		{
            throw new NotSupportedException();
		}

		public Response[] EndProcessRequests(IAsyncResult result)
		{
            throw new NotSupportedException();
		}

		public void ProcessRequestsAsync(Request[] requests, Action<ProcessRequestsAsyncCompletedArgs> callback)
		{
		    var worker = new BackgroundWorker();
		    worker.DoWork += (sender, args) => args.Result = requestProcessor.Process(requests);
		    worker.RunWorkerCompleted += (sender, args) =>
		                                     {
                                                 if(args.Error == null)
                                                 {
                                                     var responses = (Response[]) args.Result;
                                                     callback(
                                                         new ProcessRequestsAsyncCompletedArgs(new object[] {responses},
                                                                                               null, false, null));
                                                 }
                                                 else
                                                 {
                                                     callback(new ProcessRequestsAsyncCompletedArgs(null, args.Error,
                                                                                                    false, null));
                                                 }
		                                     };
            worker.RunWorkerAsync();
		}

		public IAsyncResult BeginProcessOneWayRequests(OneWayRequest[] requests, AsyncCallback callback, object asyncState)
		{
            throw new NotSupportedException();
		}

		public void EndProcessOneWayRequests(IAsyncResult result)
		{
            throw new NotSupportedException();
		}

		public void ProcessOneWayRequestsAsync(OneWayRequest[] requests, Action<AsyncCompletedEventArgs> callback)
		{
            var worker = new BackgroundWorker();
            worker.DoWork += (sender, args) => args.Result = requestProcessor.Process(requests);
            worker.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error == null)
                {
                    callback(new AsyncCompletedEventArgs(null, false, null));
                }
                else
                {
                    callback(new AsyncCompletedEventArgs(args.Error, false, null));
                }
            };
            worker.RunWorkerAsync();
		}
	}
}
