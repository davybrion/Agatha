using System;
using Agatha.Common;
using Agatha.Common.Caching;
using Agatha.Common.InversionOfControl;

namespace Agatha.ServiceLayer
{
    public interface IRequestProcessingErrorHandler
    {
        void DealWithPreviouslyOccurredExceptions(RequestProcessingContext context);
    }

    public class RequestProcessingErrorHandler : IRequestProcessingErrorHandler
    {
        public void DealWithPreviouslyOccurredExceptions(RequestProcessingContext context)
        {
            var response = CreateResponse(context);
            var exceptionInfo = new ExceptionInfo(new Exception(ExceptionType.EarlierRequestAlreadyFailed.ToString()));
            response.Exception = exceptionInfo;
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
            if(cacheManager.IsCachingEnabledFor(context.Request.GetType()))
            {
                var response = cacheManager.GetCachedResponseFor(context.Request);
                if (response != null) return response.GetType();
            }
            return null;
        }

        private Type TryBasedOnRequestHandler(RequestProcessingContext context)
        {
            try
            {
                var handler = (IRequestHandler)IoC.Container.Resolve(GetRequestHandlerTypeFor(context.Request));
                return handler.CreateDefaultResponse().GetType();
            }
            catch
            {
                return null;
            }
        }

        private static Type GetRequestHandlerTypeFor(Request request)
        {
            return typeof(IRequestHandler<>).MakeGenericType(request.GetType());
        }
    }
}