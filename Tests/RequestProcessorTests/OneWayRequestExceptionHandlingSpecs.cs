using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Agatha.Common;
using Agatha.ServiceLayer;
using Common.Logging;
using QuickNet;
using QuickNet.Specifications;
using QuickGenerate;
using QuickGenerate.Meta;
using QuickNet.Transitions;
using QuickNet.Types;
using Rhino.Mocks;
using xunit.extensions.quicknet;
using QuickGenerate.Uber;

namespace Tests.RequestProcessorTests
{
	public class OneWayRequestExceptionHandlingSpecs : RequestProcessorAcidTest
	{
		public OneWayRequestExceptionHandlingSpecs() : base(20, 40) { }

		private static ILog logger;
		private static List<Exception> exceptionsThrown;

		public const BindingFlags FlagsForGettingEverything =
			BindingFlags.NonPublic
			| BindingFlags.Public
			| BindingFlags.Instance
			| BindingFlags.FlattenHierarchy;

		public class OneWayRequestsAndHandlers : Dictionary<Type, ProcessOneWayRequestsInputElement>
		{
			public OneWayRequestsAndHandlers()
			{
				Add(typeof(FirstOneWayRequest), new ProcessOneWayRequestsInputElement(firstOneWayRequestHandler, new FirstOneWayRequest()));
				Add(typeof(SecondOneWayRequest), new ProcessOneWayRequestsInputElement(secondOneWayRequestHandler, new SecondOneWayRequest()));
				Add(typeof(ThirdOneWayRequest), new ProcessOneWayRequestsInputElement(thirdOneWayRequestHandler, new ThirdOneWayRequest()));
			}
		}

		public class ProcessOneWayRequestsInputElement
		{
			public IOneWayRequestHandler RequestHandler { get; private set; }

			public OneWayRequest Request { get; private set; }

			public ProcessOneWayRequestsInputElement(IOneWayRequestHandler requestHandler, OneWayRequest request)
			{
				RequestHandler = requestHandler;
				Request = request;
			}

			public void StubIt(Action<IList<Exception>> action, IList<Exception> exceptionsThrown)
			{
				RequestHandler
					.Stub(r => r.Handle(Request))
					.WhenCalled(arg => action(exceptionsThrown))
					.Repeat.Once();
			}
		}

		public class ProcessOneWayRequestsInput
		{
			public IList<Action<IList<Exception>>> Actions { get; set; }
			public OneWayRequestsAndHandlers OneWayRequestsAndHandlers { get; set; }
			public OneWayRequest[] Requests
			{
				get
				{
					return OneWayRequestsAndHandlers.ToList().ConvertAll(kv => kv.Value.Request).ToArray();
				}
			}
		}


		class ProcessOneWayRequestsTransition : MetaTransition<ProcessOneWayRequestsInput, Null>
		{
			public ProcessOneWayRequestsTransition()
			{
                GenerateInput =
                    () => new GeneratorRepository()
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
                                })
                            .With<ProcessOneWayRequestsInput>(
                                g => g.For(i => i.OneWayRequestsAndHandlers, new OneWayRequestsAndHandlers()))
                            .With<ProcessOneWayRequestsInput>(
                                (r, i) => { i.Actions = new List<Action<IList<Exception>>>(
                                    r.Randoms<Action<IList<Exception>>>(3, 3)); return i; })
                            .Random<ProcessOneWayRequestsInput>();
				Execute = 
					input =>
						{
							logger = MockRepository.GenerateMock<ILog>();
							FieldInfo info = requestProcessor.GetType().GetField("logger", FlagsForGettingEverything);
							info.SetValue(requestProcessor, logger);

							int ix = 0;
							exceptionsThrown = new List<Exception>();
							foreach (Type key in input.OneWayRequestsAndHandlers.Keys)
							{
								input.OneWayRequestsAndHandlers[key].StubIt(input.Actions[ix], exceptionsThrown);
								ix++;
							}
							requestProcessor.ProcessOneWayRequests(input.Requests);
							return null;
						};
                
			}
		}

		[SpecFor(typeof(ProcessOneWayRequestsTransition))]
		public Spec ProcessRequestsWithoutException(ProcessOneWayRequestsInput input, Null output)
		{
			return new Spec(() => logger.AssertWasNotCalled(l => l.Error(Arg<string>.Is.Anything, Arg<Exception>.Is.Anything)))
				.IfAfter(() => !exceptionsThrown.Exists(e => e != null));
		}

		private void VerifyThatLoggerWasCalledForEachExpectedException(Predicate<Exception> exceptionPredicate, Type exceptionType)
		{
			int times = exceptionsThrown.FindAll(exceptionPredicate).Count;

			logger.AssertWasCalled(
				l => l.Error(
						Arg<string>.Matches(arg => arg == "RequestProcessor: unhandled exception while handling request!"),
						Arg<Exception>.Matches(arg => arg.GetType() == exceptionType)),
				options => options.Repeat.Times(times));
		}

		[SpecFor(typeof(ProcessOneWayRequestsTransition))]
		public Spec ProcessRequestsWithBusinessException(ProcessOneWayRequestsInput input, Null output)
		{
			Predicate<Exception> predicate = exception => exception != null && exception.GetType() == typeof(BusinessException);

			return new Spec(() => VerifyThatLoggerWasCalledForEachExpectedException(predicate, typeof(BusinessException)))
				.IfAfter(() => exceptionsThrown.Exists(predicate));
		}

		[SpecFor(typeof(ProcessOneWayRequestsTransition))]
		public Spec ProcessRequestsWithSecurityException(ProcessOneWayRequestsInput input, Null output)
		{
			Predicate<Exception> predicate = exception => exception != null && exception.GetType() == typeof(SecurityException);

			return new Spec(() => VerifyThatLoggerWasCalledForEachExpectedException(predicate, typeof(SecurityException)))
				.IfAfter(() => exceptionsThrown.Exists(predicate));
		}

		[SpecFor(typeof(ProcessOneWayRequestsTransition))]
		public Spec ProcessRequestsWithUnknownException(ProcessOneWayRequestsInput input, Null output)
		{
			Predicate<Exception> predicate = exception => exception != null && exception.GetType() == typeof(UnknownException);

			return new Spec(() => VerifyThatLoggerWasCalledForEachExpectedException(predicate, typeof(UnknownException)))
				.IfAfter(() => exceptionsThrown.Exists(predicate));
		}

		[SpecFor(typeof(ProcessOneWayRequestsTransition))]
		public Spec ProcessRequestsWithAnotherUnknownException(ProcessOneWayRequestsInput input, Null output)
		{
			Predicate<Exception> predicate = exception => exception != null && exception.GetType() == typeof(AnotherUnknownException);

			return new Spec(() => VerifyThatLoggerWasCalledForEachExpectedException(predicate, typeof(AnotherUnknownException)))
				.IfAfter(() => exceptionsThrown.Exists(predicate));
		}
	}
}