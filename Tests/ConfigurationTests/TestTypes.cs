using System;
using System.Threading.Tasks;
using Agatha.Common;
using Agatha.ServiceLayer;

namespace Tests.ConfigurationTests
{
	public class RequestA : Request {}

	public class ResponseA : Response {}

	public class OneWayRequestA : OneWayRequest {}

	public class RequestHandlerA : RequestHandler<RequestA, ResponseA>
	{
		public override Response Handle(RequestA request)
		{
			return CreateTypedResponse();
		}

	    public override Task<Response> HandleAsync(RequestA request)
	    {
	        throw new NotImplementedException();
	    }
	}

	public class OneWayRequestHandlerA : OneWayRequestHandler<OneWayRequestA>
	{
		public override void Handle(OneWayRequestA request)
		{
		}
	}

	public class RequestX : Request {}

	public class ResponseX : Response {}

	public abstract class RequestHandlerBase<T> : RequestHandler<RequestX, ResponseX>
	{
	}

	public class ConcreteRequestHandler : RequestHandlerBase<int>
	{
		public override Response Handle(RequestX request)
		{
			return CreateTypedResponse();
		}

	    public override Task<Response> HandleAsync(RequestX request)
	    {
	        throw new NotImplementedException();
	    }
	}
}