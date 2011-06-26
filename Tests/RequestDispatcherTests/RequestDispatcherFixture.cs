using System;
using System.Linq;
using Agatha.Common;
using QuickDotNetCheck;

namespace Tests.RequestDispatcherTests
{
    public abstract class RequestDispatcherFixture : Fixture
    {
        protected readonly Func<RequestDispatcher> requestDispatcher;

        protected RequestDispatcherFixture(Func<RequestDispatcher> requestDispatcher)
        {
            this.requestDispatcher = requestDispatcher;
        }

        protected bool RequestTypeAllreadyUsed(Type type)
        {
            return requestDispatcher().SentRequests.Select(r => r.GetType()).Any(t => t.Equals(type));
        }

        protected bool RequestTypeNotYetUsed(Type type)
        {
            return !RequestTypeAllreadyUsed(type);
        }
    }
}