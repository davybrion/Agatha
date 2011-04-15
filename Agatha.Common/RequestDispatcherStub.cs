using System;
using System.Collections.Generic;
using System.Linq;

namespace Agatha.Common
{
	public class RequestDispatcherStub : RequestDispatcher
	{
		private readonly List<OneWayRequest> oneWayRequests = new List<OneWayRequest>();
		private readonly List<Response> responsesToReturn = new List<Response>();
		private readonly Dictionary<string, Request> keyToRequest = new Dictionary<string, Request>();

		public RequestDispatcherStub() : base(null, null) { }

		public void AddResponsesToReturn(params Response[] responses)
		{
			responsesToReturn.AddRange(responses);
		}

		public void AddResponsesToReturn(Dictionary<string, Response> keyedResponses)
		{
			responsesToReturn.AddRange(keyedResponses.Values);

			for (int i = 0; i < keyedResponses.Keys.Count; i++)
			{
				var key = keyedResponses.Keys.ElementAt(i);

				if (key != null)
				{
					keyToResultPositions.Add(key, i);
				}
			}
		}

		public void AddResponseToReturn(Response response)
		{
			responsesToReturn.Add(response);
		}

		public void AddResponseToReturn(string key, Response response)
		{
			responsesToReturn.Add(response);
			keyToResultPositions.Add(key, responsesToReturn.Count - 1);
		}

		public override void Clear()
		{
			// this Stub can't clear the state because we have to be able to inspect the sent requests
			// during our tests
		}

		public override void Add(string key, Request request)
		{
			base.Add(key, request);
			keyToRequest[key] = request;
		}

		public TRequest GetRequest<TRequest>() where TRequest : Request
		{
			return (TRequest)SentRequests.First(r => r.GetType().Equals(typeof(TRequest)));
		}

		public TRequest GetRequest<TRequest>(string key) where TRequest : Request
		{
			return (TRequest)keyToRequest[key];
		}

		public bool HasRequest<TRequest>() where TRequest : Request
		{
			return SentRequests.Count(r => r.GetType().Equals(typeof(TRequest))) > 0;
		}

		public bool HasOneWayRequest<TOneWayRequest>() where TOneWayRequest : OneWayRequest
		{
			return oneWayRequests.Any(r => r.GetType() == typeof(TOneWayRequest));
		}

		public TOneWayRequest GetOneWayRequest<TOneWayRequest>() where TOneWayRequest : OneWayRequest
		{
			if (oneWayRequests.Count(r => r.GetType() == typeof(TOneWayRequest)) > 0)
			{
				throw new InvalidOperationException(string.Format("Multiple OneWayRequests of type {0} have been added, use the GetOneWayRequests method instead to perform your inspection", typeof(TOneWayRequest)));
			}

			return oneWayRequests.FirstOrDefault(r => r.GetType() == typeof(TOneWayRequest)) as TOneWayRequest;
		}

		public IEnumerable<OneWayRequest> GetOneWayRequests()
		{
			return oneWayRequests;
		}

		protected override Response[] GetResponses(Request[] requestsToProcess)
		{
			return responsesToReturn.ToArray();
		}

		public override void Send(params OneWayRequest[] oneWayRequests)
		{
			this.oneWayRequests.AddRange(oneWayRequests);
			base.Send(oneWayRequests);
		}
	}
}