using System.Collections.Generic;
using System.Linq;
using Agatha.Common;
using QuickNet;
using QuickNet.Specifications;
using QuickGenerate.Implementation;
using QuickNet.Types;
using xunit.extensions.quicknet;

namespace Tests.NewRequestProcessorTests
{
    public class RequestHandlerCalledHowSpecs : ExceptionHandlingSpecsHelper
    {
        public RequestHandlerCalledHowSpecs() : base(10, 100) { }

        [SpecFor(typeof(ProcessRequestsTransition))]
        public Spec AnyNonCachedRequestHandledBeforeTheFirstExceptionCallsTheHandleMethod(IList<Tuple<Request, DescribedAction>> input, Response[] output)
        {
            return new Spec(
                () =>
                    {
                        var nonFailedRequests =
                            input
                                .Select(tuple => tuple.First)
                                .Take(GetIndexOfFirstExceptionThrown() + 1);

                        var requestTypes =
                            nonFailedRequests
                                .Select(request => request.GetType())
                                .Distinct()
								.Where(type => !type.HasAttribute<EnableServiceResponseCachingAttribute>());

                        requestTypes.ForEach(
                            type =>
                            Ensure.Equal(
                                nonFailedRequests.Count(request => request.GetType() == type),
                                State.GetHandler(type).HandledRequest));
                    })
                .IfAfter(ExceptionWasThrownAfterTheFirstRequest);
        }

        [SpecFor(typeof(ProcessRequestsTransition))]
        public Spec AnyNonCachedRequestHandledAfterAndIncludingTheFirstExceptionCallsTheCreateDefaultResponseMethod(IList<Tuple<Request, DescribedAction>> input, Response[] output)
        {
            return new Spec(
                () =>
                    {
                        var requestsAfterTheFirstException =
                            input
                                .Select(tuple => tuple.First)
                                .Skip(GetIndexOfFirstExceptionThrown());

                        var requestTypes =
                            requestsAfterTheFirstException
                                .Select(request => request.GetType())
                                .Distinct()
								.Where(type => !type.HasAttribute<EnableServiceResponseCachingAttribute>());

                        requestTypes.ForEach(
                            type =>
                            Ensure.Equal(
                                requestsAfterTheFirstException.Count(request => request.GetType() == type),
                                State.GetHandler(type).DefaultResponseReturned));
                    })
                .IfAfter(ExceptionWasThrownBeforeLastRequest);
        }

        [SpecFor(typeof(ProcessRequestsTransition))]
        public Spec AnyCachedRequestThatHasNotGotAResponseInTheCacheHandledBeforeTheFirstExceptionCallsTheHandleMethod(IList<Tuple<Request, DescribedAction>> input, Response[] output)
        {
            return new Spec(
                () =>
                {
                    var nonFailedRequests =
                        input
                            .Select(tuple => tuple.First)
                            .Take(GetIndexOfFirstExceptionThrown() + 1)
							.Where(request => request.GetType().HasAttribute<EnableServiceResponseCachingAttribute>());

                    var requestTypes =
                        nonFailedRequests
                            .Select(request => request.GetType())
                            .Distinct();
                            

                    var requestsThatReturnedACachedResponse =
                        nonFailedRequests
                            .Where(request => State.CacheManager.ReturnedCachedResponseFor(request));


                    requestTypes.ForEach(
                        type =>
                        Ensure.Equal(
                            nonFailedRequests.Count(request => request.GetType() == type) 
                            - requestsThatReturnedACachedResponse.Count(request => request.GetType() == type),
                            State.GetHandler(type).HandledRequest));
                })
                .IfAfter(ExceptionWasThrownAfterTheFirstRequest);
        }

        [SpecFor(typeof(ProcessRequestsTransition))]
        public Spec CachableRequestsNeverCallTheCreateDefaultResponseMethodImplicitely(IList<Tuple<Request, DescribedAction>> input, Response[] output)
        {
            return new Spec(
                () => input
						  .Where(request => request.GetType().HasAttribute<EnableServiceResponseCachingAttribute>())
                          .Select(request => request.GetType())
                          .Distinct()
                          .ForEach(type => Ensure.Equal(0, State.GetHandler(type).DefaultResponseReturned)));
        }
    }
}