using xunit.extensions.quicknet;

namespace Tests.NewRequestProcessorTests
{
    // Not included in solution yet, because the caching specs still need a specific setup.
    public class AVialOfRequestProcessorProcessRequestSpecs : Vial { }

    [Using(typeof(AVialOfRequestProcessorProcessRequestSpecs))]
    public class AllRequestProcessorProcessRequestSpecs : AcidTest<RequestProcessorTestsState>
    {
        public AllRequestProcessorProcessRequestSpecs() 
            : base(100, 100) { DontShrink(); }
    }
}
