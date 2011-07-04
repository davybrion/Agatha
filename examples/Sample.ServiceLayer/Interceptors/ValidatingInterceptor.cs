using Agatha.Common;
using Agatha.Common.Interceptors;

namespace Sample.ServiceLayer.Interceptors
{
    public class ValidatingInterceptor : ConventionBasedInterceptor
    {
        public IConventions Conventions { get; set; }

        public ValidatingInterceptor(IConventions conventions)
        {
            Conventions = conventions;
        }

        public override void BeforeHandlingRequest(RequestProcessingContext context)
        {
            try
            {
                Validate.Object(context.Request as dynamic);
            }
            catch (ValidationException exc)
            {
                var response = CreateDefaultResponseFor(context.Request);
                response.Exception = new ExceptionInfo(exc);
                response.ExceptionType = ExceptionType.Unknown;
                context.MarkAsProcessed(response);
            }
        }

        public override void AfterHandlingRequest(RequestProcessingContext context)
        {
        }

        protected override void DisposeManagedResources()
        {
        }
    }
}