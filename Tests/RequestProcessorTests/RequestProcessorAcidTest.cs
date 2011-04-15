using System.Reflection;
using Agatha.Common;
using Agatha.Common.Caching;
using Agatha.Common.InversionOfControl;
using Agatha.ServiceLayer;
using Rhino.Mocks;
using xunit.extensions.quicknet;

namespace Tests.RequestProcessorTests
{
	public abstract class RequestProcessorAcidTest : AcidTest
	{
		protected static IRequestHandler<FirstRequest> firstRequestHandler;
		protected static IRequestHandler<SecondRequest> secondRequestHandler;
		protected static IRequestHandler<ThirdRequest> thirdRequestHandler;
		protected static IRequestHandler<FourthRequest> fourthRequestHandler;
		protected static IRequestHandler<FifthRequest> fifthRequestHandler;
		protected static IRequestHandler<FirstCachedRequest> firstCachedRequestHandler;
		protected static IRequestHandler<SecondCachedRequest> secondCachedRequestHandler;
		protected static IOneWayRequestHandler<FirstOneWayRequest> firstOneWayRequestHandler;
		protected static IOneWayRequestHandler<SecondOneWayRequest> secondOneWayRequestHandler;
		protected static IOneWayRequestHandler<ThirdOneWayRequest> thirdOneWayRequestHandler;

		protected static IRequestProcessor requestProcessor;
		protected static CacheManagerSpy cacheManager;
		protected static ServiceLayerConfiguration serviceLayerConfiguration;

		protected RequestProcessorAcidTest(int testruns, int transitions) : base(testruns, transitions) { }

		public override void Setup()
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

			firstRequestHandler = MockRepository.GenerateMock<IRequestHandler<FirstRequest>>();
			secondRequestHandler = MockRepository.GenerateMock<IRequestHandler<SecondRequest>>();
			thirdRequestHandler = MockRepository.GenerateMock<IRequestHandler<ThirdRequest>>();
			fourthRequestHandler = MockRepository.GenerateMock<IRequestHandler<FourthRequest>>();
			fifthRequestHandler = MockRepository.GenerateMock<IRequestHandler<FifthRequest>>();
			firstCachedRequestHandler = MockRepository.GenerateMock<IRequestHandler<FirstCachedRequest>>();
			secondCachedRequestHandler = MockRepository.GenerateMock<IRequestHandler<SecondCachedRequest>>();
			firstOneWayRequestHandler = MockRepository.GenerateMock<IOneWayRequestHandler<FirstOneWayRequest>>();
			secondOneWayRequestHandler = MockRepository.GenerateMock<IOneWayRequestHandler<SecondOneWayRequest>>();
			thirdOneWayRequestHandler = MockRepository.GenerateMock<IOneWayRequestHandler<ThirdOneWayRequest>>();

			IoC.Container.RegisterInstance(firstRequestHandler);
			IoC.Container.RegisterInstance(secondRequestHandler);
			IoC.Container.RegisterInstance(thirdRequestHandler);
			IoC.Container.RegisterInstance(fourthRequestHandler);
			IoC.Container.RegisterInstance(fifthRequestHandler);
			IoC.Container.RegisterInstance(firstCachedRequestHandler);
			IoC.Container.RegisterInstance(secondCachedRequestHandler);
			IoC.Container.RegisterInstance(firstOneWayRequestHandler);
			IoC.Container.RegisterInstance(secondOneWayRequestHandler);
			IoC.Container.RegisterInstance(thirdOneWayRequestHandler);
		}
	}
}