using System;
using System.Collections.Generic;
using Agatha.Common;
using QuickNet;
using QuickNet.Specifications;
using QuickGenerate.Implementation;
using QuickNet.Types;
using xunit.extensions.quicknet;
using Tests.RequestProcessorTests;

namespace Tests.NewRequestProcessorTests
{
    public class ExceptionInfoSpecs : ExceptionHandlingSpecsHelper
    {
        public ExceptionInfoSpecs() 
            : base(50, 100) { }

        [SpecFor(typeof(ProcessRequestsTransition))]
        public Spec ProcessRequestsWithoutException(IList<QuickNet.Types.Tuple<Request, DescribedAction>> input, Response[] output)
        {
            return new Spec(
                () => output.ForEach(response => Ensure.Equal(ExceptionType.None, response.ExceptionType)))
                .IfAfter(NoExceptionWasThrown);
        }

        private void EnsureExceptionInfoIsCorrect<TException>(ExceptionType exceptionTypeEnum, Response[] responses)
        {
            int index = GetIndexOfFirstExceptionThrown<TException>();
            Ensure.Equal(exceptionTypeEnum, responses[index].ExceptionType);
            Ensure.Equal(State.ExceptionsThrown[index].Message, responses[index].Exception.Message);
            Ensure.Equal(State.ExceptionsThrown[index].StackTrace, responses[index].Exception.StackTrace);
            Ensure.Equal(typeof(TException).FullName, responses[index].Exception.Type);
        }

        [SpecFor(typeof(ProcessRequestsTransition))]
        public Spec ProcessRequestsWithBusinessException(IList<QuickNet.Types.Tuple<Request, DescribedAction>> input, Response[] output)
        {

            return new Spec(() => EnsureExceptionInfoIsCorrect<BusinessException>(ExceptionType.Business, output))
                .IfAfter(Threw<BusinessException>);
        }

        [SpecFor(typeof(ProcessRequestsTransition))]
        public Spec ProcessRequestsWithSecurityException(IList<QuickNet.Types.Tuple<Request, DescribedAction>> input, Response[] output)
        {
            return new Spec(() => EnsureExceptionInfoIsCorrect<SecurityException>(ExceptionType.Security, output))
                .IfAfter(Threw<SecurityException>);
        }

        [SpecFor(typeof(ProcessRequestsTransition))]
        public Spec ProcessRequestsWithUnknownException(IList<QuickNet.Types.Tuple<Request, DescribedAction>> input, Response[] output)
        {
            return new Spec(() => EnsureExceptionInfoIsCorrect<UnknownException>(ExceptionType.Unknown, output))
                .IfAfter(Threw<UnknownException>);
        }

        [SpecFor(typeof(ProcessRequestsTransition))]
        public Spec ProcessRequestsWithAnotherUnknownException(IList<QuickNet.Types.Tuple<Request, DescribedAction>> input, Response[] output)
        {
            return new Spec(() => EnsureExceptionInfoIsCorrect<AnotherUnknownException>(ExceptionType.Unknown, output))
                .IfAfter(Threw<AnotherUnknownException>);
        }

        [SpecFor(typeof(ProcessRequestsTransition))]
        public Spec ProcessRequestsWithExceptionFailsAllFollowingRequests(IList<QuickNet.Types.Tuple<Request, DescribedAction>> input, Response[] output)
        {
            Predicate<Exception> predicate = exception => exception != null;

            return new Spec(
                () =>
                    {
                        int exceptionIndex = State.ExceptionsThrown.FindIndex(predicate);
                        int responseIndex = exceptionIndex + 1;
                            // WHY: +1 because we need the response _after_ the one that threw the first exception

                        for (int i = responseIndex; i < output.Length; i++)
                        {
                            Ensure.Equal(ExceptionType.EarlierRequestAlreadyFailed, output[i].ExceptionType);

                            Ensure.Equal("EarlierRequestAlreadyFailed", output[i].Exception.Message);
                            Ensure.Null(output[i].Exception.StackTrace);
                            Ensure.Equal("System.Exception", output[i].Exception.Type);
                        }
                    })
                .IfAfter(ExceptionWasThrownBeforeLastRequest);
        }
    }
}