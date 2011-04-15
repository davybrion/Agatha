using System.Linq;
using QuickNet;
using QuickNet.Specifications;
using QuickNet.Types;
using xunit.extensions.quicknet;

namespace Tests.RequestDispatcherTests.Clear
{
    public class ClearSpecs : AcidTest<RequestDispatcherTestsState>
    {
        public ClearSpecs() : base(10, 10) { }

        [SpecFor(typeof(ClearTransition))]
        public Spec EmptiesOutTheRequests(Null input, Null output)
        {
            return new Spec( () => Ensure.Equal(0, State.RequestDispatcher.SentRequests.Count()))
                .WhenHolds(State.ClearUsedTypes);
        }
    }
}