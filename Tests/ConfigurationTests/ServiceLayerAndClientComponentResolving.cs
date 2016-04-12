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
	public abstract class ServiceLayerAndClientComponentResolving<TContainer> where TContainer : IContainer
	{
        private static readonly List<Assembly> requestHandlerAssemblies;
        private static readonly List<Assembly> requestResponseAssemblies;

		static ServiceLayerAndClientComponentResolving()
		{
			IoC.Container = null;
			KnownTypeProvider.ClearAllKnownTypes();
            requestHandlerAssemblies = new List<Assembly> { Assembly.GetExecutingAssembly(), typeof(RequestHandlerB).Assembly };
            requestResponseAssemblies = new List<Assembly> { Assembly.GetExecutingAssembly(), typeof(RequestB).Assembly };
			var configuration = new ServiceLayerAndClientConfiguration(Assembly.GetExecutingAssembly(), Assembly.GetExecutingAssembly(), 
																		Activator.CreateInstance<TContainer>());
			configuration.AddRequestHandlerAssembly(typeof(RequestHandlerB).Assembly);
			configuration.AddRequestAndResponseAssembly(typeof(RequestB).Assembly);
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
		public void CanResolveRequestDispatcher()
		{
			Assert.NotNull(IoC.Container.Resolve<IRequestDispatcher>());
		}

		[Fact]
		public void RequestDispatcherIsTransient()
		{
		    AssertIsTransient<IRequestDispatcher>();
		}

		[Fact]
		public void CanResolveAsyncRequestDispatcher()
		{
			Assert.NotNull(IoC.Container.Resolve<IAsyncRequestDispatcher>());
		}

		[Fact]
		public void AsyncRequestDispatcherIsTransient()
		{
		    AssertIsTransient<IAsyncRequestDispatcher>();
		}

		[Fact]
		public void CanResolveRequestDispatcherFactory()
		{
			Assert.NotNull(IoC.Container.Resolve<IRequestDispatcherFactory>());
		}

		[Fact]
		public void RequestDispatcherFactoryIsSingleton()
		{
		    AssertIsSingleton<IRequestDispatcherFactory>();
		}

		[Fact]
		public void CanResolveAsyncRequestDispatcherFactory()
		{
			Assert.NotNull(IoC.Container.Resolve<IAsyncRequestDispatcherFactory>());
		}

		[Fact]
		public void AsyncRequestDispatcherIsSingleton()
		{
		    AssertIsSingleton<IAsyncRequestDispatcherFactory>();
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

        private static void AssertIsSingleton<T>()
        {
            Assert.Same(IoC.Container.Resolve<T>(), IoC.Container.Resolve<T>());
        }

        private static void AssertIsTransient<T>()
        {
            Assert.NotSame(IoC.Container.Resolve<T>(), IoC.Container.Resolve<T>());
        }
	}

	public class ServiceLayerAndClientComponentResolvingWithUnity : ServiceLayerAndClientComponentResolving<Agatha.Unity.Container> { }

	
}