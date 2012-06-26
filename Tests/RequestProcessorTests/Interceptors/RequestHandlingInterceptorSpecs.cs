using System.Linq;
using Agatha.Common;
using Agatha.Common.InversionOfControl;
using Agatha.ServiceLayer;
using Xunit;

namespace Tests.RequestProcessorTests.Interceptors
{
    public class Given_a_RequestHandlerInterceptor_is_registered
        : BddSpecs
    {
        private SpyRequest request;
        private Response response;

        protected override void Given()
        {
            IoC.Container = null;
            new ServiceLayerConfiguration(GetType().Assembly, GetType().Assembly, typeof(Agatha.Castle.Container))
               .RegisterRequestHandlerInterceptor<TestInterceptor>()
               .Initialize();

            request = new SpyRequest();
        }

        protected override void When()
        {
            using (var requestProcessor = IoC.Container.Resolve<IRequestProcessor>())
            {
                response = requestProcessor.Process(request).SingleOrDefault();
            }
        }

        [Fact]
        public void The_response_should_not_contain_errors()
        {
            Assert.True(response.Exception == null, response.Exception != null ? response.Exception.Message : "");
            Assert.True(response.ExceptionType == ExceptionType.None);
        }

        [Fact]
        public void Its_BeforeHandlingRequest_method_should_be_invoked_before_handling_a_request()
        {
            Assert.True(request.BeforeProcessingTimeStamp < request.RequestHandlingTimeStamp);
        }

        [Fact]
        public void Its_AfterHandlingRequest_method_should_be_invoked_after_handling_a_request()
        {
            Assert.True(request.AfterProcessingTimeStamp > request.RequestHandlingTimeStamp);
        }

        [Fact]
        public void The_request_should_be_handled_by_the_RequestHandler()
        {
            Assert.IsType<SpyResponse>(response);
        }

        [Fact]
        public void The_response_of_the_RequestHandler_should_be_added_to_the_RequestProcessingContext()
        {
            Assert.Equal(request.ResponseFromContext, response);
        }
    }

    public class Given_a_RequestHandlerInterceptor_marks_a_request_as_processed
        : BddSpecs
    {
        private InterceptedSpyRequest request;
        private Response response;

        protected override void Given()
        {
            IoC.Container = null;
            request = new InterceptedSpyRequest();
            new ServiceLayerAndClientConfiguration(typeof(ThrowingRequestHandler).Assembly, GetType().Assembly, new Agatha.Castle.Container())
               .RegisterRequestHandlerInterceptor<InterceptingInterceptor>()
               .RegisterRequestHandlerInterceptor<SubSequentInterceptor>()
               .Initialize();
        }

        protected override void When()
        {
            using (var processor = IoC.Container.Resolve<IRequestProcessor>())
            {
                response = processor.Process(request).Single();
            }
        }

        [Fact]
        public void SubSequent_interceptors_should_not_be_invoked()
        {
            Assert.False(request.ProcessedBySubSequentInterceptorBeforeHandling);
            Assert.False(request.ProcessedBySubSequentInterceptorAfterHandling);
        }

        [Fact]
        public void The_request_should_not_be_handled()
        {
            Assert.Equal(response.ExceptionType, ExceptionType.None);
        }

        [Fact]
        public void The_response_of_the_interceptor_should_be_returned()
        {
            Assert.IsType<ResponseFromInterceptor>(response);
        }
    }

    public class Given_two_interceptors_are_registered
        : BddSpecs
    {
        private SpyRequest request;

        protected override void Given()
        {
            IoC.Container = null;
            new ServiceLayerConfiguration(GetType().Assembly, GetType().Assembly, new Agatha.Castle.Container())
               .RegisterRequestHandlerInterceptor<TestInterceptor>()
               .RegisterRequestHandlerInterceptor<SubSequentInterceptor>()
               .Initialize();

            request = new SpyRequest();
        }

        protected override void When()
        {
            using (var requestProcessor = IoC.Container.Resolve<IRequestProcessor>())
            {
                requestProcessor.Process(request).SingleOrDefault();
            }
        }

        [Fact]
        public void Their_BeforeHandlingRequest_method_should_be_invoked_in_the_order_they_where_registered()
        {
            Assert.True(request.BeforeProcessingTimeStamp < request.SubSequentInterceptorBeforeProcessingTimeStamp);
        }

        [Fact]
        public void Their_AfterHandlingRequest_method_should_be_invoked_in_the_opposite_order_they_where_registered()
        {
            Assert.True(request.SubSequentInterceptorAfterProcessingTimeStamp < request.AfterProcessingTimeStamp);
        }
    }

