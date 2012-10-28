using System;
using System.Collections.Generic;
using Agatha.Common;
using Agatha.Common.Configuration;

namespace Agatha.ServiceLayer.Conventions
{
    public class RequestHandlerBasedConventions : IConventions
    {
        private readonly IDictionary<Type, Type> requestResponseMappings = new Dictionary<Type, Type>();
        private readonly IDictionary<Type, Type> responseRequestMappings = new Dictionary<Type, Type>();

        public RequestHandlerBasedConventions(IRequestHandlerRegistry configuration)
        {
            BuildRequestReponseMappings(configuration.GetTypedRequestHandlers());
        }

        private void BuildRequestReponseMappings(IEnumerable<Type> typedRequestHandlers)
        {
            foreach (var handlerType in typedRequestHandlers)
            {
                BuildRequestResponseMapping(handlerType);
            }
        }

        private void BuildRequestResponseMapping(Type handlerType)
        {
            var handlerBase = FindHandlerBase(handlerType);
            if (handlerBase == null) return;

            var genericArguments = handlerBase.GetGenericArguments();
            if (genericArguments.Length < 2) return;

            var requestType = genericArguments[0];
            var responseType = genericArguments[1];

            if (requestResponseMappings.ContainsKey(requestType))
                return;

            requestResponseMappings.Add(requestType, responseType);

            if (!responseRequestMappings.ContainsKey(responseType))
                responseRequestMappings.Add(responseType, requestType);
        }

        private Type FindHandlerBase(Type handlerType)
        {
            while (!handlerType.IsGenericType && handlerType != typeof(object))
            {
                if (handlerType.BaseType.IsGenericType)
                {
                    return handlerType.BaseType;
                }
                handlerType = handlerType.BaseType;
            }
            return null;
        }

        public Type GetResponseTypeFor(Request request)
        {
            if (request == null) throw new ArgumentNullException("request");
            var requestType = request.GetType();
            if (!requestResponseMappings.ContainsKey(requestType))
                throw new Exception("No response type found by convention for request type " + requestType);

            return requestResponseMappings[requestType];
        }

        public Type GetRequestTypeFor(Response response)
        {
            if (response == null) throw new ArgumentNullException("response");
            var responseType = response.GetType();
            if (!responseRequestMappings.ContainsKey(responseType))
                throw new Exception("No request type found by convention for response type " + responseType);

            return responseRequestMappings[responseType];
        }
    }
}