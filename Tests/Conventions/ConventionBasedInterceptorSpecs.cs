using Agatha.Castle;
using Agatha.Common;
using Agatha.Common.Interceptors;
using Agatha.Common.InversionOfControl;
using Rhino.Mocks;
using Tests.RequestProcessorTests;
using Xunit;

namespace Tests.Conventions
{
    public class ConventionBasedInterceptorSpecs
    {
        [Fact]
        public void Should_create_a_default_response_based_on_the_conventions()
        {
            var request = new FirstRequest();
            var conventions = MockRepository.GenerateMock<IConventions>();
            conventions.Stub(c => c.GetResponseTypeFor(request)).Return(typeof(FirstResponse));
            IoC.Container = new Container();
            IoC.Container.RegisterInstance(conventions);

            var interceptorBase = new MockRepository().PartialMock<ConventionBasedInterceptor>();
            var response = interceptorBase.CreateDefaultResponseFor(request);
            Assert.IsType<FirstResponse>(response);
        }
    }
}