using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Agatha.Common;
using Agatha.Common.Caching;
using Agatha.Common.Configuration;
using Agatha.Common.InversionOfControl;
using Agatha.Common.WCF;
using Agatha.ServiceLayer;
using Agatha.ServiceLayer.Conventions;
using TestTypes;
using Xunit;

namespace Tests.ConfigurationTests
{
    public abstract class ServiceLayerComponentResolving<TContainer> where TContainer : IContainer
    {
        private static readonly List<Assembly> requestHandlerAssemblies;
        private static readonly List<Assembly> requestResponseAssemblies;
        private static ServiceLayerConfiguration configuration;

        static ServiceLayerComponentResolving()
        {
            IoC.Container = null;
            KnownTypeProvider.ClearAllKnownTypes();
            requestHandlerAssemblies = new List<Assembly> { Assembly.GetExecutingAssembly(), typeof(RequestHandlerB).Assembly };
            requestResponseAssemblies = new List<Assembly> { Assembly.GetExecutingAssembly(), typeof(RequestB).Assembly };
            configuration = new ServiceLayerConfiguration(requestHandlerAssemblies[0], requestResponseAssemblies[0],
                                                          Activator.CreateInstance<TContainer>());
            configuration.AddRequestHandlerAssembly(requestHandlerAssemblies[1]);
            configuration.AddRequestAndResponseAssembly(requestResponseAssemblies[1]);
            configuration.Use<RequestHandlerBasedConventions>();
            configuration.Initialize();
        }

        [Fact]
        public void CanResolveServiceLayerConfiguration()
        {
            Assert.NotNull(IoC.Container.Resolve<ServiceLayerConfiguration>());
        }

        [Fact]
        public void ServiceLayerConfigurationIsSingleton()
        {
            AssertIsSingleton<ServiceLayerConfiguration>();
        }

        [Fact]
        public void CanResolveRequestProcessor()
        {
            Assert.NotNull(IoC.Container.Resolve<IRequestProcessor>());
        }

        [Fact]
        public void RequestProcessorIsTransient()
        {
            AssertIsTransient<IRequestProcessor>();
        }

        [Fact]
        public void CanResolveAsyncRequestProcessor()
        {
            Assert.NotNull(IoC.Container.Resolve<IAsyncRequestProcessor>());
        }

        [Fact]
        public void AsyncRequestProcessorIsTransient()
        {
            AssertIsTransient<IAsyncRequestProcessor>();
        }

        [Fact]
        public void CanResolveRequestHandler()
        {
            Assert.NotNull(IoC.Container.Resolve<IRequestHandler<RequestA>>());
        }

        [Fact]
        public void CanResolveRequestHandlerWithBaseRequestHandlerWhoseFirstGenericTypeIsNotARequestType()
        {
            Assert.NotNull(IoC.Container.Resolve<IRequestHandler<RequestX>>());
        }

        [Fact]
        public void CanResolveRequestHandlerFromExtraAssembly()
        {
            Assert.NotNull(IoC.Container.Resolve<IRequestHandler<RequestB>>());
        }

        [Fact]
        public void RequestHandlerIsTransient()
        {
            AssertIsTransient<IRequestHandler<RequestA>>();
        }

        [Fact]
        public void CanResolveOneWayRequestHandler()
        {
            Assert.NotNull(IoC.Container.Resolve<IOneWayRequestHandler<OneWayRequestA>>());
        }

        [Fact]
        public void CanResolveOneWayRequestHandlerFromExtraAssembly()
        {
            Assert.NotNull(IoC.Container.Resolve<IRequestHandler<RequestB>>());
        }

        [Fact]
        public void OneWayRequestHandlerIsTransient()
        {
            AssertIsTransient<IOneWayRequestHandler<OneWayRequestA>>();
        }

        [Fact]
        public void CanResolveCacheProvider()
        {
            Assert.NotNull(IoC.Container.Resolve<ICacheProvider>());
        }

        [Fact]
        public void CacheProviderIsSingleton()
        {
            AssertIsSingleton<ICacheProvider>();
        }

        [Fact]
        public void CanResolveCacheManager()
        {
            Assert.NotNull(IoC.Container.Resolve<ICacheManager>());
        }

        [Fact]
        public void CacheManagerIsSingleton()
        {
            AssertIsSingleton<ICacheManager>();
        }

        [Fact]
        public void CanResolveCacheConfiguration()
        {
            Assert.NotNull(IoC.Container.Resolve<CacheConfiguration>());
        }

