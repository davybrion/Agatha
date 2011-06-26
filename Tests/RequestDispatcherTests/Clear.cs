using System;
using System.Linq;
using QuickDotNetCheck;

namespace Tests.RequestDispatcherTests
{
    public class Clear : RequestDispatcherFixture
    {
        private readonly Func<RequestDispatcherTestsState> state;

        public Clear(Func<RequestDispatcherTestsState> state)
            : base(() => state().RequestDispatcher) 
        {
            this.state = state;
        }

        protected override void Act()
        {
            requestDispatcher().Clear();
            state().ClearKeysUsed();
        }

        public Spec EmptiesOutTheRequests()
        {
            return new Spec(() => Ensure.Equal(0, requestDispatcher().SentRequests.Count()));
        }
    }
}
