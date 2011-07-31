using System;

namespace Agatha.Common.Caching
{
    public class CachingInterceptor : Disposable, IRequestHandlerInterceptor
    {
        private readonly ICacheManager cacheManager;

        public CachingInterceptor(ICacheManager cacheManager)
        {
            this.cacheManager = cacheManager;
        }

        public void BeforeHandlingRequest(RequestProcessingContext context)
        {
            if (CachingIsEnabledForThisRequest(context))
            {
                var response = cacheManager.GetCachedResponseFor(context.Request);
                if (response != null)
                {
                    context.MarkAsProcessed(response);
                }
            }
        }

        public void AfterHandlingRequest(RequestProcessingContext context)
        {
            if (ResponseCanBeCached(context))
            {
                context.Response.IsCached = true;
                cacheManager.StoreInCache(context.Request, context.Response);
            }
        }

        private bool CachingIsEnabledForThisRequest(RequestProcessingContext context)
        {
            return cacheManager.IsCachingEnabledFor(context.Request.GetType());
        }

        private bool ResponseCanBeCached(RequestProcessingContext context)
        {
            return CachingIsEnabledForThisRequest(context) && context.Response.ExceptionType == ExceptionType.None;
        }

        protected override void DisposeManagedResources()
        {
        }
    }
}