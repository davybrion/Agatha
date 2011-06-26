using System.Reflection;
using Agatha.Common;
using Agatha.Common.Caching;
using Agatha.Common.InversionOfControl;
using Agatha.ServiceLayer;
using QuickDotNetCheck;
using Rhino.Mocks;
using Xunit;

namespace Tests.RequestProcessorTests.OneWay
{
    public class OneWayRequestSuite : Suite
    {
        public static IOneWayRequestHandler<FirstOneWayRequest> firstOneWayRequestHandler;
        public static IOneWayRequestHandler<SecondOneWayRequest> secondOneWayRequestHandler;
        public static IOneWayRequestHandler<ThirdOneWayRequest> thirdOneWayRequestHandler;

        public static IRequestProcessor requestProcessor;
        public static CacheManagerSpy cacheManager;
        public static ServiceLayerConfiguration serviceLayerConfiguration;

        public OneWayRequestSuite() 
            : base(40, 20)
        {
            IoC.Container = new Agatha.Castle.Container();

            serviceLayerConfiguration = new ServiceLayerConfiguration(Assembly.GetExecutingAssembly(), Assembly.GetExecutingAssembly(), IoC.Container)
                                            {
                                                BusinessExceptionType = typeof(BusinessException),
                                                SecurityExceptionType = typeof(SecurityException),
                                                CacheManagerImplementation = typeof(CacheManagerSpy)
                                            };
            serviceLayerConfiguration.Initialize();

            // i want to take advantage of the automatic initialization, so i'm just resolving the requestprocessor instead of creating it
            requestProcessor = IoC.Container.Resolve<IRequestProcessor>();
            // the cache manager is a singleton so i can just resolve it and it'll be the same one the request processor uses
            cacheManager = (CacheManagerSpy)IoC.Container.Resolve<ICacheManager>();

            firstOneWayRequestHandler = MockRepository.GenerateMock<IOneWayRequestHandler<FirstOneWayRequest>>();
            secondOneWayRequestHandler = MockRepository.GenerateMock<IOneWayRequestHandler<SecondOneWayRequest>>();
            thirdOneWayRequestHandler = MockRepository.GenerateMock<IOneWayRequestHandler<ThirdOneWayRequest>>();

            IoC.Container.RegisterInstance(firstOneWayRequestHandler);
            IoC.Container.RegisterInstance(secondOneWayRequestHandler);
            IoC.Container.RegisterInstance(thirdOneWayRequestHandler);
        }

        [Fact]
        public void Verify()
        {
            Register(() => new OneWayRequestExceptionHandling());
            Run();
        }
    }
}