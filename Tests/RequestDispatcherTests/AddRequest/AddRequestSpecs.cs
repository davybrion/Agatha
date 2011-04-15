using System;
using System.Linq;
using Agatha.Common;
using QuickNet;
using QuickNet.Specifications;
using QuickNet.Types;
using xunit.extensions.quicknet;

namespace Tests.RequestDispatcherTests.AddRequest
{
    public class AddRequestSpecs : AcidTest<RequestDispatcherTestsState>
    {
        public AddRequestSpecs() : base(10, 10) { }

        [SpecFor(typeof(AddRequestTransition))]
        public Spec TheRequestExistsInTheRequestCollection(Request input, Null output)
        {
            return new Spec(
                () => Ensure.True(State.RequestDispatcher.SentRequests.Any(src => src == input)))
                .If(() => !State.RequestTypeAllreadyUsed(input))
                .WhenHolds(() => State.RegisterRequestTypeUsed(input.GetType()));
        }

        [SpecFor(typeof(AddRequestTransition))]
        public Spec ThrowsExceptionIfRequestTypeAllreadyUsed(Request input, Null output)
        {
            return new Spec()
                .Throws<InvalidOperationException>()
                .If(() => State.RequestTypeAllreadyUsed(input));
        }
    }
}