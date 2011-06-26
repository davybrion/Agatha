using System;
using System.Threading;
using Agatha.Common;
using Agatha.ServiceLayer;
using QuickDotNetCheck;
using QuickGenerate;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Tests.RequestProcessorTests.Async
{
    public class ProcessRequestsAsynchronously : Fixture
    {
        public class ProcessRequestsAsynchronouslyInput
        {
            public Func<Tuple<Request[], Response[]>, IMethodOptions<object>> Stub { get; set; }
            public readonly Tuple<Request[], Response[]> requestResponsePair =
                Tuple.Create(new Request[0], new Response[0]);
        }

        private ProcessRequestsAsynchronouslyInput input;
        public static ProcessRequestsAsyncCompletedArgs output;

        private static IRequestProcessor requestProcessor;
        private static AsyncRequestProcessor asyncRequestProcessor;

        public ProcessRequestsAsynchronously()
        {
            requestProcessor = MockRepository.GenerateMock<IRequestProcessor>();
            asyncRequestProcessor = new AsyncRequestProcessor(requestProcessor);
        }

        
        private static bool lastProcessRequestsThrewException;

        public static ProcessRequestsAsynchronouslyInput lastProcessRequestInput;

        public static bool LastRequestThrewAnException()
        {
            return lastProcessRequestsThrewException;
        }

        public static bool LastRequestDidNotThrowAnException()
        {
            return !lastProcessRequestsThrewException;
        }

        public override void Arrange()
        {
            input =
                new DomainGenerator()
                    .With<ProcessRequestsAsynchronouslyInput>(
                        g => g.For(i => i.Stub,

                                   inputpair => requestProcessor.Stub<IRequestProcessor>(
                                       r => r.Process(inputpair.Item1))
                                                    .Return(inputpair.Item2)
                                                    .Repeat.Once()
                                                    .WhenCalled(arg => lastProcessRequestsThrewException = false),

                                   inputpair => requestProcessor.Stub<IRequestProcessor>(
                                       r => r.Process(inputpair.Item1))
                                                    .Return(inputpair.Item2)
                                                    .Repeat.Once()
                                                    .WhenCalled(arg =>
                                                                    {
                                                                        lastProcessRequestsThrewException = true;
                                                                        throw new Exception();
                                                                    })))
                    .One<ProcessRequestsAsynchronouslyInput>();
        }

        protected override void Act()
        {
            lastProcessRequestInput = input;
            lastProcessRequestsThrewException = false;
            output = null;
            input.Stub(input.requestResponsePair);
            asyncRequestProcessor.ProcessRequestsAsync(input.requestResponsePair.Item1, args => output = args);
            // this uglyness is only here because of the async stuff
            int counter = 0;
            while (output == null)
            {
                if (++counter == 20)
                {
                    throw new InvalidOperationException("time out... the callback should've been called already");
                }
                Thread.Sleep(10);
            }
        }
        
        public Spec ProcessRequestsAsyncCompletedShouldNotTimeout()
        {
            return new Spec(() => { }); //throws InvalidOperationException if it does timeout
        }
    }
}