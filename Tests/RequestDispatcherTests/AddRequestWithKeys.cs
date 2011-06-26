using System;
using System.Linq;
using System.Text;
using Agatha.Common;
using QuickDotNetCheck;
using QuickGenerate;

namespace Tests.RequestDispatcherTests
{
    public class AddRequestWithKeys : RequestDispatcherFixture
    {
        private string key;
        private Request request;

        private readonly Func<RequestDispatcherTestsState> state;

        public AddRequestWithKeys(Func<RequestDispatcherTestsState> state) 
            : base(() => state().RequestDispatcher) 
        {
            this.state = state;
        }

        public override void Arrange()
        {
            key = new[] {"KeyOne", "KeyTwo"}.PickOne();
            request = new RequestGenerator().One<Request>();
            state().UseKey(key);
        }

        protected override void Act()
        {
            requestDispatcher().Add(key, request);
        }

        public Spec TheKeyedRequestExistsInTheRequestCollection()
        {
            return new Spec(
                () => Ensure.True(requestDispatcher().SentRequests.Any(src => src == request)))
                .If(
                    () => !state().KeyAllreadyUsed(key));
        }

        public Spec ThrowsExceptionIfKeyAllreadyUsed()
        {
            return new Spec(Ensure.Throws<InvalidOperationException>)
                .If(() => state().KeyAllreadyUsed(key));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(GetType().Name);
            sb.AppendFormat("    input : ");
            sb.AppendLine();
            sb.AppendFormat("        key : {0}", key);
            sb.AppendLine();
            sb.AppendFormat("        request : {0}", request.GetType().Name);
            sb.AppendLine();
            return sb.ToString();
        }
    }
}