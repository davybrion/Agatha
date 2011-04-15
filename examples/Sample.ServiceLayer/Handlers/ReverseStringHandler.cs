using System;
using Agatha.Common;
using Agatha.ServiceLayer;
using Sample.Common.RequestsAndResponses;

namespace Sample.ServiceLayer.Handlers
{
	public class ReverseStringHandler : RequestHandler<ReverseStringRequest, ReverseStringResponse>
	{
		public override Response Handle(ReverseStringRequest request)
		{
			var response = CreateTypedResponse();

			// not the best way to reverse a string, but it's easy... and we're caching this anyway! ;)
			var charArray = request.StringToReverse.ToCharArray();
			Array.Reverse(charArray);
			response.ReversedString = new string(charArray);

			return response;
		}
	}
}