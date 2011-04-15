using System;
using System.Collections.Generic;
using System.Linq;
using Agatha.Common;
using Agatha.ServiceLayer;
using QuickNet;
using QuickNet.Specifications;
using QuickGenerate;
using QuickGenerate.Meta;
using QuickNet.Transitions;
using QuickNet.Types;
using Rhino.Mocks;
using xunit.extensions.quicknet;
using Tuple = QuickNet.Types.Tuple;
using QuickGenerate.Uber;
using QuickGenerate.Primitives;

namespace Tests.RequestProcessorTests
{
	public class CachingSpecs : RequestProcessorAcidTest
	{
		public CachingSpecs() : base(10, 20) {}

		public class ProcessInputElement
		{
			public IRequestHandler RequestHandler { get; private set; }
			public Request Request { get; private set; }
			public Response Response { get; private set; }

			public bool RequestHandlerWasExecuted { get; private set; }

			public ProcessInputElement(IRequestHandler requestHandler, Request request, Response response)
			{
				RequestHandler = requestHandler;
				Request = request;
				Response = response;
			}

			public void StubHandler()
			{
				RequestHandler
					.Stub(r => r.Handle(Arg<Request>.Is.Same(Request))) // WHY: reference check iso equality check
					.Return(Response)
					.WhenCalled(a => RequestHandlerWasExecuted = true)
					.Repeat.Once();
			}
		}

		public class ProcessRequestsTransition : MetaTransition<QuickNet.Types.Tuple<ProcessInputElement, ProcessInputElement>, Response[]>
		{
			public ProcessRequestsTransition()
			{
                GenerateInput =
                    () =>
                    {
                        GeneratorRepository repo =
                            new GeneratorRepository()
                                .With<string>(mi => true, new StringGenerator(1, 1))
                                .With<int>(mi => true, new IntGenerator(0, 5));

                        var firstElement =
                            new ProcessInputElement(
                                firstCachedRequestHandler,
                                repo.Random<FirstCachedRequest>(),
                                new FirstCachedResponse());

                        var secondElement =
                            new ProcessInputElement(
                                secondCachedRequestHandler,
                                repo.Random<SecondCachedRequest>(),
                                new SecondCachedResponse());

                        return Tuple.New(firstElement, secondElement);
                    };

				Execute =
					input =>
						{
							cacheManager.Clear(); // clears the spy for every execution of this transition
                            cacheManager.ExceptionsThrown = new List<Exception>();
							input.First.StubHandler();
							input.Second.StubHandler();

							return requestProcessor.Process(new[] {input.First.Request, input.Second.Request});
						};
			}
		}

		[SpecFor(typeof(ProcessRequestsTransition))]
		public Spec ResponsesAreCachedIfTheyArentInTheCacheYet(QuickNet.Types.Tuple<ProcessInputElement, ProcessInputElement> input, Response[] output)
		{
			return new Spec(() =>
			                	{
			                		Action<ProcessInputElement> verify =
			                			element =>
			                				{
												Ensure.Equal(element.Response, cacheManager.CacheEntries.First(e => e.Request.Equals(element.Request)).Response);
												Ensure.True(output.Contains(element.Response));
												Ensure.True(element.RequestHandlerWasExecuted);
											};

			                		if (cacheManager.CacheEntries.Any(e => e.Request.Equals(input.First.Request)))
			                		{
			                			verify(input.First);
									}

			                		if (cacheManager.CacheEntries.Any(e => e.Request.Equals(input.Second.Request)))
			                		{
			                			verify(input.Second);
			                		}
								})
				.IfAfter(() => cacheManager.ReturnedCachedResponses.Any(r => r == null));
		}

		[SpecFor(typeof(ProcessRequestsTransition))]
		public Spec CachedResponsesAreReturnedWhenAvailableInsteadOfCallingTheHandler(QuickNet.Types.Tuple<ProcessInputElement, ProcessInputElement> input, Response[] output)
		{
			return new Spec(() =>
								{
									Action<ProcessInputElement> verify =
										element =>
											{
												Ensure.False(element.RequestHandlerWasExecuted);
												Ensure.True(output.Contains(cacheManager.ReturnedCachedResponses.First(r => r != null && r.GetType() == element.Response.GetType())));
											};

									if (cacheManager.ReturnedCachedResponses.Any(r => r != null &&  r.GetType() == input.First.Response.GetType()))
									{
										verify(input.First);
									}

									if (cacheManager.ReturnedCachedResponses.Any(r => r != null && r.GetType() == input.Second.Response.GetType()))
									{
										verify(input.Second);
									}
								})
				.IfAfter(() => cacheManager.ReturnedCachedResponses.Any(r => r != null));
		}
	}
}