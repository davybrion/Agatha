using Agatha.Common.InversionOfControl;

namespace Agatha.Common
{
	public interface IAsyncRequestDispatcherFactory
	{
		IAsyncRequestDispatcher CreateAsyncRequestDispatcher();
	}

	public class AsyncRequestDispatcherFactory : IAsyncRequestDispatcherFactory
	{
		public IAsyncRequestDispatcher CreateAsyncRequestDispatcher()
		{
			return IoC.Container.Resolve<IAsyncRequestDispatcher>();
		}
	}
}