namespace Agatha.Common
{
	public class RequestDispatcherFactoryStub : IRequestDispatcherFactory
	{
		private readonly RequestDispatcherStub requestDispatcherStub;

		public RequestDispatcherFactoryStub(RequestDispatcherStub requestDispatcherStub)
		{
			this.requestDispatcherStub = requestDispatcherStub;
		}

		public IRequestDispatcher CreateRequestDispatcher()
		{
			return requestDispatcherStub;
		}
	}
}