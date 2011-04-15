using System.Reflection;
using Agatha.ServiceLayer;
using Sample.Common.RequestsAndResponses;

namespace Sample.ServiceLayer
{
	public static class ComponentRegistration
	{
		public static void Register()
		{
			// NOTE: i'm using Castle Windsor as the IOC container for this sample... if you would want Agatha to use the same container instance that the rest
			// of your code is using, check this out: http://davybrion.com/blog/2009/11/integrating-your-ioc-container-with-agatha/

			//new ServiceLayerConfiguration(Assembly.GetExecutingAssembly(), typeof(HelloWorldRequest).Assembly, typeof(Agatha.Castle.Container)).Initialize();
			//new ServiceLayerConfiguration(Assembly.GetExecutingAssembly(), typeof(HelloWorldRequest).Assembly, typeof(Agatha.StructureMap.Container)).Initialize();

			//var config = new ServiceLayerConfiguration(Assembly.GetExecutingAssembly(), typeof(HelloWorldRequest).Assembly, typeof(Agatha.Castle.Container))
			//                {
			//                    RequestProcessorImplementation = typeof(PerformanceLoggingRequestProcessor)
			//                };
			//config.Initialize();

			//new ServiceLayerConfiguration(Assembly.GetExecutingAssembly(), typeof(HelloWorldRequest).Assembly, typeof(Agatha.Unity.Container)).Initialize();
			//new ServiceLayerConfiguration(Assembly.GetExecutingAssembly(), typeof(HelloWorldRequest).Assembly, typeof(Agatha.Ninject.Container)).Initialize();
            new ServiceLayerConfiguration(Assembly.GetExecutingAssembly(), typeof(HelloWorldRequest).Assembly, typeof(Agatha.Spring.Container)).Initialize();
		}
	}
}