namespace Agatha.Common
{
	public class AsyncRequestDispatcherFactoryStub : IAsyncRequestDispatcherFactory
	{
		private readonly AsyncRequestDispatcherStub asyncRequestDispatcherStub;

		public AsyncRequestDispatcherFactoryStub(AsyncRequestDispatcherStub asyncRequestDispatcherStub)
		{
			this.asyncRequestDispatcherStub = asyncRequestDispatcherStub;
		}

		public IAsyncRequestDispatcher CreateAsyncRequestDispatcher()
		{
			return asyncRequestDispatcherStub;
		}
	}
}