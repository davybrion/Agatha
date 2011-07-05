using Sample.Common.RequestsAndResponses;

namespace Sample.ServiceLayer.Interceptors
{
    public class HelloWorldRequestValidator : IValidator<HelloWorldRequest>
    {
        public ValidationError[] Validate(HelloWorldRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name)) return new [] { new ValidationError("Name cannot be empty") };
            return new ValidationError[0];
        }
    }
}