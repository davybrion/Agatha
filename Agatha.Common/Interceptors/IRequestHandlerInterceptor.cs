using System;

namespace Agatha.Common
{
    public interface IRequestHandlerInterceptor : IDisposable
    {
        void BeforeHandlingRequest(RequestProcessingContext context);
        void AfterHandlingRequest(RequestProcessingContext context);
    }
}