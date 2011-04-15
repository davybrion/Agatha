using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Agatha.Common.Caching;
using Agatha.Common.Caching.Timers;
using Agatha.Common.InversionOfControl;
using Agatha.Common.WCF;

namespace Agatha.Common
{
	public class ClientConfiguration
	{
		private readonly List<Assembly> requestsAndResponseAssemblies = new List<Assembly>();
		private readonly IContainer container;

		public Type AsyncRequestDispatcherImplementation { get; set; }
		public Type AsyncRequestDispatcherFactoryImplementation { get; set; }
		public Type RequestProcessorImplementation { get; set; }
		public Type ContainerImplementation { get; private set; }
		public Type CacheProviderImplementation { get; set; }
		public Type CacheManagerImplementation { get; set; }

		public ClientConfiguration(IContainer container)
		{
			this.container = container;
			SetDefaultImplementations();
		}

		public ClientConfiguration(Type containerImplementation)
		{
			ContainerImplementation = containerImplementation;
			SetDefaultImplementations();
		}

		public ClientConfiguration(Assembly requestsAndResponsesAssembly, IContainer container)
			: this(container)
		{
			AddRequestAndResponseAssembly(requestsAndResponsesAssembly);
		}

		public ClientConfiguration(Assembly requestsAndResponsesAssembly, Type containerImplementation)
			: this(containerImplementation)
		{
			AddRequestAndResponseAssembly(requestsAndResponsesAssembly);
		}

		public ClientConfiguration AddRequestAndResponseAssembly(Assembly assembly)
		{
			requestsAndResponseAssemblies.Add(assembly);
			return this;
		}

		private void SetDefaultImplementations()
		{
			AsyncRequestDispatcherImplementation = typeof(AsyncRequestDispatcher);
			AsyncRequestDispatcherFactoryImplementation = typeof(AsyncRequestDispatcherFactory);
			RequestProcessorImplementation = typeof(AsyncRequestProcessorProxy);
			CacheManagerImplementation = typeof(CacheManager);
			CacheProviderImplementation = typeof(InMemoryCacheProvider);
		}

		public void Initialize()
		{
			IoC.Container = container ?? (IContainer)Activator.CreateInstance(ContainerImplementation);
			IoC.Container.RegisterInstance(this);
			IoC.Container.Register(typeof(IAsyncRequestProcessor), RequestProcessorImplementation, Lifestyle.Transient);
			IoC.Container.Register(typeof(IAsyncRequestDispatcher), AsyncRequestDispatcherImplementation, Lifestyle.Transient);
			IoC.Container.Register(typeof(IAsyncRequestDispatcherFactory), AsyncRequestDispatcherFactoryImplementation, Lifestyle.Singleton);
			IoC.Container.Register(typeof(ICacheProvider), CacheProviderImplementation, Lifestyle.Singleton);
			IoC.Container.Register(typeof(ICacheManager), CacheManagerImplementation, Lifestyle.Singleton);
			IoC.Container.Register<ITimerProvider, TimerProvider>(Lifestyle.Singleton);
			RegisterRequestAndResponseTypes();
			ConfigureCachingLayer();
		}

		private void ConfigureCachingLayer()
		{
			var requestTypes = requestsAndResponseAssemblies.SelectMany(a => a.GetTypes()).Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Request)));
			var cacheConfiguration = new ClientCacheConfiguration(requestTypes);
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
	}
}