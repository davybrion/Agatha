using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Agatha.Common;
using Agatha.Common.Caching;
using Agatha.Common.Caching.Timers;
using Agatha.Common.Configuration;
using Agatha.Common.InversionOfControl;
using Agatha.Common.WCF;

namespace Agatha.ServiceLayer
{
    public class ServiceLayerConfiguration
    {
        private readonly List<Assembly> requestHandlerAssemblies = new List<Assembly>();
        private readonly List<Assembly> requestsAndResponseAssemblies = new List<Assembly>();
        private readonly IContainer container;
        private readonly List<Type> registeredInterceptors = new List<Type>();

        public Type RequestProcessorImplementation { get; set; }
        public Type AsyncRequestProcessorImplementation { get; set; }
        public Type CacheManagerImplementation { get; set; }
        public Type CacheProviderImplementation { get; set; }
        public Type ContainerImplementation { get; private set; }

        public IRequestTypeRegistry RequestTypeRegistry { get; private set; }
        public IRequestHandlerRegistry RequestHandlerRegistry { get; private set; }
        public Type BusinessExceptionType { get; set; }
        public Type SecurityExceptionType { get; set; }
        public Type Conventions { get; private set; }

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
            RequestTypeRegistry = new WcfKnownTypesBasedRequestTypeRegistry();
            RequestHandlerRegistry = new RequestHandlerRegistry();
            RegisterRequestHandlerInterceptor<CachingInterceptor>();
        }

        public void Initialize()
        {
            if (IoC.Container == null)
            {
                IoC.Container = container ?? (IContainer)Activator.CreateInstance(ContainerImplementation);
            }

            IoC.Container.RegisterInstance(this);
            IoC.Container.RegisterInstance(RequestTypeRegistry);
            IoC.Container.RegisterInstance(RequestHandlerRegistry);
            IoC.Container.Register(typeof(IRequestProcessor), RequestProcessorImplementation, Lifestyle.Transient);
            IoC.Container.Register(typeof(IAsyncRequestProcessor), AsyncRequestProcessorImplementation, Lifestyle.Transient);
            IoC.Container.Register(typeof(ICacheProvider), CacheProviderImplementation, Lifestyle.Singleton);
            IoC.Container.Register(typeof(ICacheManager), CacheManagerImplementation, Lifestyle.Singleton);
            IoC.Container.Register<ITimerProvider, TimerProvider>(Lifestyle.Singleton);
            if (Conventions != null) IoC.Container.Register(typeof(IConventions), Conventions, Lifestyle.Singleton);
            IoC.Container.Register<IRequestProcessingErrorHandler, RequestProcessingErrorHandler>(Lifestyle.Transient);
            RegisterRequestAndResponseTypes();
            RegisterRequestHandlers();
            ConfigureCachingLayer();
            RegisterInterceptors();
        }

        private void RegisterInterceptors()
        {
            foreach (var interceptorType in registeredInterceptors)
            {
                IoC.Container.Register(interceptorType, interceptorType, Lifestyle.Transient);
            }
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
            var oneWayHandlerType = typeof(IOneWayRequestHandler);
            var openOneWayHandlerType = typeof(IOneWayRequestHandler<>);
            var requestResponseHandlerType = typeof(IRequestHandler);
            var openRequestReponseHandlerType = typeof(IRequestHandler<>);

            var requestWithRequestHandlers = new Dictionary<Type, Type>();
            foreach (var assembly in requestHandlerAssemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract)
                        continue;

                    if (!oneWayHandlerType.IsAssignableFrom(type) && !requestResponseHandlerType.IsAssignableFrom(type))
                        continue;

                    RequestHandlerRegistry.Register(type);

                    var requestType = GetRequestType(type);

                    if (requestType != null)
                    {
                        Type handlerType = null;
                        if (oneWayHandlerType.IsAssignableFrom(type))
                        {
                            handlerType = openOneWayHandlerType.MakeGenericType(requestType);
                        }
                        else if (requestResponseHandlerType.IsAssignableFrom(type))
                        {
                            handlerType = openRequestReponseHandlerType.MakeGenericType(requestType);
                        }

                        if (handlerType != null)
                        {
                            if (requestWithRequestHandlers.ContainsKey(requestType))
                            {
                                throw new InvalidOperationException(String.Format("Found two request handlers that handle the same request: {0}. "
                                                                                + " First request handler: {1}, second: {2}. "
                                                                                + " For each request type there must by only one request handler.", requestType.FullName, type.FullName, requestWithRequestHandlers[requestType].FullName));
                            }

                            IoC.Container.Register(handlerType, type, Lifestyle.Transient);
                            requestWithRequestHandlers.Add(requestType, type);
                        }
                    }
                }
            }
        }

        private static Type GetRequestType(Type type)
        {
            var interfaceType = type.GetInterfaces().FirstOrDefault(i => i.Name.StartsWith("IRequestHandler`") || i.Name.StartsWith("IOneWayRequestHandler`"));

            if (interfaceType == null || interfaceType.GetGenericArguments().Count() == 0)
            {
                return null;
            }

            return GetFirstGenericTypeArgument(interfaceType);
        }

        private static Type GetFirstGenericTypeArgument(Type type)
        {
            return type.GetGenericArguments()[0];
        }

        public ServiceLayerConfiguration RegisterRequestHandlerInterceptor<T>() where T : IRequestHandlerInterceptor
        {
            registeredInterceptors.Add(typeof(T));
            return this;
        }

        public ServiceLayerConfiguration Use<TConventions>() where TConventions : IConventions
        {
            Conventions = typeof(TConventions);
            return this;
        }

        public IList<Type> GetRegisteredInterceptorTypes()
        {
            return registeredInterceptors;
        }
    }
}