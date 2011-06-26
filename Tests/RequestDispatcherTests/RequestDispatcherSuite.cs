using QuickDotNetCheck;
using Xunit;

namespace Tests.RequestDispatcherTests
{
    public class RequestDispatcherSuite : Suite
    {
        public RequestDispatcherSuite() : base(50, 20) { }

        [Fact]
        public void Verify()
        {
            Using(() => new RequestDispatcherTestsState());
            Register(() => new AddRequest(() => Get<RequestDispatcherTestsState>().RequestDispatcher));
            Register(() => new AddRequestArray(() => Get<RequestDispatcherTestsState>().RequestDispatcher));
            //Register(() => new AddRequestWithKeys(Get<RequestDispatcherTestsState>));
            Register(() => new Clear(Get<RequestDispatcherTestsState>));
            Run();
        }

        [Fact(Skip="unskip to see")]
        public void Bug()
        {
            Using(() => new RequestDispatcherTestsState());
            Register(() => new AddRequest(() => Get<RequestDispatcherTestsState>().RequestDispatcher));
            Register(() => new AddRequestWithKeys(Get<RequestDispatcherTestsState>));
            Run();
        }
    }
}