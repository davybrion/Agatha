using System;
using Agatha.Common;
using Agatha.Common.Configuration;
using Agatha.Common.Conventions;
using Rhino.Mocks;
using Xunit;

namespace Tests.Conventions
{
    public class When_resolving_the_ResponseType_using_BasicConventions_given_one_exists
        : BddSpecs
    {
        private IConventions conventions;
        private Type responseType;

        protected override void Given()
        {
            var requestTypeRegistry = MockRepository.GenerateStub<IRequestTypeRegistry>();
            requestTypeRegistry.Stub(r => r.GetRegisteredRequestTypes()).Return(new[] {typeof (TestRequest)});
            conventions = new BasicConventions(requestTypeRegistry);
        }

        protected override void When()
        {
            responseType = conventions.GetResponseTypeFor(new TestRequest());
        }

        [Fact]
        public void The_Request_suffix_should_be_replaced_by_the_Response_suffix()
        {
            Assert.Equal(responseType.Name, "TestResponse");
        }
    }

    public class When_resolving_the_RequestType_using_BasicConventions_given_one_exists
        : BddSpecs
    {
        private IConventions conventions;
        private Type requestType;

        protected override void Given()
        {
            var requestTypeRegistry = MockRepository.GenerateStub<IRequestTypeRegistry>();
            requestTypeRegistry.Stub(r => r.GetRegisteredRequestTypes()).Return(new[] { typeof(TestRequest) });
            conventions = new BasicConventions(requestTypeRegistry);
        }

        protected override void When()
        {
            requestType = conventions.GetRequestTypeFor(new TestResponse());
        }

        [Fact]
        public void The_Response_suffix_should_be_replaced_by_the_Request_suffix()
        {
            Assert.Equal(requestType.Name, "TestRequest");
        }
    }

    public class When_creating_BasicConventions_given_a_request_type_without_corresponding_response_type
         : BddSpecs
    {
        private Exception catchedException;
        private IRequestTypeRegistry requestTypeRegistry;

        protected override void Given()
        {
            requestTypeRegistry = MockRepository.GenerateStub<IRequestTypeRegistry>();
            requestTypeRegistry.Stub(r => r.GetRegisteredRequestTypes()).Return(new[] { typeof(TestRequestWithoutResponseRequest) });
        }

        protected override void When()
        {
            try
            {
                new BasicConventions(requestTypeRegistry);
            }
            catch (Exception exc)
            {
                catchedException = exc;
            }
        }

        [Fact]
        public void An_exception_should_be_thrown()
        {
            Assert.IsType<InvalidOperationException>(catchedException);
            Assert.Equal(catchedException.Message,
                "Could not determine response type by convention for request of type " + typeof(TestRequestWithoutResponseRequest).FullName);
        }
    }

    public class TestRequest : Request
    {
    }

    public class TestResponse : Response
    {
    }

    public class TestRequestWithoutResponseRequest : Request
    {
    }
}