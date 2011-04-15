using Agatha.Common.InversionOfControl;

namespace Agatha.Common
{
	public interface IRequestDispatcherFactory
	{
		IRequestDispatcher CreateRequestDispatcher();
	}

	public class RequestDispatcherFactory : IRequestDispatcherFactory
	{
		public IRequestDispatcher CreateRequestDispatcher()
		{
			return IoC.Container.Resolve<IRequestDispatcher>();
		}
	}
}