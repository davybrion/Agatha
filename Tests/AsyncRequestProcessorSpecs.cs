using System;
using System.Reflection;
using System.Threading;
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

using RequestProcessorFunction = System.Func<QuickNet.Types.Tuple<Agatha.Common.Request[], Agatha.Common.Response[]>, Rhino.Mocks.Interfaces.IMethodOptions<object>>;
using Tuple = QuickNet.Types.Tuple;
using QuickGenerate.Uber;

namespace Tests
{
    public class AsyncRequestProcessorSpecs : AcidTest
    {
        #region Setup

        private static IRequestProcessor requestProcessor;
        private static AsyncRequestProcessor asyncRequestProcessor;

        public AsyncRequestProcessorSpecs() : base(10, 10) { }

        public override void Setup()
        {
            requestProcessor = MockRepository.GenerateMock<IRequestProcessor>();
            asyncRequestProcessor = new AsyncRequestProcessor(requestProcessor);
        }

        #endregion

        #region Context

        private static ProcessRequestsAsyncCompletedArgs lastProcessRequestsAsyncCompletedArgs;
        private static bool lastProcessRequestsThrewException;
        private static ProcessRequestsAsynchronouslyInput lastProcessRequestInput;

        public class ProcessRequestsAsynchronouslyInput
        {
            public RequestProcessorFunction Stub { get; set; }
            public readonly QuickNet.Types.Tuple<Request[], Response[]> RequestResponsePair = 
                Tuple.New(new Request[0], new Response[0]);
        }

        private static bool LastRequestThrewAnException()
        {
            return lastProcessRequestsThrewException;
        }

        private static bool LastRequestDidNotThrowAnException()
        {
            return !lastProcessRequestsThrewException;
        }
        #endregion

        #region Transitions 

        class ProcessRequestsAsynchronously : MetaTransition<ProcessRequestsAsynchronouslyInput, ProcessRequestsAsyncCompletedArgs>
        {
            public ProcessRequestsAsynchronously()
            {
                GenerateInput =
                    () => new GeneratorRepository()
                        .Random<ProcessRequestsAsynchronouslyInput>(
                             g => g.For(i => i.Stub, 
                                 
                                 input => requestProcessor.Stub<IRequestProcessor>(
                                             r => r.Process(input.First))
                                             .Return(input.Second)
                                             .Repeat.Once()
                                             .WhenCalled(arg => lastProcessRequestsThrewException = false),

                                input => requestProcessor.Stub<IRequestProcessor>(
                                             r => r.Process(input.First))
                                             .Return(input.Second)
                                             .Repeat.Once()
                                             .WhenCalled(arg =>
                                                 {
                                                     lastProcessRequestsThrewException = true;
                                                     throw new Exception();
                                                 })));
                Execute =
                    input =>
                    {
                        lastProcessRequestInput = input;
                        lastProcessRequestsThrewException = false;
                        lastProcessRequestsAsyncCompletedArgs = null;
                        input.Stub(input.RequestResponsePair);
                        asyncRequestProcessor.ProcessRequestsAsync(input.RequestResponsePair.First, args => lastProcessRequestsAsyncCompletedArgs = args);
                        // this uglyness is only here because of the async stuff
                        int counter = 0;
                        while (lastProcessRequestsAsyncCompletedArgs == null)
                        {
                            if (++counter == 20)
                            {
                                throw new InvalidOperationException("time out... the callback should've been called already");
                            }
                            Thread.Sleep(10);
                        }
                        return lastProcessRequestsAsyncCompletedArgs;
                    };
            }
        }

        class GetResults : MetaTransition<ProcessRequestsAsyncCompletedArgs, Response[]>
        {
            public GetResults()
            {
                Precondition = () => lastProcessRequestsAsyncCompletedArgs != null;
                GenerateInput = () => lastProcessRequestsAsyncCompletedArgs;
                Execute = input => input.Result; 
            }
        }

        class GetError : MetaTransition<ProcessRequestsAsyncCompletedArgs, Exception>
        {
            public GetError()
            {
                Precondition = () => lastProcessRequestsAsyncCompletedArgs != null;
                GenerateInput = () => lastProcessRequestsAsyncCompletedArgs;
                Execute = input => input.Error;
            }
        }

        #endregion

        [SpecFor(typeof(ProcessRequestsAsynchronously))]
        public Spec ProcessRequestsAsyncCompletedShouldNotTimeout(ProcessRequestsAsynchronouslyInput input, ProcessRequestsAsyncCompletedArgs output)
        {
            return new Spec(); //throws InvalidOperationException if it does timeout
        }

        [SpecFor(typeof(GetResults))]
        public Spec GetResultsIfAllGoesWell(ProcessRequestsAsyncCompletedArgs input, Response[] output)
        {
            return new Spec(() => Ensure.Equal(lastProcessRequestInput.RequestResponsePair.Second, output))
                .If(LastRequestDidNotThrowAnException);
        }

        [SpecFor(typeof(GetResults))]
        public Spec GetResultsIfSomethingGoesWrong(ProcessRequestsAsyncCompletedArgs input, Response[] output)
        {
            return new Spec().Throws<TargetInvocationException>()
                .If(LastRequestThrewAnException);
        }

        [SpecFor(typeof(GetError))]
        public Spec GetErrorIfAllGoesWell(ProcessRequestsAsyncCompletedArgs input, Exception output)
        {
            return new Spec(() => Ensure.Null(output))
                .If(LastRequestDidNotThrowAnException);
        }

        [SpecFor(typeof(GetError))]
        public Spec GetErrorIfSomethingGoesWrong(ProcessRequestsAsyncCompletedArgs input, Exception output)
        {
            return new Spec(() => Ensure.NotNull(output))
                .If(LastRequestThrewAnException);
        }
    }
}