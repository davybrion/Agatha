using System;
using Agatha.Common;
using Agatha.Common.Caching;
using Agatha.Common.InversionOfControl;

namespace Agatha.ServiceLayer
{
    public interface IRequestProcessingErrorHandler
    {
        void DealWithException(RequestProcessingContext context, Exception exception);

        void DealWithPreviouslyOccurredExceptions(RequestProcessingContext context);
    }

    public class RequestProcessingErrorHandler : IRequestProcessingErrorHandler
    {
        private ServiceLayerConfiguration serviceLayerConfiguration;

        public RequestProcessingErrorHandler(ServiceLayerConfiguration serviceLayerConfiguration)
        {
            this.serviceLayerConfiguration = serviceLayerConfiguration;
        }

        public void DealWithException(RequestProcessingContext context, Exception exception)
        {
            var response = CreateResponse(context);
            response.Exception = new ExceptionInfo(exception);
            SetExceptionType(response, exception);
            context.MarkAsProcessed(response);
        }

        public void DealWithPreviouslyOccurredExceptions(RequestProcessingContext context)
        {
            var response = CreateResponse(context);
            response.Exception = new ExceptionInfo(new Exception(ExceptionType.EarlierRequestAlreadyFailed.ToString()));
            response.ExceptionType = ExceptionType.EarlierRequestAlreadyFailed;
            context.MarkAsProcessed(response);
        }

        private Response CreateResponse(RequestProcessingContext context)
        {
            var responseType = DetermineResponseType(context);
            return (Response)Activator.CreateInstance(responseType);
        }

        private Type DetermineResponseType(RequestProcessingContext context)
        {
            var strategies = new Func<RequestProcessingContext, Type>[]
                {
                        TryBasedOnConventions,
                        TryBasedOnCachedResponse,
                        TryBasedOnRequestHandler
                };
            foreach (var strategy in strategies)
            {
                var responseType = strategy(context);
                if (responseType != null) return responseType;
            }
            return typeof(Response);
        }

        private Type TryBasedOnConventions(RequestProcessingContext context)
        {
            var conventions = IoC.Container.TryResolve<IConventions>();
            if (conventions != null) return conventions.GetResponseTypeFor(context.Request);
            return null;
        }

        private Type TryBasedOnCachedResponse(RequestProcessingContext context)
        {
            var cacheManager = IoC.Container.Resolve<ICacheManager>();
            if (cacheManager.IsCachingEnabledFor(context.Request.GetType()))
            {
                var response = cacheManager.GetCachedResponseFor(context.Request);
                if (response != null) return response.GetType();
            }
            return null;
        }

        private Type TryBasedOnRequestHandler(RequestProcessingContext context)
        {
            IRequestHandler handler = null;
            try
            {
                handler = (IRequestHandler)IoC.Container.Resolve(GetRequestHandlerTypeFor(context.Request));
                return handler.CreateDefaultResponse().GetType();
            }
            catch
            {
                return null;
            }
            finally
            {
                if (handler != null)
                {
                    IoC.Container.Release(handler);
                }
            }
        }

        private static Type GetRequestHandlerTypeFor(Request request)
        {
            return typeof(IRequestHandler<>).MakeGenericType(request.GetType());
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
                SetExceptionFaultCode(exception, response.Exception);
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
    }
}