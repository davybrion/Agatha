using System;
using System.Collections.Generic;
using System.Reflection;
using Agatha.Common;
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
		public IContainer Container { get; private set; }
		public Type ContainerImplementation { get; private set; }
		public Type BusinessExceptionType { get; set; }
		public Type SecurityExceptionType { get; set; }

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
			RequestProcessorImplementation = typeof(RequestProcessor);
			AsyncRequestDispatcherImplementation = typeof(AsyncRequestDispatcher);
			AsyncRequestDispatcherFactoryImplementation = typeof(AsyncRequestDispatcherFactory);
			AsyncRequestProcessorImplementation = typeof(AsyncRequestProcessor);

			IoC.Container = container ?? (IContainer)Activator.CreateInstance(ContainerImplementation);
		}

		public void Initialize()
		{
			serviceLayerConfiguration = new ServiceLayerConfiguration(IoC.Container)
			{
				AsyncRequestProcessorImplementation = AsyncRequestProcessorImplementation,
				BusinessExceptionType = BusinessExceptionType,
				RequestProcessorImplementation = RequestProcessorImplementation,
				SecurityExceptionType = SecurityExceptionType
			};

			foreach (var assembly in requestHandlerAssemblies)
				serviceLayerConfiguration.AddRequestHandlerAssembly(assembly);

			foreach (var assembly in requestsAndResponseAssemblies)
				serviceLayerConfiguration.AddRequestAndResponseAssembly(assembly);

			serviceLayerConfiguration.Initialize();

			IoC.Container.Register(typeof(IAsyncRequestDispatcher), AsyncRequestDispatcherImplementation, Lifestyle.Transient);
			IoC.Container.Register(typeof(IAsyncRequestDispatcherFactory), AsyncRequestDispatcherFactoryImplementation, Lifestyle.Singleton);
		}
	}
}