        [Fact]
        public void CacheConfigurationIsSingleton()
        {
            AssertIsSingleton<CacheConfiguration>();
        }

        [Fact]
        public void CachingInterceptorIsRegisteredAsFirstInterceptor()
        {
            Assert.Equal(typeof(CachingInterceptor), configuration.GetRegisteredInterceptorTypes().First());
        }

        [Fact]
        public void InterceptorsAreTransient()
        {
            AssertIsTransient<CachingInterceptor>();
        }

        [Fact]
        public void RequestTypeIsRegistered()
        {
            Assert.Contains(typeof(RequestA), KnownTypeProvider.GetKnownTypes(null));
        }

        [Fact]
        public void RequestTypeFromExtraAssemblyIsRegistered()
        {
            Assert.Contains(typeof(RequestB), KnownTypeProvider.GetKnownTypes(null));
        }

        [Fact]
        public void ResponseTypeIsRegistered()
        {
            Assert.Contains(typeof(ResponseA), KnownTypeProvider.GetKnownTypes(null));
        }

        [Fact]
        public void ResponseTypeFromExtraAssemblyIsRegistered()
        {
            Assert.Contains(typeof(ResponseB), KnownTypeProvider.GetKnownTypes(null));
        }

        [Fact]
        public void CanResolveConventions()
        {
            Assert.IsType(typeof(RequestHandlerBasedConventions), IoC.Container.Resolve<IConventions>());
        }

        [Fact]
        public void ConventionsAreRegisteredAsSingleton()
        {
            AssertIsSingleton<IConventions>();
        }

        [Fact]
        public void CanResolveResponseTypeByConvention()
        {
            var conventions = IoC.Container.Resolve<IConventions>();
            var reponseType = conventions.GetResponseTypeFor(new RequestA());
            Assert.Equal(typeof(ResponseA), reponseType);
        }

        [Fact]
        public void RequestHandlerRegistryIsSingleton()
        {
            AssertIsSingleton<IRequestHandlerRegistry>();
        }

        [Fact]
        public void RequestHandlerRegistryContainsTypedRequestHandlers()
        {
            var expectedRequestHandlers = requestHandlerAssemblies.SelectMany(assembly =>
                    assembly.GetTypes().Where(t => typeof(ITypedRequestHandler).IsAssignableFrom(t) && !t.IsAbstract).Select(t => t.FullName));
            var actualRequestHandlers = IoC.Container.Resolve<IRequestHandlerRegistry>().GetTypedRequestHandlers().Select(t => t.FullName);
            Assert.Equal(expectedRequestHandlers.Count(), expectedRequestHandlers.Intersect(actualRequestHandlers).Count());
        }

        [Fact]
        public void RequestTypeRegistryIsSingleton()
        {
            AssertIsSingleton<IRequestTypeRegistry>();
        }

        [Fact]
        public void RequestTypeRegistryContainsAllRequestTypesFromRegisteredAssemblies()
        {
            var expectedRequestTypes = requestResponseAssemblies.SelectMany(assembly =>
                    assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Request)) && !t.IsAbstract)).Select(t => t.FullName);
            var actualRequestTypes = IoC.Container.Resolve<IRequestTypeRegistry>().GetRegisteredRequestTypes().Select(t => t.FullName);
            Assert.Equal(expectedRequestTypes.Count(), expectedRequestTypes.Intersect(actualRequestTypes).Count());
        }

        [Fact]
        public void CanResolveRequestProcessingErrorHandler()
        {
            Assert.NotNull(IoC.Container.Resolve<IRequestProcessingErrorHandler>());
        }

        private static void AssertIsSingleton<T>()
        {
            Assert.Same(IoC.Container.Resolve<T>(), IoC.Container.Resolve<T>());
        }

        private static void AssertIsTransient<T>()
        {
            Assert.NotSame(IoC.Container.Resolve<T>(), IoC.Container.Resolve<T>());
        }
    }

    public class ServiceLayerComponentResolvingWithCastle : ServiceLayerComponentResolving<Agatha.Castle.Container> { }

    public class ServiceLayerComponentResolvingWithUnity : ServiceLayerComponentResolving<Agatha.Unity.Container> { }

    public class ServiceLayerComponentResolvingWithNinject : ServiceLayerComponentResolving<Agatha.Ninject.Container> { }

    public class ServiceLayerComponentResolvingWithStructureMap : ServiceLayerComponentResolving<Agatha.StructureMap.Container> { }

    public class ServiceLayerComponentResolvingWithSpring : ServiceLayerComponentResolving<Agatha.Spring.Container> { }
}