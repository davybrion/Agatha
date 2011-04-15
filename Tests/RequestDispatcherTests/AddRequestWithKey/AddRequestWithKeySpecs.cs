using System;
using System.Linq;
using Agatha.Common;
using QuickNet;
using QuickNet.Specifications;
using QuickNet.Types;
using xunit.extensions.quicknet;

namespace Tests.RequestDispatcherTests.AddRequestWithKey
{
    public class AddRequestWithKeySpecs : AcidTest<RequestDispatcherTestsState>
    {
        public AddRequestWithKeySpecs() : base(10, 10) { }

        [SpecFor(typeof(AddRequestWithKeyTransition))]
        public Spec TheKeyedRequestExistsInTheRequestCollection(QuickNet.Types.Tuple<string, Request> input, Null output)
        {
            return new Spec(
                () => Ensure.True(State.RequestDispatcher.SentRequests.Any(src => src == input.Second)))
                .If(() =>
                          !State.KeyAllreadyUsed(input.First)
                       && !State.RequestTypeAllreadyUsedWithoutKey(input.Second))
                .WhenHolds(
                () => State.RegisterRequestTypeKeyCombinationUsed(input));
        }

        [SpecFor(typeof(AddRequestWithKeyTransition))]
        public Spec ThrowsExceptionIfKeyAllreadyUsed(QuickNet.Types.Tuple<string, Request> input, Null output)
        {
            return new Spec()
                .Throws<InvalidOperationException>()
                .If(() => State.KeyAllreadyUsed(input.First));
        }
    }
}