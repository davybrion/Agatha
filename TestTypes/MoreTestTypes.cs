﻿using System.Threading.Tasks;
using Agatha.Common;
using Agatha.ServiceLayer;

namespace TestTypes
{
	public class RequestB : Request { }

	public class ResponseB : Response { }

	public class OneWayRequestB : OneWayRequest { }

	public class RequestHandlerB : RequestHandler<RequestB, ResponseB>
	{
		public override Response Handle(RequestB request)
		{
			return CreateTypedResponse();
		}

	    public override async Task<Response> HandleAsync(RequestB request)
	    {
	        return await Task.FromResult(CreateTypedResponse());
	    }
	}

	public class OneWayRequestHandlerB : OneWayRequestHandler<OneWayRequestB>
	{
		public override void Handle(OneWayRequestB request)
		{
		}
	}
}
