using System.Collections.Generic;
using System.Linq;

namespace Agatha.Common
{
	/// <summary>
	/// Originally written by Tom Ceulemans
	/// </summary>
	public class ReceivedResponses
	{
		private readonly Response[] responses;
		private readonly Dictionary<string, int> keyToResultPositions;

		public ReceivedResponses(Response[] responses)
			: this(responses, new Dictionary<string, int>())
		{ }

		public ReceivedResponses(Response[] responses, Dictionary<string, int> keyToResultPositions)
		{
			this.responses = responses;
			this.keyToResultPositions = keyToResultPositions;
		}

		public IEnumerable<Response> Responses
		{
			get { return responses;  }
		}

		public virtual TResponse Get<TResponse>() where TResponse : Response
		{
			var responseType = typeof(TResponse);
			return (TResponse)responses.Single(r => r.GetType().Equals(responseType));
		}

		public virtual TResponse Get<TResponse>(string key) where TResponse : Response
		{
			return (TResponse)responses[keyToResultPositions[key]];
		}

		public virtual bool HasResponse<TResponse>() where TResponse : Response
		{
			return responses.OfType<TResponse>().Any();
		}
	}
}