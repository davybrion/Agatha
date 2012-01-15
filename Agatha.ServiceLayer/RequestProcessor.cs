using System;
using System.Collections.Generic;
using System.Linq;
using Agatha.Common;
using Agatha.Common.InversionOfControl;
using Common.Logging;

namespace Agatha.ServiceLayer
{
    public class RequestProcessor : Disposable, IRequestProcessor
    {
        private readonly ServiceLayerConfiguration serviceLayerConfiguration;
        private readonly ILog logger = LogManager.GetLogger(typeof(RequestProcessor));
        private readonly IRequestProcessingErrorHandler errorHandler;

        protected override void DisposeManagedResources()
        {
            // empty by default but you should override this in derived classes so you can clean up your resources
        }

        public RequestProcessor(ServiceLayerConfiguration serviceLayerConfiguration, IRequestProcessingErrorHandler errorHandler)
        {
            this.serviceLayerConfiguration = serviceLayerConfiguration;
            this.errorHandler = errorHandler;
        }

        protected virtual void BeforeProcessing(IEnumerable<Request> requests) { }

        protected virtual void AfterProcessing(IEnumerable<Request> requests, IEnumerable<Response> responses) { }

        protected virtual void BeforeHandle(Request request) { }

        protected virtual void AfterHandle(Request request) { }

        protected virtual void AfterHandle(Request request, Response response) { }

        protected virtual void BeforeResolvingRequestHandler(Request request) { }

        public Response[] Process(params Request[] requests)
        {
            if (requests == null) return null;

            var exceptionsPreviouslyOccurred = false;

            BeforeProcessing(requests);

            var processingContexts = requests.Select(request => new RequestProcessingContext(request)).ToList();
            foreach (var requestProcessingState in processingContexts)
            {
                if (exceptionsPreviouslyOccurred)
                {
                    errorHandler.DealWithPreviouslyOccurredExceptions(requestProcessingState);
                    continue;
                }
                IList<IRequestHandlerInterceptor> interceptors = new List<IRequestHandlerInterceptor>();
                IList<IRequestHandlerInterceptor> invokedInterceptors = new List<IRequestHandlerInterceptor>();
                try
                {
                    interceptors = ResolveInterceptors();
                    foreach (var interceptor in interceptors)
                    {
                        interceptor.BeforeHandlingRequest(requestProcessingState);
                        invokedInterceptors.Add(interceptor);
                        if (requestProcessingState.IsProcessed) break;
                    }

                    if (!requestProcessingState.IsProcessed)
                    {
                        InvokeRequestHandler(requestProcessingState);
                    }
                }
                catch (Exception exc)
                {
                    logger.Error(exc);
                    exceptionsPreviouslyOccurred = true;
                    errorHandler.DealWithException(requestProcessingState, exc);
                }
                finally
                {
                    try
                    {
                        var possibleExceptionsFromInterceptors = RunInvokedInterceptorsSafely(requestProcessingState, invokedInterceptors);

                        if (possibleExceptionsFromInterceptors.Count() > 0)
                        {
                            foreach (var exceptionFromInterceptor in possibleExceptionsFromInterceptors)
                            {
                                logger.Error(exceptionFromInterceptor);
                            }
                            exceptionsPreviouslyOccurred = true;
                            errorHandler.DealWithException(requestProcessingState, possibleExceptionsFromInterceptors.ElementAt(0));
                        }
                    }
                    finally
                    {
                        DisposeInterceptorsSafely(interceptors);
                    }
                }
            }
            var responses = processingContexts.Select(c => c.Response).ToArray();

            AfterProcessing(requests, responses);

            return responses;
        }

        private void InvokeRequestHandler(RequestProcessingContext requestProcessingState)
        {
            var request = requestProcessingState.Request;

            BeforeResolvingRequestHandler(request);

            using (var handler = (IRequestHandler)IoC.Container.Resolve(GetRequestHandlerTypeFor(request)))
            {
                try
                {
                    var response = GetResponseFromHandler(request, handler);
                    requestProcessingState.MarkAsProcessed(response);
                }
                finally
                {
                    IoC.Container.Release(handler);
                }
            }
        }

        private IEnumerable<Exception> RunInvokedInterceptorsSafely(RequestProcessingContext requestProcessingState, IList<IRequestHandlerInterceptor> invokedInterceptors)
        {
            var exceptionsFromInterceptor = new List<Exception>();
            foreach (var interceptor in invokedInterceptors.Reverse())
            {
                try
                {
                    interceptor.AfterHandlingRequest(requestProcessingState);
                }
                catch (Exception exc)
                {
                    exceptionsFromInterceptor.Add(exc);
                }
            }

            return exceptionsFromInterceptor;
        }

        private void DisposeInterceptorsSafely(IList<IRequestHandlerInterceptor> interceptors)
        {
            if (interceptors == null)
            {
                return;
            }

            foreach (var interceptor in interceptors.Reverse())
            {
                try
                {
                    IoC.Container.Release(interceptor);
                    interceptor.Dispose();
                }
                catch (Exception exc)
                {
                    logger.Error("error disposing " + interceptor, exc);
                }
            }
        }

        private IList<IRequestHandlerInterceptor> ResolveInterceptors()
        {
            return serviceLayerConfiguration.GetRegisteredInterceptorTypes()
                .Select(t => (IRequestHandlerInterceptor)IoC.Container.Resolve(t)).ToList();
        }

        private static Type GetRequestHandlerTypeFor(Request request)
        {
            // get a type reference to IRequestHandler<ThisSpecificRequestType>
            return typeof(IRequestHandler<>).MakeGenericType(request.GetType());
        }

        private Response GetResponseFromHandler(Request request, IRequestHandler handler)
        {
            try
            {
                BeforeHandle(request);
                var response = handler.Handle(request);
                AfterHandle(request);
                AfterHandle(request, response);
                return response;
            }
            catch (Exception e)
            {
                OnHandlerException(request, e);
                throw;
            }
        }

        protected virtual void OnHandlerException(Request request, Exception exception)
        {
            logger.Error("RequestProcessor: unhandled exception while handling request!", exception);
        }

        public void ProcessOneWayRequests(params OneWayRequest[] requests)
        {
            if (requests == null) return;

            BeforeProcessing(requests);

            DispatchRequestsToHandlers(requests);

            AfterProcessing(requests, null);
        }

        private void DispatchRequestsToHandlers(OneWayRequest[] requests)
        {
            foreach (var request in requests)
            {
                try
                {
                    BeforeResolvingRequestHandler(request);

                    using (var handler = (IOneWayRequestHandler)IoC.Container.Resolve(GetOneWayRequestHandlerTypeFor(request)))
                    {
                        try
                        {
                            ExecuteHandler(request, handler);
                        }
                        finally
                        {
                            IoC.Container.Release(handler);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e);
                    throw;
                }
            }
        }

        private static Type GetOneWayRequestHandlerTypeFor(Request request)
        {
            return typeof(IOneWayRequestHandler<>).MakeGenericType(request.GetType());
        }

        private void ExecuteHandler(OneWayRequest request, IOneWayRequestHandler handler)
        {
            try
            {
                BeforeHandle(request);
                handler.Handle(request);
                AfterHandle(request);
            }
            catch (Exception e)
            {
                OnHandlerException(request, e);
            }
        }

        protected virtual void OnHandlerException(OneWayRequest request, Exception exception)
        {
            logger.Error("RequestProcessor: unhandled exception while handling request!", exception);
        }
    }
}