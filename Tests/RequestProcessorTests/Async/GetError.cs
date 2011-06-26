using System;
using Agatha.Common;
using QuickDotNetCheck;

namespace Tests.RequestProcessorTests.Async
{
    public class GetError : Fixture
    {
        private Exception error;

        public override bool CanAct()
        {
            return ProcessRequestsAsynchronously.output != null;
        }

        protected override void Act()
        {
            error = ProcessRequestsAsynchronously.output.Error;
        }

        public Spec GetErrorIfAllGoesWell()
        {
            return new Spec(() => Ensure.Null(error))
                .If(ProcessRequestsAsynchronously.LastRequestDidNotThrowAnException);
        }

        public Spec GetErrorIfSomethingGoesWrong()
        {
            return new Spec(() => Ensure.NotNull(error))
                .If(ProcessRequestsAsynchronously.LastRequestThrewAnException);
        }
    }
}