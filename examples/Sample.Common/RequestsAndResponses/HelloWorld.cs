using Agatha.Common;

namespace Sample.Common.RequestsAndResponses
{
	public class HelloWorldRequest : Request {}

	public class HelloWorldResponse : Response
	{
		public string Message { get; set; }
	}
}