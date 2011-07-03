using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Agatha.Common.Caching;
using Agatha.Common.InversionOfControl;

namespace Agatha.Common
{
	/// <summary>
	/// Do not use this type directly, use it via an IAsyncRequestDispatcherFactory
	/// </summary>
	public interface IAsyncRequestDispatcher : IDisposable
	{
		void Add(Request request);
		void Add<TRequest>(Action<TRequest> action) where TRequest : Request, new();
		void Add(params Request[] requestsToAdd);
		void Add(string key, Request request);
		void Add(params OneWayRequest[] oneWayRequests);
		void ProcessOneWayRequests();
		void ProcessRequests(Action<ReceivedResponses> receivedResponsesDelegate, Action<ExceptionInfo> exceptionOccurredDelegate);
		void ProcessRequests(Action<ReceivedResponses> receivedResponsesDelegate, Action<ExceptionInfo, ExceptionType> exceptionAndTypeOccurredDelegate);
	}

	// TODO: make sure that OneWayRequests can't be added through the Add methods

	public class AsyncRequestDispatcher : Disposable, IAsyncRequestDispatcher
	{
		private readonly IAsyncRequestProcessor requestProcessor;
	    private readonly ICacheManager cacheManager;
	    protected Dictionary<string, int> keyToResultPositions;
		private Dictionary<string, Type> keyToTypes;

		private bool oneWayRequestsAdded;
		private bool twoWayRequestsAdded;

		private List<Request> queuedRequests;
		private List<OneWayRequest> queuedOneWayRequests;

		public AsyncRequestDispatcher(IAsyncRequestProcessor requestProcessor, ICacheManager cacheManager)
		{
			this.requestProcessor = requestProcessor;
		    this.cacheManager = cacheManager;
		    InitializeState();
		}

		public virtual Request[] QueuedRequests
		{
			get { return queuedRequests.ToArray(); }
		}

		public virtual void Add(params Request[] requestsToAdd)
		{
			foreach (var request in requestsToAdd)
			{
				Add(request);
			}
		}

		public virtual void Add(string key, Request request)
		{
			AddRequest(request, true);
			keyToTypes[key] = request.GetType();
			keyToResultPositions[key] = queuedRequests.Count - 1;
		}

		public virtual void Add<TRequest>(Action<TRequest> action) where TRequest : Request, new()
		{
			var request = new TRequest();
			action(request);
			Add(request);
		}

		public virtual void Add(Request request)
		{
			AddRequest(request, false);
		}

		public virtual void Add(params OneWayRequest[] oneWayRequests)
		{
			EnsureWeOnlyHaveOneWayRequests();
			queuedOneWayRequests.AddRange(oneWayRequests);
		}

		public virtual void ProcessOneWayRequests()
		{
			var requests = queuedOneWayRequests.ToArray();
			BeforeSendingRequests(requests);
			requestProcessor.ProcessOneWayRequestsAsync(requests, OnProcessOneWayRequestsCompleted);
			AfterSendingRequests(requests);
			queuedOneWayRequests.Clear();
		}

		private void OnProcessOneWayRequestsCompleted(AsyncCompletedEventArgs args)
		{
			Dispose();

			if (args.Error != null)
			{
				throw new InvalidOperationException("Exception occurred during processing of one-way requests", args.Error);
			}
		}

		public virtual void ProcessRequests(Action<ReceivedResponses> receivedResponsesDelegate, Action<ExceptionInfo> exceptionOccurredDelegate)
		{
			ProcessRequests(new ResponseReceiver(receivedResponsesDelegate, exceptionOccurredDelegate, keyToResultPositions, cacheManager));
		}

		public virtual void ProcessRequests(Action<ReceivedResponses> receivedResponsesDelegate, Action<ExceptionInfo, ExceptionType> exceptionAndTypeOccurredDelegate)
		{
			ProcessRequests(new ResponseReceiver(receivedResponsesDelegate, exceptionAndTypeOccurredDelegate, keyToResultPositions, cacheManager));
		}

