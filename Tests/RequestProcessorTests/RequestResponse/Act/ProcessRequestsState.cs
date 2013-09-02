using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Agatha.Common;
using Agatha.Common.Caching;
using Agatha.Common.InversionOfControl;
using Agatha.ServiceLayer;
using QuickGenerate;
using QuickGenerate.Primitives;
using Tests.RequestProcessorTests.RequestResponse.Act.Helpers;

namespace Tests.RequestProcessorTests.RequestResponse.Act
{
    public class ProcessRequestsState : IDisposable
    {
        public static IRequestProcessor RequestProcessor { get; set; }
        public static List<Exception> ExceptionsThrown { get; set; }
        public static CacheManagerSpy CacheManager { get; set; }

        public static DomainGenerator Generator { get; set; }

        public ProcessRequestsState()
        {
            Generator =
                new DomainGenerator()
                    .With(mi => mi.DeclaringType == typeof(string), new StringGenerator(1, 1))
                    .With(mi => mi.DeclaringType == typeof(int), new IntGenerator(0, 5))
                    .With<Request>(opt => opt.StartingValue(
                        () =>
                            new Request[]
                            {
                                new FirstRequest(),
                                new SecondRequest(),
                                new ThirdRequest(),
                                new FourthRequest(),
                                new FifthRequest(),
                                new SixthRequest(),
                                new SeventhRequest(), 
                                new FirstCachedRequest(),
                                new SecondCachedRequest()
                            }.PickOne()))
                    .With(
                        new DescribedAction { Exception = null, Description = "Does nothing" },
                        new DescribedAction { Exception = new BusinessException(), Description = "Throws BusinessException" },
                        new DescribedAction { Exception = new SecurityException(), Description = "Throws SecurityException" },
                        new DescribedAction { Exception = new UnknownException(), Description = "Throws UnknownException" },
                        new DescribedAction { Exception = new AnotherUnknownException(), Description = "Throws AnotherUnknownException" },
                        new DescribedAction { Exception = new SubTypeOfBusinessException(), Description = "Throws a subtype of BusinessException" },
                        new DescribedAction { Exception = new SubTypeOfSecurityException(), Description = "Throws a subtype of SecurityException" });

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

        public void Dispose()
        {
            IoC.Container.Release(RequestProcessor);
        }

        public static void StubHandler(Type requestType, Action<IList<Exception>> handleAction)
        {
            AbstractHandlerForTest handler = GetHandler(requestType);
            handler.ExceptionsThrown = ExceptionsThrown;
            handler.HandleAction = handleAction;
            handler.HandledRequest = 0;
            handler.DefaultResponseReturned = 0;
        }

        public static AbstractHandlerForTest GetHandler(Type requestType)
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

        public static bool ExceptionWasThrownAfterTheFirstRequest()
        {
            return
                ExceptionWasThrown()
                && FirstExceptionThrownWasntOnFirstRequest();
        }

        public static bool ExceptionWasThrownBeforeLastRequest()
        {
            return
                ExceptionWasThrown()
                && FirstExceptionThrownWasntOnLastRequest();
        }

        public static bool ExceptionWasThrown()
        {
            return GetIndexOfFirstExceptionThrown() >= 0;
        }

        public static bool NoExceptionWasThrown()
        {
            return !ExceptionWasThrown();
        }

        public static bool FirstExceptionThrownWasntOnFirstRequest()
        {
            return GetIndexOfFirstExceptionThrown() != 0;
        }

        public static bool FirstExceptionThrownWasntOnLastRequest()
        {
            return GetIndexOfFirstExceptionThrown() < (ExceptionsThrown.Count - 1);
        }

        public static int GetIndexOfFirstExceptionThrown()
        {
            return ExceptionsThrown.FindIndex(e => e != null);
        }

        public static int GetIndexOfFirstExceptionThrown<TException>()
        {
            return ExceptionsThrown.FindIndex(e => e != null && e.GetType() == typeof(TException));
        }

        public static bool Threw<TException>()
        {
            return ExceptionsThrown.Any(e => e != null && e.GetType() == typeof(TException));
        }
    }
}