using QuickDotNetCheck;
using Xunit;

namespace Tests.RequestProcessorTests.Async
{
    public class AsyncRequestProcessorSuite : Suite
    {
        public AsyncRequestProcessorSuite() : base(50, 20) { }

        [Fact]
        public void Verify()
        {
            Register(() => new ProcessRequestsAsynchronously());
            Register(() => new GetResults());
            Register(() => new GetError());
            Run();
        }
    }
}
