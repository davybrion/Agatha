using System.Collections.Generic;
using Agatha.Common;
using QuickNet;
using QuickNet.Specifications;
using QuickNet.Types;
using xunit.extensions.quicknet;

namespace Tests.NewRequestProcessorTests
{
    public class GeneralSpecs : AcidTest<RequestProcessorTestsState>
    {
        public GeneralSpecs() 
            : base(5, 10) { }

        [SpecFor(typeof(ProcessRequestsTransition))]
        public Spec NumberOfResponsesEqualsNumberOfRequests(IList<Tuple<Request, DescribedAction>> input, Response[] output)
        {
            return new Spec( () => Ensure.Equal(input.Count, output.Length));
        }
    }
}
