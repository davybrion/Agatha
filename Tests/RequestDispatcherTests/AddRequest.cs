using System;
using System.Linq;
using System.Text;
using Agatha.Common;
using QuickDotNetCheck;

namespace Tests.RequestDispatcherTests
{
    public class AddRequest : RequestDispatcherFixture
    {
        private Request input;

        public AddRequest(Func<RequestDispatcher> requestDispatcher)
            : base(requestDispatcher) { }

        public override void Arrange()
        {
            input = new RequestGenerator().One<Request>();
        }

        protected override void Act()
        {
            requestDispatcher().Add(input);
        }

        public Spec TheRequestExistsInTheRequestCollection()
        {
            return new Spec(() => Ensure.True(requestDispatcher().SentRequests.Any(src => src == input)))
                .If(() => RequestTypeNotYetUsed(input.GetType()));
        }

        public Spec ThrowsExceptionIfRequestTypeAllreadyUsed()
        {
            return new Spec(Ensure.Throws<InvalidOperationException>)
                .If(() => RequestTypeAllreadyUsed(input.GetType()));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(GetType().Name);
            sb.AppendFormat("    input : {0}", input.GetType().Name);
            return sb.ToString();
        }
    }
}