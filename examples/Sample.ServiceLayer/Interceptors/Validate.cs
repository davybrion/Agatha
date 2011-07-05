using System;
using System.Collections.Generic;
using System.Text;
using Agatha.Common.InversionOfControl;

namespace Sample.ServiceLayer.Interceptors
{
    public static class Validate
    {
        public static void Object<T>(T obj)
        {
            var validator = IoC.Container.Resolve<IValidator<T>>();
            var validationErrors = validator.Validate(obj);
            if (validationErrors.Length > 0) throw new ValidationException(validationErrors);
        }
    }

    public interface IValidator<T>
    {
        ValidationError[] Validate(T obj);
    }

    public class ValidationError
    {
        public string Description;

        public ValidationError(string description)
        {
            Description = description;
        }
    }

    public class ValidationException : Exception
    {
        private readonly string _message;

        public ValidationException(IEnumerable<ValidationError> validationErrors)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Object is not valid:");
            foreach (var validationError in validationErrors)
            {
                builder.AppendLine(validationError.Description);
            }
            _message = builder.ToString();
        }

        public override string Message
        {
            get { return _message; }
        }
    }
}