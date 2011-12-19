using Agatha.Castle;
using Agatha.Common;
using Agatha.Common.Caching;
using Agatha.Common.InversionOfControl;
using Agatha.ServiceLayer;
using Rhino.Mocks;
using Tests.ConfigurationTests;
using Xunit;

namespace Tests
{
    public class RequestProcessingErrorHandlerSpecs
    {
        public class Given_exceptions_previously_occurred_during_processing
            : RequestProcessingErrorHandlingSpecs
        {
            protected override void EstablishContext()
            {
                //This is the last fallback logic -> use base Response type if the real response type can't be determined by other means
            }

            [Fact]
            public void The_ExceptionType_should_be_EarlierRequestAlreadyFailed()
            {
                Assert.Equal(ExceptionType.EarlierRequestAlreadyFailed, Context.Response.ExceptionType);
            }

            [Fact]
            public void The_response_should_contain_an_ExceptionInfo_explaining_the_error()
            {
                Assert.Equal(ExceptionType.EarlierRequestAlreadyFailed.ToString(), Context.Response.Exception.Message);
            }

            [Fact]
            public void The_request_should_be_marked_as_processed()
            {
                Assert.True(Context.IsProcessed);
            }
        }

        public class Given_conventions_are_registered
            : Given_exceptions_previously_occurred_during_processing
        {
            protected override void EstablishContext()
            {
                RegisterConventions();
                CacheResponse();
                RegisterRequestHandler();
            }

            [Fact]
            public void The_response_should_be_based_on_the_conventions()
            {
                Assert.IsType(typeof(ConventionBasedResponse), Context.Response);
            }
        }

        public class Given_no_coventions_registered_but_a_response_is_cached
            : Given_exceptions_previously_occurred_during_processing
        {
            protected override void EstablishContext()
            {
                CacheResponse();
                RegisterRequestHandler();
            }

            [Fact]
            public void The_type_of_the_cached_response_should_be_used()
            {
                Assert.IsType(typeof(CachedResponse), Context.Response);
            }    
        }

        public class Given_no_conventions_nor_cached_response_but_registered_RequestHandler
            : Given_exceptions_previously_occurred_during_processing
        {
            protected override void EstablishContext()
            {
                RegisterRequestHandler();
            }

            [Fact]
            public void The_default_response_created_by_the_requesthandler_should_be_used()
            {
                Assert.IsType(typeof(ResponseCreatedByRequestHandler), Context.Response);
            }   
        }

        public class CachedResponse : Response
        {
        }

        public class ResponseCreatedByRequestHandler : Response
        {
        }

        public class ConventionBasedResponse : Response
        {
        }

        public abstract class RequestProcessingErrorHandlingSpecs : BddSpecs
        {
            protected RequestProcessingContext Context;
            private Request request;
            private IRequestProcessingErrorHandler errorHandler;
            private ICacheManager cacheManager;

            protected IContainer Container
            {
                get { return IoC.Container; }
            }

            protected override void Given()
            {
                IoC.Container = new Container();
                errorHandler = new RequestProcessingErrorHandler(new ServiceLayerConfiguration(IoC.Container));
                request = new RequestA();
                Context = new RequestProcessingContext(request);
                cacheManager = Stubbed<ICacheManager>();
                Container.RegisterInstance(cacheManager);
                EstablishContext();
            }

            protected abstract void EstablishContext();

            protected override void When()
            {
                errorHandler.DealWithPreviouslyOccurredExceptions(Context);
            }

            protected void RegisterConventions()
            {
                var conventions = Stubbed<IConventions>();
                conventions.Stub(c => c.GetResponseTypeFor(Context.Request)).Return(typeof(ConventionBasedResponse));
                Container.RegisterInstance(conventions);
            }

            protected void CacheResponse()
            {
                cacheManager.Stub(cm => cm.IsCachingEnabledFor(request.GetType())).Return(true);
                cacheManager.Stub(cm => cm.GetCachedResponseFor(request)).Return(new CachedResponse());
            }

            protected void RegisterRequestHandler()
            {
                var requestHandler = Stubbed<IRequestHandler<RequestA>>();
                requestHandler.Stub(rh => rh.CreateDefaultResponse()).Return(new ResponseCreatedByRequestHandler());
                Container.RegisterInstance(requestHandler);
            }
        }
    }
}