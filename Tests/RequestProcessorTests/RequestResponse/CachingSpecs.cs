using System;
using System.Linq;
using Agatha.Common;
using Castle.Core.Internal;
using QuickDotNetCheck;
using QuickGenerate;
using Tests.RequestProcessorTests.RequestResponse.Act;
using Tests.RequestProcessorTests.RequestResponse.Act.Helpers;
using Xunit;

namespace Tests.RequestProcessorTests.RequestResponse
{
    public class CachingSpecs : ProcessRequests
    {
        public CachingSpecs()
        {
            // The adjustements made below to this test's inputs simplifies the specs a little.

            // Overwriting the Request Generator in RequestProcessorTestsState
            // so we only have Requests eligable for caching.
            ProcessRequestsState.Generator =
                new DomainGenerator()
                    .With<Request>(opt => opt.StartingValue(
                        () =>
                            new Request[]
                                {
                                    new FirstCachedRequest(),
                                    new SecondCachedRequest()
                                }.PickOne()))
                    // Not throwing exceptions here
                    .With(new DescribedAction { Exception = null, Description = "Does nothing" });
        }

        [Fact]
        public void VerifyMySpecs()
        {
            new Suite(100, 20)
                .Using(() => new ProcessRequestsState())
                .Register(() => new CachingSpecs())
                .Run();
        }

        public Spec CachedResponsesDontCallTheHandler()
        {
            return new Spec(
                () =>
                    {
                        var requestTypes = input.Select(i => i.Item1.GetType()).Distinct().ToList();
                        foreach (var requestType in requestTypes)
                        {
                            var timesHandlerCalled = ProcessRequestsState.GetHandler(requestType).HandledRequest;

                            Type type = requestType;

                            var numberOfCachedResponsesReturned =
                                input
                                    .Select(i => i.Item1)
                                    .Where(request => request.GetType() == type)
                                    .Where(request => ProcessRequestsState.CacheManager.ReturnedCachedResponseFor(request))
                                    .Count();

                            var numberOfRequests =
                                input
                                    .Select(i => i.Item1.GetType())
                                    .Where(t => t == requestType)
                                    .Count();

                            Ensure.Equal(numberOfRequests - numberOfCachedResponsesReturned, timesHandlerCalled);
                        }
                    })
                .IfAfter(() => ProcessRequestsState.CacheManager.ReturnedCachedResponses.Any(r => r != null));
        }

        public Spec CachedResponsesAreReturnedWhenAvailable()
        { 
            return new Spec(
                () => ProcessRequestsState.CacheManager.ReturnedCachedResponses
                    .Where(cachedResponse => cachedResponse != null)
                    .ForEach(response => Ensure.True(output.Any(src => src.Equals(response)))))
                .IfAfter(() => ProcessRequestsState.CacheManager.ReturnedCachedResponses.Any(r => r != null));
        }

        public Spec AllReturnedCachedResponsesIfAnyHaveTheirIsCachedFlagSetToTrue()
        {
            return new Spec(
                () => ProcessRequestsState.CacheManager.ReturnedCachedResponses
                    .Where(cachedResponse => cachedResponse != null)
                    .ForEach(response => Ensure.True(output.First(src => src.Equals(response)).IsCached)))
                .IfAfter(() => ProcessRequestsState.CacheManager.ReturnedCachedResponses.Any(r => r != null));
        }

        public Spec ResponsesAreCachedIfTheyArentInTheCacheYet()
        {
            return new Spec(
                () =>
                output.Where(src => !ProcessRequestsState.CacheManager.ReturnedCachedResponses.Contains(src) && src.IsCached)
                    .ForEach(response =>
                                 {
                                     int index = output.ToList().IndexOf(response);
                                     Request request = input[index].Item1;
                                     Ensure.Equal(response, ProcessRequestsState.CacheManager.CacheEntries.First(entry => entry.Request.Equals(request)).Response);
                                 }))
                .IfAfter(() => ProcessRequestsState.CacheManager.ReturnedCachedResponses.Any(r => r == null));               
        }
    }
}