using System;
using System.Threading;
using Agatha.Common;
using Agatha.ServiceLayer;

namespace Tests.RequestProcessorTests.Interceptors
{
    public class SpyRequest : Request
    {
        public DateTime? BeforeProcessingTimeStamp { get; set; }
        public DateTime? SubSequentInterceptorBeforeProcessingTimeStamp { get; set; }
        public DateTime? RequestHandlingTimeStamp { get; set; }
        public DateTime? AfterProcessingTimeStamp { get; set; }
        public DateTime? SubSequentInterceptorAfterProcessingTimeStamp { get; set; }

        public Response ResponseFromContext { get; set; }
    }

    public class SpyResponse : Response{}

    public class InterceptedSpyRequest : SpyRequest
    {
        public bool ProcessedBySubSequentInterceptorBeforeHandling { get { return SubSequentInterceptorBeforeProcessingTimeStamp != null; } }
        public bool ProcessedBySubSequentInterceptorAfterHandling { get { return SubSequentInterceptorAfterProcessingTimeStamp != null; } }
    }

    public class TestInterceptor : Disposable, IRequestHandlerInterceptor
    {
        public virtual void BeforeHandlingRequest(RequestProcessingContext context)
        {
            var testRequest = (SpyRequest)context.Request;
            testRequest.BeforeProcessingTimeStamp = SystemClock.Now();
            Thread.Sleep(50);
        }

        public void AfterHandlingRequest(RequestProcessingContext context)
        {
            var testRequest = (SpyRequest)context.Request;
            testRequest.AfterProcessingTimeStamp = SystemClock.Now();
            testRequest.ResponseFromContext = context.Response;
            Thread.Sleep(50);
        }

        protected override void DisposeManagedResources()
        {
        }
    }

    public class SubSequentInterceptor : Disposable, IRequestHandlerInterceptor
    {
        public void BeforeHandlingRequest(RequestProcessingContext context)
        {
            ((SpyRequest) context.Request).SubSequentInterceptorBeforeProcessingTimeStamp = SystemClock.Now();
            Thread.Sleep(50);
        }

        public void AfterHandlingRequest(RequestProcessingContext context)
        {
            ((SpyRequest)context.Request).SubSequentInterceptorAfterProcessingTimeStamp = SystemClock.Now();
            Thread.Sleep(50);
        }

        protected override void DisposeManagedResources()
        {
        }
    }

    public class InterceptingInterceptor : TestInterceptor
    {
        public override void BeforeHandlingRequest(RequestProcessingContext context)
        {
            context.MarkAsProcessed(new ResponseFromInterceptor());
        }
    }

    public class ResponseFromInterceptor : Response
    {
    }

    public class SpyRequestHandler : RequestHandler<SpyRequest, SpyResponse>
    {
        public override Response Handle(SpyRequest spyRequest)
        {
            spyRequest.RequestHandlingTimeStamp = SystemClock.Now();
            Thread.Sleep(50);
            return CreateDefaultResponse();
        }
    }

    public class ThrowingRequestHandler : RequestHandler<InterceptedSpyRequest, Response>
    {
        public override Response Handle(InterceptedSpyRequest spyRequest)
        {
            throw new InvalidOperationException("The request handler should not be invoked");
        }
    }

    public static class SystemClock
    {
        public static readonly Func<DateTime> Now = () => DateTime.Now;
    }

   
}