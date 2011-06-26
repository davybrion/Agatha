using System;
using System.Reflection;
using Agatha.Common;
using QuickDotNetCheck;

namespace Tests.RequestProcessorTests.Async
{
    public class GetResults : Fixture
    {
        private Response[] result;

        public override bool CanAct()
        {
            return ProcessRequestsAsynchronously.output != null;
        }

        protected override void Act()
        {
            result = ProcessRequestsAsynchronously.output.Result;
        }

        public Spec GetResultsIfAllGoesWell()
        {
            return new Spec(() => Ensure.Equal(ProcessRequestsAsynchronously.lastProcessRequestInput.requestResponsePair.Item2, result))
                .If(ProcessRequestsAsynchronously.LastRequestDidNotThrowAnException);
        }

        public Spec GetResultsIfSomethingGoesWrong()
        {
            return new Spec(Ensure.Throws<TargetInvocationException>)
                .If(ProcessRequestsAsynchronously.LastRequestThrewAnException);
        }
    }
}