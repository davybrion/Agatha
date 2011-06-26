using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agatha.Common;
using QuickDotNetCheck;

namespace Tests.RequestDispatcherTests
{
    public class AddRequestArray : RequestDispatcherFixture
    {
        private List<Request> input;

        public AddRequestArray(Func<RequestDispatcher> requestDispatcher)
            : base(requestDispatcher) { }

        public override void Arrange()
        {
            input = new RequestGenerator().Many<Request>(2, 2).ToList();
        }

        protected override void Act()
        {
            requestDispatcher().Add(input.ToArray());
        }

        public Spec AllRequestsExistsInTheRequestCollection()
        {
            return new Spec(
                () => input.ForEach(r => Ensure.True(requestDispatcher().SentRequests.Any(src => src == r))))
                .If(() => AllRequestTypesInInputArrayAreNotYetUsed()
                          && NoRequestsOfSameTypeExistInInputArray());
        }

        public Spec ThrowsExceptionIfAnyRequestTypeAllreadyUsed()
        {
            return new Spec(Ensure.Throws<InvalidOperationException>)
                .If(AnyRequestTypeInInputArrayIsAllreadyUsed);
        }

        public Spec ThrowsExceptionIfRequestsOfSameTypeExistsInInputArray()
        {
            return new Spec(Ensure.Throws<InvalidOperationException>)
                .If(() => !AnyRequestTypeInInputArrayIsAllreadyUsed() && RequestsOfSameTypeExistsInInputArray());
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(GetType().Name);
            sb.AppendLine("    input : Request[]");
            sb.AppendLine("    {");
            foreach (var request in input)
            {
                sb.AppendFormat("        {0},", request.GetType().Name);
                sb.AppendLine();
            }
            sb.AppendLine("    }");
            return sb.ToString();
        }

        private bool RequestsOfSameTypeExistsInInputArray()
        {
            return input.Count() != input.Select(src => src.GetType()).Distinct().Count();
        }

        private bool NoRequestsOfSameTypeExistInInputArray()
        {
            return !RequestsOfSameTypeExistsInInputArray();
        }

        private bool AllRequestTypesInInputArrayAreNotYetUsed()
        {
            return input.All(request => !RequestTypeAllreadyUsed(request.GetType()));
        }

        private bool AnyRequestTypeInInputArrayIsAllreadyUsed()
        {
            return !AllRequestTypesInInputArrayAreNotYetUsed();
        }
    }
}