		private void ProcessRequests(ResponseReceiver responseReciever)
		{
            var requestsToProcess = queuedRequests.ToArray();
            
            BeforeSendingRequests(requestsToProcess);
            
            var tempResponseArray = new Response[requestsToProcess.Length];
            var requestsToSend = new List<Request>(requestsToProcess);

            GetCachedResponsesAndRemoveThoseRequests(requestsToProcess, tempResponseArray, requestsToSend);
            var requestsToSendAsArray = requestsToSend.ToArray();

            if (requestsToSendAsArray.Length > 0)
            {
                requestProcessor.ProcessRequestsAsync(requestsToSendAsArray, 
                    a => OnProcessRequestsCompleted(a, responseReciever, tempResponseArray, requestsToSendAsArray));
            }
            else
            {
                var synchronizationContext = SynchronizationContext.Current ?? new SynchronizationContext();
                synchronizationContext.Post(s => OnProcessRequestsCompleted(null, responseReciever, tempResponseArray, requestsToSendAsArray), null);
            }
            
            AfterSendingRequests(requestsToProcess);
		}

        private void GetCachedResponsesAndRemoveThoseRequests(Request[] requestsToProcess, Response[] tempResponseArray, List<Request> requestsToSend)
        {
            for (int i = 0; i < requestsToProcess.Length; i++)
            {
                var request = requestsToProcess[i];

                if (cacheManager.IsCachingEnabledFor(request.GetType()))
                {
                    var cachedResponse = cacheManager.GetCachedResponseFor(request);

                    if (cachedResponse != null)
                    {
                        tempResponseArray[i] = cachedResponse;
                        requestsToSend.Remove(request);
                    }
                }
            }
        }

		protected virtual void BeforeSendingRequests(IEnumerable<Request> requestsToProcess) {}
		protected virtual void AfterSendingRequests(IEnumerable<Request> sentRequests) {}

		public virtual void OnProcessRequestsCompleted(ProcessRequestsAsyncCompletedArgs args, ResponseReceiver responseReciever, 
            Response[] tempResponseArray, Request[] requestsToSendAsArray)
		{
			Dispose();
			responseReciever.ReceiveResponses(args, tempResponseArray, requestsToSendAsArray);
		}

		protected override void DisposeManagedResources()
		{
			// WHY: this is really only important for containers that require explicit release of disposable components (Castle)... if
			// people use the AsyncRequestDispatcherFactory to create instances of this class in combination with Castle Windsor, these
			// instances need to be released explicitly because they have been resolved through the container
			if (IoC.Container != null)
			{
				IoC.Container.Release(this);
			}

			if (requestProcessor != null) requestProcessor.Dispose();
		}

		private void AddRequest(Request request, bool wasAddedWithKey)
		{
			EnsureWeOnlyHaveTwoWayRequests();
			
			Type requestType = request.GetType();

			if (RequestTypeIsAlreadyPresent(requestType) &&
				(RequestTypeIsNotAssociatedWithKey(requestType) || !wasAddedWithKey))
			{
				throw new InvalidOperationException(String.Format("A request of type {0} has already been added. "
																  + "Please add requests of the same type with a different key.", requestType.FullName));
			}

			queuedRequests.Add(request);
		}

		private bool RequestTypeIsAlreadyPresent(Type requestType)
		{
			return QueuedRequests.Any(r => r.GetType().Equals(requestType));
		}

		private bool RequestTypeIsNotAssociatedWithKey(Type requestType)
		{
			return !keyToTypes.Values.Contains(requestType);
		}

		private void InitializeState()
		{
			queuedRequests = new List<Request>();
			queuedOneWayRequests = new List<OneWayRequest>();
			keyToTypes = new Dictionary<string, Type>();
			keyToResultPositions = new Dictionary<string, int>();
		}

		private void EnsureWeOnlyHaveOneWayRequests()
		{
			if (twoWayRequestsAdded)
			{
				ThrowInvalidUsageException();
			}

			oneWayRequestsAdded = true;
		}

		private void EnsureWeOnlyHaveTwoWayRequests()
		{
			if (oneWayRequestsAdded)
			{
				ThrowInvalidUsageException();
			}

			twoWayRequestsAdded = true;
		}

		private void ThrowInvalidUsageException()
		{
			throw new InvalidOperationException("You cannot combine one-way and two-way requests in the same AsyncRequestDispatcher.");
		}
	}
}