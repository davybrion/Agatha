using System;
using System.Collections.Generic;
using System.Reflection;
using Agatha.Common;
using Agatha.Common.Caching;
using Agatha.Common.InversionOfControl;

namespace Agatha.ServiceLayer
{
	public class ServiceLayerAndClientConfiguration
	{
		private readonly List<Assembly> requestHandlerAssemblies = new List<Assembly>();
		private readonly List<Assembly> requestsAndResponseAssemblies = new List<Assembly>();
		private readonly IContainer container;
		private ServiceLayerConfiguration serviceLayerConfiguration;

		public Type RequestProcessorImplementation { get; set; }
		public Type AsyncRequestProcessorImplementation { get; set; }
		public Type CacheManagerImplementation { get; set; }
		public Type CacheProviderImplementation { get; set; }
	    public Type ContainerImplementation { get; private set; }
		public Type BusinessExceptionType { get; set; }
		public Type SecurityExceptionType { get; set; }

		public Type RequestDispatcherImplementation { get; set; }
		public Type RequestDispatcherFactoryImplementation { get; set; }
		public Type AsyncRequestDispatcherImplementation { get; set; }
		public Type AsyncRequestDispatcherFactoryImplementation { get; set; }

		public ServiceLayerAndClientConfiguration(IContainer container)
		{
			this.container = container;
			SetDefaultImplementations();
		}

		public ServiceLayerAndClientConfiguration(Type containerImplementation)
		{
			ContainerImplementation = containerImplementation;
			SetDefaultImplementations();
		}

		public ServiceLayerAndClientConfiguration(Assembly requestHandlersAssembly, Assembly requestsAndResponsesAssembly, IContainer container)
			: this(container)
		{
			AddRequestHandlerAssembly(requestHandlersAssembly);
			AddRequestAndResponseAssembly(requestsAndResponsesAssembly);
		}

		public ServiceLayerAndClientConfiguration(Assembly requestHandlersAssembly, Assembly requestsAndResponsesAssembly, Type containerImplementation)
			: this(containerImplementation)
		{
			AddRequestHandlerAssembly(requestHandlersAssembly);
			AddRequestAndResponseAssembly(requestsAndResponsesAssembly);
		}

		public ServiceLayerAndClientConfiguration AddRequestHandlerAssembly(Assembly assembly)
		{
			requestHandlerAssemblies.Add(assembly);
			return this;
		}

		public ServiceLayerAndClientConfiguration AddRequestAndResponseAssembly(Assembly assembly)
		{
			requestsAndResponseAssemblies.Add(assembly);
			return this;
		}

		private void SetDefaultImplementations()
		{
			RequestDispatcherImplementation = typeof(RequestDispatcher);
			RequestDispatcherFactoryImplementation = typeof(RequestDispatcherFactory);
			RequestProcessorImplementation = typeof(RequestProcessor);
			AsyncRequestDispatcherImplementation = typeof(AsyncRequestDispatcher);
			AsyncRequestDispatcherFactoryImplementation = typeof(AsyncRequestDispatcherFactory);
			AsyncRequestProcessorImplementation = typeof(AsyncRequestProcessor);
			CacheManagerImplementation = typeof(CacheManager);
			CacheProviderImplementation = typeof(InMemoryCacheProvider);

			IoC.Container = container ?? (IContainer)Activator.CreateInstance(ContainerImplementation);
		    serviceLayerConfiguration = new ServiceLayerConfiguration(IoC.Container);
		}

		public void Initialize()
		{
		    serviceLayerConfiguration.AsyncRequestProcessorImplementation = AsyncRequestProcessorImplementation;
		    serviceLayerConfiguration.BusinessExceptionType = BusinessExceptionType;
		    serviceLayerConfiguration.RequestProcessorImplementation = RequestProcessorImplementation;
		    serviceLayerConfiguration.SecurityExceptionType = SecurityExceptionType;
		    serviceLayerConfiguration.CacheManagerImplementation = CacheManagerImplementation;
		    serviceLayerConfiguration.CacheProviderImplementation = CacheProviderImplementation;

			foreach (var assembly in requestHandlerAssemblies)
				serviceLayerConfiguration.AddRequestHandlerAssembly(assembly);

			foreach (var assembly in requestsAndResponseAssemblies)
				serviceLayerConfiguration.AddRequestAndResponseAssembly(assembly);

			serviceLayerConfiguration.Initialize();

			IoC.Container.Register(typeof(IRequestDispatcher), RequestDispatcherImplementation, Lifestyle.Transient);
			IoC.Container.Register(typeof(IRequestDispatcherFactory), RequestDispatcherFactoryImplementation, Lifestyle.Singleton);
			IoC.Container.Register(typeof(IAsyncRequestDispatcher), AsyncRequestDispatcherImplementation, Lifestyle.Transient);
			IoC.Container.Register(typeof(IAsyncRequestDispatcherFactory), AsyncRequestDispatcherFactoryImplementation, Lifestyle.Singleton);
		}

         public ServiceLayerAndClientConfiguration RegisterRequestHandlerInterceptor<T>() where T : IRequestHandlerInterceptor
         {
             serviceLayerConfiguration.RegisterRequestHandlerInterceptor<T>();
             return this;
         }

         public ServiceLayerAndClientConfiguration Use<TConventions>() where TConventions : IConventions
         {
             serviceLayerConfiguration.Use<TConventions>();
             return this;
         }
	}
}