    public class Given_the_first_request_fails
        : BddSpecs
    {
        private InterceptedSpyRequest erroneousRequest;
        private SpyRequest subsequentRequest;
        private SpyRequest anotherSubsequentRequest;
        private Response[] responses;
        private SpyRequest[] subsequentRequests;

        protected override void Given()
        {
            erroneousRequest = new InterceptedSpyRequest();
            subsequentRequest = new SpyRequest();
            anotherSubsequentRequest = new SpyRequest();
            subsequentRequests = new[] { subsequentRequest, anotherSubsequentRequest };
            new ServiceLayerAndClientConfiguration(typeof(ThrowingRequestHandler).Assembly, GetType().Assembly, new Agatha.Castle.Container())
                    .RegisterRequestHandlerInterceptor<TestInterceptor>()
                    .Initialize();
        }

        protected override void When()
        {
            using (var processor = IoC.Container.Resolve<IRequestProcessor>())
            {
                responses = processor.Process(new[] { erroneousRequest, subsequentRequest, anotherSubsequentRequest });
            }
        }

        [Fact]
        public void The_BeforeHandlingRequest_should_not_be_invoked_for_the_subsequent_requests()
        {
            foreach (var request in subsequentRequests)
            {
                Assert.Null(request.BeforeProcessingTimeStamp);
            }
        }

        [Fact]
        public void The_AfterHandlingRequest_should_not_be_invoked_for_the_subsequent_requests()
        {
            foreach (var request in subsequentRequests)
            {
                Assert.Null(request.AfterProcessingTimeStamp);
            }
        }

        [Fact]
        public void The_responses_for_the_subsequent_requests_should_contain_an_exception_info()
        {
            foreach (var response in responses.Skip(1))
            {
                Assert.Equal(ExceptionType.EarlierRequestAlreadyFailed, response.ExceptionType);
                Assert.NotNull(response.Exception);
            }
        }

        [Fact]
        public void The_AfterHandlingRequest_should_still_be_invoked_for_the_first_request()
        {
            Assert.NotNull(erroneousRequest.AfterProcessingTimeStamp);
        }
    }

    public class Given_the_first_interceptor_fails_on_BeforeHandlingRequest
        : BddSpecs
    {
        SpyRequest request;
        Response response;

        protected override void Given()
        {
            IoC.Container = null;
            new ServiceLayerConfiguration(GetType().Assembly, GetType().Assembly, typeof(Agatha.Castle.Container))
               .RegisterRequestHandlerInterceptor<FailingBeforeHandlingRequestInterceptor>()
               .RegisterRequestHandlerInterceptor<SubSequentInterceptor>()
               .Initialize();

            request = new SpyRequest();
        }

        protected override void When()
        {
            using (var requestProcessor = IoC.Container.Resolve<IRequestProcessor>())
            {
                response = requestProcessor.Process(request).SingleOrDefault();
            }
        }

        [Fact]
        public void The_subsequent_interceptor_should_not_be_called()
        {
            Assert.Null(request.SubSequentInterceptorBeforeProcessingTimeStamp);
            Assert.Null(request.SubSequentInterceptorAfterProcessingTimeStamp);
        }

        [Fact]
        public void The_AfterHandlingRequest_should_not_be_called_for_first_interceptor()
        {
            Assert.Null(request.AfterProcessingTimeStamp);
        }

        [Fact]
        public void The_response_should_contain_error()
        {
            Assert.NotNull(response.Exception);
            Assert.NotEqual(response.ExceptionType, ExceptionType.None);
        }

        [Fact]
        public void All_interceptors_are_disposed()
        {
            Assert.True(FailingBeforeHandlingRequestInterceptor.Disposed);
            Assert.True(SubSequentInterceptor.Disposed);
        }
    }

    public class Given_the_second_interceptor_fails_on_AfterHandlingRequest
        : BddSpecs
    {
        SpyRequest request;
        Response response;

        protected override void Given()
        {
            IoC.Container = null;
            new ServiceLayerConfiguration(GetType().Assembly, GetType().Assembly, typeof(Agatha.Castle.Container))
               .RegisterRequestHandlerInterceptor<TestInterceptor>()
               .RegisterRequestHandlerInterceptor<FailingBeforeHandlingRequestInterceptor>()
               .Initialize();

            request = new SpyRequest();
        }

        protected override void When()
        {
            using (var requestProcessor = IoC.Container.Resolve<IRequestProcessor>())
            {
                response = requestProcessor.Process(request).SingleOrDefault();
            }
        }

        [Fact]
        public void The_first_interceptor_AfterHandlingRequest_should_be_called()
        {
            Assert.NotNull(request.AfterProcessingTimeStamp);
        }

        [Fact]
        public void The_response_should_contain_error()
        {
            Assert.NotNull(response.Exception);
            Assert.NotEqual(response.ExceptionType, ExceptionType.None);
        }

        [Fact]
        public void All_interceptors_are_disposed()
        {
            Assert.True(FailingBeforeHandlingRequestInterceptor.Disposed);
            Assert.True(TestInterceptor.Disposed);
        }
    }

    public class Given_the_request_is_a_OneWay_request : BddSpecs
    {
        private OneWaySpyRequest request;
        
        protected override void Given()
        {
            IoC.Container = null;
            new ServiceLayerConfiguration(GetType().Assembly, GetType().Assembly, typeof(Agatha.Castle.Container))
               .RegisterRequestHandlerInterceptor<OneWaySpyRequestInterceptor>()
               .Initialize();

            request = new OneWaySpyRequest();
        }

        protected override void When()
        {
            using (var requestProcessor = IoC.Container.Resolve<IRequestProcessor>())
            {
                requestProcessor.ProcessOneWayRequests(request);
            }
        }

        [Fact]
        public void Its_BeforeHandlingRequest_method_should_be_invoked_before_handling_a_request()
        {
            Assert.True(request.BeforeProcessingTimeStamp < request.RequestHandlingTimeStamp);
        }

        [Fact]
        public void Its_AfterHandlingRequest_method_should_be_invoked_after_handling_a_request()
        {
            Assert.True(request.AfterProcessingTimeStamp > request.RequestHandlingTimeStamp);
        }

        [Fact]
        public void All_interceptors_are_disposed()
        {
            Assert.True(OneWaySpyRequestInterceptor.Disposed);
        }

       
    }
}