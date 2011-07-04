using System;
using System.Collections.Generic;

namespace Agatha.Common.Configuration
{
    public interface IRequestHandlerRegistry
    {
        IEnumerable<Type> GetTypedRequestHandlers();
        void Register(Type handlerType);
    }

    public class RequestHandlerRegistry : IRequestHandlerRegistry
    {
        private readonly List<Type> requestHandlerTypes = new List<Type>();

        public IEnumerable<Type> GetTypedRequestHandlers()
        {
            return requestHandlerTypes;
        }

        public void Register(Type handlerType)
        {
            requestHandlerTypes.Add(handlerType);
        }
    }
}