using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Logging;
using QuickDotNetCheck;
using QuickGenerate;
using Rhino.Mocks;

namespace Tests.RequestProcessorTests.OneWay
{
    public class OneWayRequestExceptionHandling : Fixture
    {
        private ProcessOneWayRequestsInput input;
        private static ILog logger;
        private static List<Exception> exceptionsThrown;

        public const BindingFlags FlagsForGettingEverything =
            BindingFlags.NonPublic
            | BindingFlags.Public
            | BindingFlags.Instance
            | BindingFlags.FlattenHierarchy;

        public override void Arrange()
        {
            var actionGenerator =
                new DomainGenerator()
                    .With<Action<IList<Exception>>>(
                        // repetition increases likelyhood
                    l => l.Add(null),
                    l => l.Add(null),
                    l => l.Add(null),
                    l => l.Add(null),
                    l =>
                        {
                            Exception exception = new BusinessException();
                            l.Add(exception);
                            throw exception;
                        },
                    l =>
                        {
                            Exception exception = new SecurityException();
                            l.Add(exception);
                            throw exception;
                        },
                    l =>
                        {
                            Exception exception = new UnknownException();
                            l.Add(exception);
                            throw exception;
                        },
                    l =>
                        {
                            Exception exception = new AnotherUnknownException();
                            l.Add(exception);
                            throw exception;
                        });

            input =
                new DomainGenerator()
                    .With<ProcessOneWayRequestsInput>(
                        opt => opt.For(i => i.OneWayRequestsAndHandlers, new OneWayRequestsAndHandlers()))
                    .With<ProcessOneWayRequestsInput>(
                        opt => opt.For(i => i.Actions, actionGenerator.Many<Action<IList<Exception>>>(3, 3).ToList()))
                    .One<ProcessOneWayRequestsInput>();
        }
        
        protected override void Act()
        {
            logger = MockRepository.GenerateMock<ILog>();
            FieldInfo info = OneWayRequestSuite.requestProcessor.GetType().GetField("logger", FlagsForGettingEverything);
            info.SetValue(OneWayRequestSuite.requestProcessor, logger);

            int ix = 0;
            exceptionsThrown = new List<Exception>();
            foreach (Type key in input.OneWayRequestsAndHandlers.Keys)
            {
                input.OneWayRequestsAndHandlers[key].StubIt(input.Actions[ix], exceptionsThrown);
                ix++;
            }
            OneWayRequestSuite.requestProcessor.ProcessOneWayRequests(input.Requests);
        }

        public Spec ProcessRequestsWithoutException()
        {
            return new Spec(() => logger.AssertWasNotCalled(l => l.Error(Arg<string>.Is.Anything, Arg<Exception>.Is.Anything)))
                .IfAfter(() => !exceptionsThrown.Exists(e => e != null));
        }

        private void VerifyThatLoggerWasCalledForEachExpectedException(Predicate<Exception> exceptionPredicate, Type exceptionType)
        {
            int times = exceptionsThrown.FindAll(exceptionPredicate).Count;

            logger.AssertWasCalled(
                l => l.Error(
                    Arg<string>.Is.NotNull,
                    Arg<Exception>.Matches(arg => arg.GetType() == exceptionType)),
                options => options.Repeat.Times(times));
        }

        public Spec ProcessRequestsWithBusinessException()
        {
            Predicate<Exception> predicate = exception => exception != null && exception.GetType() == typeof(BusinessException);

            return new Spec(() => VerifyThatLoggerWasCalledForEachExpectedException(predicate, typeof(BusinessException)))
                .IfAfter(() => exceptionsThrown.Exists(predicate));
        }

        public Spec ProcessRequestsWithSecurityException()
        {
            Predicate<Exception> predicate = exception => exception != null && exception.GetType() == typeof(SecurityException);

            return new Spec(() => VerifyThatLoggerWasCalledForEachExpectedException(predicate, typeof(SecurityException)))
                .IfAfter(() => exceptionsThrown.Exists(predicate));
        }

        public Spec ProcessRequestsWithUnknownException()
        {
            Predicate<Exception> predicate = exception => exception != null && exception.GetType() == typeof(UnknownException);

            return new Spec(() => VerifyThatLoggerWasCalledForEachExpectedException(predicate, typeof(UnknownException)))
                .IfAfter(() => exceptionsThrown.Exists(predicate));
        }

        public Spec ProcessRequestsWithAnotherUnknownException()
        {
            Predicate<Exception> predicate = exception => exception != null && exception.GetType() == typeof(AnotherUnknownException);

            return new Spec(() => VerifyThatLoggerWasCalledForEachExpectedException(predicate, typeof(AnotherUnknownException)))
                .IfAfter(() => exceptionsThrown.Exists(predicate));
        }
    }
}