using System;
using System.Collections.Generic;
using System.Linq;
using Agatha.Common;
using QuickNet;
using QuickNet.Specifications;
using QuickGenerate;
using QuickGenerate.Implementation;
using QuickGenerate.Meta;
using QuickGenerate.Uber;
using QuickNet.Types;
using Tests.RequestProcessorTests;
using xunit.extensions.quicknet;

namespace Tests.NewRequestProcessorTests
{
    public class CachingSpecs : AcidTest<RequestProcessorTestsState>
    {
        public CachingSpecs()
            : base(10, 200)
        {
            // The adjustements made below to this test's inputs simplifies the specs a little.

            // Overwriting the Request Generator in RequestProcessorTestsState
            // so we only have Requests eligable for caching.
            State.GeneratorRepository =
                new GeneratorRepository()
                    .With<Request, FirstCachedRequest>()
                    .With<Request, SecondCachedRequest>()
                    // Not throwing exceptions here
                    .With<DescribedAction>(new DescribedAction { Exception = null, Description = "Does nothing" });
        }

        [SpecFor(typeof(ProcessRequestsTransition))]
        public Spec CachedResponsesAreReturnedWhenAvailable(IList<QuickNet.Types.Tuple<Request, DescribedAction>> input, Response[] output)
        { 
            return new Spec(
                () => State.CacheManager.ReturnedCachedResponses
                    .Where(cachedResponse => cachedResponse != null)
                    .ForEach(response => Ensure.True(output.Any(src => src.Equals(response)))))
                .IfAfter(() => State.CacheManager.ReturnedCachedResponses.Any(r => r != null));
        }

        [SpecFor(typeof(ProcessRequestsTransition))]
        public Spec AllReturnedCachedResponsesIfAnyHaveTheirIsCachedFlagSetToTrue(IList<QuickNet.Types.Tuple<Request, DescribedAction>> input, Response[] output)
        {
            return new Spec(
                () => State.CacheManager.ReturnedCachedResponses
                    .Where(cachedResponse => cachedResponse != null)
                    .ForEach(response => Ensure.True(output.First(src => src.Equals(response)).IsCached)))
                .IfAfter(() => State.CacheManager.ReturnedCachedResponses.Any(r => r != null));
        }

        [SpecFor(typeof(ProcessRequestsTransition))]
        public Spec ResponsesAreCachedIfTheyArentInTheCacheYet(IList<QuickNet.Types.Tuple<Request, DescribedAction>> input, Response[] output)
        {
            return new Spec(
                () =>
                    {
                        output.Where(src => !State.CacheManager.ReturnedCachedResponses.Contains(src) && src.IsCached)
                            .ForEach(response =>
                                {
                                    int index = output.ToList().IndexOf(response);
                                    Request request = input[index].First;
                                    Ensure.Equal(response, State.CacheManager.CacheEntries.First(entry => entry.Request.Equals(request)).Response);
                                });
                    })
                .IfAfter(() => State.CacheManager.ReturnedCachedResponses.Any(r => r == null));               
        }
    }
}