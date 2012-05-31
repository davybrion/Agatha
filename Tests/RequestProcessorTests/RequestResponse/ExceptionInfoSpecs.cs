using System;
using Agatha.Common;
using QuickDotNetCheck;
using Castle.Core.Internal;
using Tests.RequestProcessorTests.RequestResponse.Act;
using Xunit;

namespace Tests.RequestProcessorTests.RequestResponse
{
    public class ExceptionInfoSpecs : ProcessRequests
    {
        [Fact]
        public void VerifyMySpecs()
        {
            new Suite(50, 100)
                .Using(() => new ProcessRequestsState())
                .Register(() => new ExceptionInfoSpecs())
                .Run();
        }

        public Spec ProcessRequestsWithoutException()
        {
            return new Spec(
                () => output.ForEach(response => Ensure.Equal(ExceptionType.None, response.ExceptionType)))
                .IfAfter(ProcessRequestsState.NoExceptionWasThrown);
        }

        private void EnsureExceptionInfoIsCorrect<TException>(ExceptionType exceptionTypeEnum, Response[] responses)
        {
            int index = ProcessRequestsState.GetIndexOfFirstExceptionThrown<TException>();
            Ensure.Equal(exceptionTypeEnum, responses[index].ExceptionType);
            Ensure.Equal(ProcessRequestsState.ExceptionsThrown[index].Message, responses[index].Exception.Message);
            Ensure.Equal(ProcessRequestsState.ExceptionsThrown[index].StackTrace, responses[index].Exception.StackTrace);
            Ensure.Equal(typeof(TException).FullName, responses[index].Exception.Type);
        }

        public Spec ProcessRequestsWithBusinessException()
        {

            return new Spec(() => EnsureExceptionInfoIsCorrect<BusinessException>(ExceptionType.Business, output))
                .IfAfter(ProcessRequestsState.Threw<BusinessException>);
        }

        public Spec ProcessRequestsWithSecurityException()
        {
            return new Spec(() => EnsureExceptionInfoIsCorrect<SecurityException>(ExceptionType.Security, output))
                .IfAfter(ProcessRequestsState.Threw<SecurityException>);
        }

        public Spec ProcessRequestsWithUnknownException()
        {
            return new Spec(() => EnsureExceptionInfoIsCorrect<UnknownException>(ExceptionType.Unknown, output))
                .IfAfter(ProcessRequestsState.Threw<UnknownException>);
        }

        public Spec ProcessRequestsWithAnotherUnknownException()
        {
            return new Spec(() => EnsureExceptionInfoIsCorrect<AnotherUnknownException>(ExceptionType.Unknown, output))
                .IfAfter(ProcessRequestsState.Threw<AnotherUnknownException>);
        }

        public Spec ProcessRequestsWithExceptionFailsAllFollowingRequests()
        {
            Predicate<Exception> predicate = exception => exception != null;

            return new Spec(
                () =>
                    {
                        int exceptionIndex = ProcessRequestsState.ExceptionsThrown.FindIndex(predicate);
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
                .IfAfter(ProcessRequestsState.ExceptionWasThrownBeforeLastRequest);
        }
    }
}