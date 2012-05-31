using System;
using System.Linq;
using Agatha.Common;
using Castle.Core.Internal;
using QuickDotNetCheck;
using Tests.RequestProcessorTests.RequestResponse.Act;
using Xunit;

namespace Tests.RequestProcessorTests.RequestResponse
{
    public class RequestHandlerCalledHowSpecs : ProcessRequests
    {
        [Fact]
        public void VerifyMySpecs()
        {
            new Suite(100, 20)
                .Using(() => new ProcessRequestsState())
                .Register(() => new RequestHandlerCalledHowSpecs())
                .Run();
        }

        public Spec AnyNonCachedRequestHandledBeforeTheFirstExceptionCallsTheHandleMethod()
        {
            return new Spec(
                () =>
                    {
                        var nonFailedRequests =
                            input
                                .Select(tuple => tuple.Item1)
                                .Take(ProcessRequestsState.GetIndexOfFirstExceptionThrown() + 1);

                        var requestTypes =
                            nonFailedRequests
                                .Select(request => request.GetType())
                                .Distinct()
								.Where(type => !type.HasAttribute<EnableServiceResponseCachingAttribute>());

                        requestTypes.ForEach(
                            type =>
                            Ensure.Equal(
                                nonFailedRequests.Count(request => request.GetType() == type),
                                ProcessRequestsState.GetHandler(type).HandledRequest));
                    })
                .IfAfter(ProcessRequestsState.ExceptionWasThrownAfterTheFirstRequest);
        }

        public Spec AnyNonCachedRequestHandledAfterAndIncludingTheFirstExceptionCallsTheCreateDefaultResponseMethod()
        {
            return new Spec(
                () =>
                    {
                        var requestsAfterTheFirstException =
                            input
                                .Select(tuple => tuple.Item1)
                                .Skip(ProcessRequestsState.GetIndexOfFirstExceptionThrown());

                        var requestTypes =
                            requestsAfterTheFirstException
                                .Select(request => request.GetType())
                                .Distinct()
								.Where(type => !type.HasAttribute<EnableServiceResponseCachingAttribute>());

                        requestTypes.ForEach(
                            type =>
                            Ensure.Equal(
                                requestsAfterTheFirstException.Count(request => request.GetType() == type),
                                ProcessRequestsState.GetHandler(type).DefaultResponseReturned));
                    })
                .IfAfter(ProcessRequestsState.ExceptionWasThrownBeforeLastRequest);
        }

        public Spec AnyCachedRequestThatHasNotGotAResponseInTheCacheHandledBeforeTheFirstExceptionCallsTheHandleMethod()
        {
            return new Spec(
                () =>
                {
                    var nonFailedRequests =
                        input
                            .Select(tuple => tuple.Item1)
                            .Take(ProcessRequestsState.GetIndexOfFirstExceptionThrown() + 1)
							.Where(request => request.GetType().HasAttribute<EnableServiceResponseCachingAttribute>());

                    var requestTypes =
                        nonFailedRequests
                            .Select(request => request.GetType())
                            .Distinct();
                            

                    var requestsThatReturnedACachedResponse =
                        nonFailedRequests
                            .Where(request => ProcessRequestsState.CacheManager.ReturnedCachedResponseFor(request));


                    requestTypes.ForEach(
                        type =>
                        Ensure.Equal(
                            nonFailedRequests.Count(request => request.GetType() == type) 
                            - requestsThatReturnedACachedResponse.Count(request => request.GetType() == type),
                            ProcessRequestsState.GetHandler(type).HandledRequest));
                })
                .IfAfter(ProcessRequestsState.ExceptionWasThrownAfterTheFirstRequest);
        }

        public Spec CachableRequestsNeverCallTheCreateDefaultResponseMethodImplicitely()
        {
            return new Spec(
                () => input
						  .Where(request => request.GetType().HasAttribute<EnableServiceResponseCachingAttribute>())
                          .Select(request => request.GetType())
                          .Distinct()
                          .ForEach(type => Ensure.Equal(0, ProcessRequestsState.GetHandler(type).DefaultResponseReturned)));
        }
    }

    public static class TypeExt
    {
        public static bool HasAttribute<T>(this Type type)
            where T : class
        {
            return type.GetCustomAttributes(true).Any(src => (src as T) != null);
        }
    }
}