using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Agatha.Common;
using Agatha.Common.Caching;
using Agatha.Common.Caching.Timers;
using Agatha.Common.InversionOfControl;
using Agatha.Common.WCF;

namespace Agatha.ServiceLayer
{
	public class ServiceLayerConfiguration
	{
		private readonly List<Assembly> requestHandlerAssemblies = new List<Assembly>();
		private readonly List<Assembly> requestsAndResponseAssemblies = new List<Assembly>();
		private readonly IContainer container;

		public Type RequestProcessorImplementation { get; set; }
		public Type AsyncRequestProcessorImplementation { get; set; }
		public Type CacheManagerImplementation { get; set; }
		public Type CacheProviderImplementation { get; set; }
		public IContainer Container { get; private set; }
		public Type ContainerImplementation { get; private set; }

		public Type BusinessExceptionType { get; set; }
		public Type SecurityExceptionType { get; set; }

		public ServiceLayerConfiguration(IContainer container)
		{
			this.container = container;
			SetDefaultImplementations();
		}

		public ServiceLayerConfiguration(Type containerImplementation)
		{
			ContainerImplementation = containerImplementation;
			SetDefaultImplementations();
		}

		public ServiceLayerConfiguration(Assembly requestHandlersAssembly, Assembly requestsAndResponsesAssembly, IContainer container)
			: this(container)
		{
			AddRequestHandlerAssembly(requestHandlersAssembly);
			AddRequestAndResponseAssembly(requestsAndResponsesAssembly);
		}

		public ServiceLayerConfiguration(Assembly requestHandlersAssembly, Assembly requestsAndResponsesAssembly, Type containerImplementation)
			: this(containerImplementation)
		{
			AddRequestHandlerAssembly(requestHandlersAssembly);
			AddRequestAndResponseAssembly(requestsAndResponsesAssembly);
		}

		public ServiceLayerConfiguration AddRequestHandlerAssembly(Assembly assembly)
		{
			requestHandlerAssemblies.Add(assembly);
			return this;
		}

		public ServiceLayerConfiguration AddRequestAndResponseAssembly(Assembly assembly)
		{
			requestsAndResponseAssemblies.Add(assembly);
			return this;
		}

		private void SetDefaultImplementations()
		{
			RequestProcessorImplementation = typeof(RequestProcessor);
			AsyncRequestProcessorImplementation = typeof(AsyncRequestProcessor);
			CacheManagerImplementation = typeof(CacheManager);
			CacheProviderImplementation = typeof(InMemoryCacheProvider);
		}

		public void Initialize()
		{
			if (IoC.Container == null)
			{
				IoC.Container = container ?? (IContainer)Activator.CreateInstance(ContainerImplementation);
			}

			IoC.Container.RegisterInstance(this);
			IoC.Container.Register(typeof(IRequestProcessor), RequestProcessorImplementation, Lifestyle.Transient);
			IoC.Container.Register(typeof(IAsyncRequestProcessor), AsyncRequestProcessorImplementation, Lifestyle.Transient);
			IoC.Container.Register(typeof(ICacheProvider), CacheProviderImplementation, Lifestyle.Singleton);
			IoC.Container.Register(typeof(ICacheManager), CacheManagerImplementation, Lifestyle.Singleton);
			IoC.Container.Register<ITimerProvider, TimerProvider>(Lifestyle.Singleton);
			RegisterRequestAndResponseTypes();
			RegisterRequestHandlers();
			ConfigureCachingLayer();
		}

		private void ConfigureCachingLayer()
		{
			var requestTypes = requestsAndResponseAssemblies.SelectMany(a => a.GetTypes()).Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Request)));
			var cacheConfiguration = new ServiceCacheConfiguration(requestTypes);
			IoC.Container.RegisterInstance<CacheConfiguration>(cacheConfiguration);
		}

		private void RegisterRequestAndResponseTypes()
		{
			foreach (var assembly in requestsAndResponseAssemblies)
			{
				KnownTypeProvider.RegisterDerivedTypesOf<Request>(assembly);
				KnownTypeProvider.RegisterDerivedTypesOf<Response>(assembly);
			}
		}

		private void RegisterRequestHandlers()
		{
			var oneWayHandlerType = typeof(OneWayRequestHandler);
			var openOneWayHandlerType = typeof(IOneWayRequestHandler<>);
			var requestResponseHandlerType = typeof(RequestHandler);
			var openRequestReponseHandlerType = typeof(IRequestHandler<>);

			foreach (var assembly in requestHandlerAssemblies)
			{
				foreach (var type in assembly.GetTypes())
				{
					if (type.IsAbstract)
						continue;

					if (!type.IsSubclassOf(oneWayHandlerType) && !type.IsSubclassOf(requestResponseHandlerType))
						continue;

					var requestType = GetRequestType(type);

					if (requestType != null)
					{
						Type handlerType = null;
						if (type.IsSubclassOf(oneWayHandlerType))
						{
							handlerType = openOneWayHandlerType.MakeGenericType(requestType);
						}
						else if (type.IsSubclassOf(requestResponseHandlerType))
						{
							handlerType = openRequestReponseHandlerType.MakeGenericType(requestType);
						}

						if (handlerType != null)
						{
							IoC.Container.Register(handlerType, type, Lifestyle.Transient);
						}
					}
				}
			}
		}

		private static Type GetRequestType(Type type)
		{
			var interfaceType = type.GetInterfaces().FirstOrDefault(i => i.Name.StartsWith("IRequestHandler`") || i.Name.StartsWith("IOneWayRequestHandler`"));

			if (interfaceType == null || interfaceType.GetGenericArguments().Length == 0)
			{
				return null;
			}

			return GetFirstGenericTypeArgument(interfaceType);
		}

		private static Type GetFirstGenericTypeArgument(Type type)
		{
			return type.GetGenericArguments()[0];
		}
	}
}