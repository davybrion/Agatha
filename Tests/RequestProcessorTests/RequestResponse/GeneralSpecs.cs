using QuickDotNetCheck;
using Tests.RequestProcessorTests.RequestResponse.Act;
using Xunit;

namespace Tests.RequestProcessorTests.RequestResponse
{
    public class GeneralSpecs : ProcessRequests
    {
        [Fact]
        public void VerifyMySpecs()
        {
            new Suite(5, 10)
                .Using(() => new ProcessRequestsState())
                .Register(() => new RequestHandlerCalledHowSpecs())
                .Run();
        }

        public Spec NumberOfResponsesEqualsNumberOfRequests()
        {
            return new Spec(() => Ensure.Equal(input.Count, output.Length));
        }
    }
}