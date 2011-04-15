using System;
using System.Collections.Generic;
using System.Linq;
using Agatha.Common;
using QuickNet;
using QuickNet.Specifications;
using QuickGenerate.Implementation;
using QuickNet.Types;
using Tests.RequestProcessorTests;
using Xunit;
using xunit.extensions.quicknet;

namespace Tests.RequestDispatcherTests.AddRequestArray
{
    public class AddRequestArraySpecs : AcidTest<RequestDispatcherTestsState>
    {
        public AddRequestArraySpecs() : base(10, 10) { }

        [SpecFor(typeof(AddRequestArrayTransition))]
        public Spec AllRequestsExistsInTheRequestCollection(IList<Request> input, Null output)
        {
            return new Spec(
                () => input.ForEach(r => Ensure.True(State.RequestDispatcher.SentRequests.Any(src => src == r))))
                .If(() =>
                    !State.RequestTypeAllreadyUsed(input.ToArray())
                    && !RequestsOfSameTypeExistsInInputArray(input))
                .WhenHolds(() => State.RegisterRequestTypeUsed(input.Select(src => src.GetType()).ToArray()));
        }

        [SpecFor(typeof(AddRequestArrayTransition))]
        public Spec ThrowsExceptionIfAnyRequestTypeAllreadyUsed(IList<Request> input, Null output)
        {
            return new Spec()
                .Throws<InvalidOperationException>()
                .If(() => State.RequestTypeAllreadyUsed(input.ToArray()))
                .WhenHolds(() => State.RegisterRequestTypeUsedUpToTheFirstAllreadyUsed(input.ToArray()));
        }

        [SpecFor(typeof(AddRequestArrayTransition))]
        public Spec ThrowsExceptionIfRequestsOfSameTypeExistsInInputArray(IList<Request> input, Null output)
        {
            return new Spec()
                .Throws<InvalidOperationException>()
                .If(() => RequestsOfSameTypeExistsInInputArray(input))
                .WhenHolds(()=> State.RegisterRequestTypeUsedUpToTheFirstAllreadyUsed(input.ToArray()));
        }

        private static bool RequestsOfSameTypeExistsInInputArray(ICollection<Request> input)
        {
            return input.Count != input.Select(src => src.GetType()).Distinct().Count();
        }
    }

    public class DocTest
    {
        [Fact(Skip = "The first call to the dispatcher produces some strange side-effects. UnSkip this to see.")]
        public void Reproduce()
        {
            IRequestDispatcher dispatcher = new RequestDispatcherStub();
            try
            {
                dispatcher.Add(new Request[] { new FirstRequest(), new FirstRequest() });
            }
            catch { }

            dispatcher.Add(new Request[] { new FirstRequest() });

        }
    }
}