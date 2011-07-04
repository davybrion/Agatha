using System;
using System.Collections.Generic;
using System.Linq;
using Agatha.Common;
using Agatha.Common.Caching;
using Agatha.Common.InversionOfControl;
using Common.Logging;

namespace Agatha.ServiceLayer
{
    public class RequestProcessor : Disposable, IRequestProcessor
    {
        private readonly ServiceLayerConfiguration serviceLayerConfiguration;
        private readonly ICacheManager cacheManager;
        private readonly ILog logger = LogManager.GetLogger(typeof(RequestProcessor));

        protected override void DisposeManagedResources()
        {
            // empty by default but you should override this in derived classes so you can clean up your resources
        }

        public RequestProcessor(ServiceLayerConfiguration serviceLayerConfiguration, ICacheManager cacheManager)
        {
            this.serviceLayerConfiguration = serviceLayerConfiguration;
            this.cacheManager = cacheManager;
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

            var responses = new List<Response>(requests.Length);

            bool exceptionsPreviouslyOccurred = false;

            BeforeProcessing(requests);

            foreach (var request in requests)
            {
                var requestProcessingState = new RequestProcessingContext(request);
                IList<IRequestHandlerInterceptor> interceptors = new List<IRequestHandlerInterceptor>();
                try
                {
                    IList<IRequestHandlerInterceptor> invokedInterceptors = new List<IRequestHandlerInterceptor>();
                    if (!exceptionsPreviouslyOccurred)
                    {
                        interceptors = ResolveInterceptors();
                        
                        foreach (var interceptor in interceptors)
                        {
                            interceptor.BeforeHandlingRequest(requestProcessingState);
                            invokedInterceptors.Add(interceptor);
                            if (requestProcessingState.IsProcessed)
                            {
                                responses.Add(requestProcessingState.Response);
                                break;
                            }
                        } 
                    }

                    if (!requestProcessingState.IsProcessed)
                    {
                        var skipHandler = false;
                        var cachingIsEnabledForThisRequest = cacheManager.IsCachingEnabledFor(request.GetType());

                        if (cachingIsEnabledForThisRequest)
                        {
                            var cachedResponse = cacheManager.GetCachedResponseFor(request);

                            if (cachedResponse != null)
                            {
                                if (exceptionsPreviouslyOccurred)
                                {
                                    var dummyResponse = Activator.CreateInstance(cachedResponse.GetType()) as Response;
                                    responses.Add(SetStandardExceptionInfoWhenEarlierRequestsFailed(dummyResponse));
                                    requestProcessingState.MarkAsProcessed(dummyResponse);
                                }
                                else
                                {
                                    responses.Add(cachedResponse);
                                    requestProcessingState.MarkAsProcessed(cachedResponse);
                                }

                                skipHandler = true;
                            }
                        }
                        if (!skipHandler)
                        {
                            BeforeResolvingRequestHandler(request);

                            using (var handler = (IRequestHandler) IoC.Container.Resolve(GetRequestHandlerTypeFor(request)))
                            {
                                try
                                {
                                    if (!exceptionsPreviouslyOccurred)
                                    {
                                        var response = GetResponseFromHandler(request, handler);
                                        exceptionsPreviouslyOccurred = response.ExceptionType != ExceptionType.None;
                                        responses.Add(response);
                                        requestProcessingState.MarkAsProcessed(response);

                                        if (response.ExceptionType == ExceptionType.None && cachingIsEnabledForThisRequest)
                                        {
                                            cacheManager.StoreInCache(request, response);
                                        }
                                    }
                                    else
                                    {
                                        var response = handler.CreateDefaultResponse();
                                        responses.Add(SetStandardExceptionInfoWhenEarlierRequestsFailed(response));
                                        requestProcessingState.MarkAsProcessed(response);
                                    }
                                }
                                finally
                                {
                                    IoC.Container.Release(handler);
                                }
                            }
                        }
                    }

                    foreach (var interceptor in invokedInterceptors.Reverse())
                    {
                        interceptor.AfterHandlingRequest(requestProcessingState);
                    }
                    invokedInterceptors.Clear();
                }
                catch (Exception exc)
                {
                    logger.Error(exc);
                    throw;
                }
                finally
                {
                    SaveDisposeInterceptors(interceptors);
                }
            }

            AfterProcessing(requests, responses);

            return responses.ToArray();
        }

        private void SaveDisposeInterceptors(IList<IRequestHandlerInterceptor> interceptors)
        {
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

        private Response SetStandardExceptionInfoWhenEarlierRequestsFailed(Response response)
        {
            response.ExceptionType = ExceptionType.EarlierRequestAlreadyFailed;
            response.Exception = new ExceptionInfo(new Exception(ExceptionType.EarlierRequestAlreadyFailed.ToString()));
            return response;
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
                return CreateExceptionResponse(handler, e);
            }
        }

        protected virtual void OnHandlerException(Request request, Exception exception)
        {
            logger.Error("RequestProcessor: unhandled exception while handling request!", exception);
        }

        protected virtual Response CreateExceptionResponse(IRequestHandler handler, Exception exception)
        {
            var response = handler.CreateDefaultResponse();
            response.Exception = new ExceptionInfo(exception);
            SetExceptionType(response, exception);
            return response;
        }

        private void SetExceptionType(Response response, Exception exception)
        {
            var exceptionType = exception.GetType();

            if (exceptionType.Equals(serviceLayerConfiguration.BusinessExceptionType))
            {
                response.ExceptionType = ExceptionType.Business;

                SetExceptionFaultCode(exception, response.Exception);

                return;
            }

            if (exceptionType.Equals(serviceLayerConfiguration.SecurityExceptionType))
            {
                response.ExceptionType = ExceptionType.Security;
                return;
            }

            response.ExceptionType = ExceptionType.Unknown;
        }

        private void SetExceptionFaultCode(Exception exception, ExceptionInfo exceptionInfo)
        {
            var businessExceptionType = exception.GetType();

            var faultCodeProperty = businessExceptionType.GetProperty("FaultCode");

            if (faultCodeProperty != null
                && faultCodeProperty.CanRead
                && faultCodeProperty.PropertyType.Equals(typeof(string)))
            {
                exceptionInfo.FaultCode = (string)faultCodeProperty.GetValue(exception, null);
            }
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