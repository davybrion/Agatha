using System;
using Agatha.Common.Configuration;
using Agatha.Common;
using Agatha.ServiceLayer.Conventions;
using Rhino.Mocks;
using Tests.ConfigurationTests;
using Xunit;

namespace Tests.Conventions
{
    public class When_resolving_the_ResponseType_using_RequestHandlerBasedConventions
        : BddSpecs
    {
        private IConventions conventions;
        private Type responseType;

        protected override void Given()
        {
            var requestHandlerRegistry = MockRepository.GenerateStub<IRequestHandlerRegistry>();
            requestHandlerRegistry.Stub(r => r.GetTypedRequestHandlers()).Return(new[] { typeof(RequestHandlerA) });
            conventions = new RequestHandlerBasedConventions(requestHandlerRegistry);
        }

        protected override void When()
        {
            responseType = conventions.GetResponseTypeFor(new RequestA());
        }

        [Fact]
        public void The_response_type_should_be_the_type_returned_by_the_request_handler()
        {
            Assert.Equal(responseType, typeof(ResponseA));
        }
    }
}