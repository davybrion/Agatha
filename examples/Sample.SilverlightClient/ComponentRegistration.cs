using Agatha.Common;
using Sample.Common.RequestsAndResponses;

namespace Sample.SilverlightClient
{
	public static class ComponentRegistration
	{
		public static void Register()
		{
			//new ClientConfiguration(typeof(HelloWorldRequest).Assembly, typeof(Agatha.Unity.Container)).Initialize();
			new ClientConfiguration(typeof(HelloWorldRequest).Assembly, typeof(Agatha.Castle.Container)).Initialize();
			//new ClientConfiguration(typeof(HelloWorldRequest).Assembly, typeof(Agatha.Ninject.Container)).Initialize();
		}
	}
}