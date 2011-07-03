using System;
using System.Collections.Generic;
using System.Linq;

namespace Agatha.Common
{
	public class AsyncRequestDispatcherStub : Disposable, IAsyncRequestDispatcher
	{
		private readonly Dictionary<Type, string> unkeyedTypesToAutoKey;
		private readonly Dictionary<string, Request> requests;
		private readonly Dictionary<string, int> responseKeyToIndexPosition;
		private readonly List<Response> responsesToReturn;
		private readonly List<OneWayRequest> oneWayRequests;
		private ResponseReceiver responseReceiver;
		private bool twoWayRequestsAdded;
		private bool oneWayRequestsAdded;

		public AsyncRequestDispatcherStub()
		{
			unkeyedTypesToAutoKey = new Dictionary<Type, string>();
			requests = new Dictionary<string, Request>();
			responseKeyToIndexPosition = new Dictionary<string, int>();
			responsesToReturn = new List<Response>();
			oneWayRequests = new List<OneWayRequest>();
		}

		public void SetResponsesToReturn(params Response[] responses)
		{
			responsesToReturn.Clear();
			responsesToReturn.AddRange(responses);
		}

		public void AddResponseToReturn(Response response, string key)
		{
			responsesToReturn.Add(response);
			responseKeyToIndexPosition.Add(key, responsesToReturn.Count - 1);
		}

		public bool HasRequest<TRequest>() where TRequest : Request
		{
			return unkeyedTypesToAutoKey.ContainsKey(typeof(TRequest));
		}

		public bool HasRequest<TRequest>(string key) where TRequest : Request
		{
			return requests.ContainsKey(key) && (requests[key] is TRequest);
		}

		public bool HasOneWayRequest<TOneWayRequest>() where TOneWayRequest : OneWayRequest
		{
			return oneWayRequests.Any(r => r.GetType() == typeof(TOneWayRequest));
		}

		public TRequest GetRequest<TRequest>() where TRequest : Request
		{
			var autoKey = unkeyedTypesToAutoKey[typeof(TRequest)];
			return (TRequest)requests[autoKey];
		}

		public TRequest GetRequest<TRequest>(string key) where TRequest : Request
		{
			return (TRequest)requests[key];
		}

		public TOneWayRequest GetOneWayRequest<TOneWayRequest>() where TOneWayRequest : OneWayRequest
		{
			if (oneWayRequests.Any(r => r.GetType() == typeof(TOneWayRequest)))
			{
				throw new InvalidOperationException(string.Format("Multiple OneWayRequests of type {0} have been added, use the GetOneWayRequests method instead to perform your inspection", typeof(TOneWayRequest)));
			}

			return oneWayRequests.FirstOrDefault(r => r.GetType() == typeof(TOneWayRequest)) as TOneWayRequest;
		}

		public IEnumerable<OneWayRequest> GetOneWayRequests()
		{
			return oneWayRequests;
		}

		public void ClearRequests()
		{
			unkeyedTypesToAutoKey.Clear();
			requests.Clear();
		}

		public void Add(Request request)
		{
			EnsureWeOnlyHaveTwoWayRequests();
			var autoKey = Guid.NewGuid().ToString();
			unkeyedTypesToAutoKey.Add(request.GetType(), autoKey);
			requests.Add(autoKey, request);
		}

		public void Add(params Request[] requestsToAdd)
		{
			if (requestsToAdd != null)
			{
				foreach (var request in requestsToAdd)
				{
					Add(request);
				}
			}
		}

		public void Add<TRequest>(Action<TRequest> action) where TRequest : Request, new()
		{
			var request = new TRequest();
			action(request);
			Add(request);
		}

		public void Add(string key, Request request)
		{
			EnsureWeOnlyHaveTwoWayRequests();
			requests.Add(key, request);
		}

		public void Add(params OneWayRequest[] oneWayRequests)
		{
			EnsureWeOnlyHaveOneWayRequests();
			this.oneWayRequests.AddRange(oneWayRequests);
		}

		public void ProcessOneWayRequests()
		{
			//wanted to send it to the RequestProcessor, but none available..
		}

		public void ProcessRequests(Action<ReceivedResponses> receivedResponsesDelegate, Action<ExceptionInfo> exceptionOccurredDelegate)
		{
			ProcessRequests(new ResponseReceiver(receivedResponsesDelegate, exceptionOccurredDelegate, responseKeyToIndexPosition, null));
		}

		public void ProcessRequests(Action<ReceivedResponses> receivedResponsesDelegate, Action<ExceptionInfo, ExceptionType> exceptionAndTypeOccurredDelegate)
		{
			ProcessRequests(new ResponseReceiver(receivedResponsesDelegate, exceptionAndTypeOccurredDelegate, responseKeyToIndexPosition, null));
		}

		private void ProcessRequests(ResponseReceiver responseReceiver)
		{
			this.responseReceiver = responseReceiver;
		}

		public void ReturnResponses()
		{
            responseReceiver.ReceiveResponses(new ProcessRequestsAsyncCompletedArgs(new[] { responsesToReturn.ToArray() }, null, false, null), new Response[responsesToReturn.Count], requests.Values.ToArray());
		}

		protected override void DisposeManagedResources() { }

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