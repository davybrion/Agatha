using System;
using System.Collections.Generic;
using System.Reflection;
using Agatha.Common;
using Agatha.Common.Caching;
using Agatha.Common.InversionOfControl;
using Agatha.ServiceLayer;
using QuickGenerate;
using QuickGenerate.Meta;
using QuickGenerate.Uber;
using Tests.RequestProcessorTests;
using xunit.extensions.quicknet;
using QuickGenerate.Primitives;

namespace Tests.NewRequestProcessorTests
{
    public class RequestProcessorTestsState : AbstractState
    {
        public IRequestProcessor RequestProcessor;
        public List<Exception> ExceptionsThrown;
        public CacheManagerSpy CacheManager;

        public GeneratorRepository GeneratorRepository;

        public RequestProcessorTestsState()
        {
            GeneratorRepository =
                new GeneratorRepository()
                    .With<string>(mi => true, new StringGenerator(1, 1))
                    .With<int>(mi => true, new IntGenerator(0, 5))
                    .With<Request, FirstRequest>()
                    .With<Request, SecondRequest>()
                    .With<Request, ThirdRequest>()
                    .With<Request, FourthRequest>()
                    .With<Request, FifthRequest>()
                    .With<Request, FirstCachedRequest>()
                    .With<Request, SecondCachedRequest>()
                    .With<DescribedAction>(
                            new DescribedAction { Exception = null, Description = "Does nothing" },
                            new DescribedAction { Exception = new BusinessException(), Description = "Throws BusinessException" },
                            new DescribedAction { Exception = new SecurityException(), Description = "Throws SecurityException" },
                            new DescribedAction { Exception = new UnknownException(), Description = "Throws UnknownException" },
                            new DescribedAction { Exception = new AnotherUnknownException(), Description = "Throws AnotherUnknownException" });
        }

        public override void Setup()
        {
            IoC.Container = new Agatha.Castle.Container();

            new ServiceLayerConfiguration(Assembly.GetExecutingAssembly(), Assembly.GetExecutingAssembly(), IoC.Container)
                {
                    BusinessExceptionType = typeof(BusinessException),
                    SecurityExceptionType = typeof(SecurityException),
                    CacheManagerImplementation = typeof(CacheManagerSpy)
                }.Initialize();

            // i want to take advantage of the automatic initialization, so i'm just resolving the requestprocessor instead of creating it
            RequestProcessor = IoC.Container.Resolve<IRequestProcessor>();

            // the cache manager is a singleton so i can just resolve it and it'll be the same one the request processor uses
            CacheManager = (CacheManagerSpy)IoC.Container.Resolve<ICacheManager>();
            CacheManager.ExceptionsThrown = ExceptionsThrown;
        }

        public override void Teardown()
        {
            IoC.Container.Release(RequestProcessor);
        }

        public void StubHandler(Type requestType, Action<IList<Exception>> handleAction)
        {
            AbstractHandlerForTest handler = GetHandler(requestType);
            handler.ExceptionsThrown = ExceptionsThrown;
            handler.HandleAction = handleAction;
            handler.HandledRequest = 0;
            handler.DefaultResponseReturned = 0;
        }

        public AbstractHandlerForTest GetHandler(Type requestType)
        {
            Type type = typeof(IRequestHandler<>).MakeGenericType(new[] { requestType });
            AbstractHandlerForTest handler;
            try
            {
                handler = (AbstractHandlerForTest)IoC.Container.Resolve(type);                
            }
            catch
            {
                Type handlerType = typeof(HandlerForTest<>).MakeGenericType(new[] { requestType });
                handler = (AbstractHandlerForTest)Activator.CreateInstance(handlerType);
                IoC.Container.RegisterInstance(type, handler);
            }
            return handler;
        }
    }
}