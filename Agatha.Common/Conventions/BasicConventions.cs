using System;
using System.Collections.Generic;
using System.Linq;
using Agatha.Common.Configuration;

namespace Agatha.Common.Conventions
{
    public class BasicConventions : IConventions
    {
        private readonly IDictionary<Type, Type> requestResponseMappings = new Dictionary<Type, Type>();
        private readonly IDictionary<Type, Type> responseRequestMappings = new Dictionary<Type, Type>();

        public BasicConventions(IRequestTypeRegistry configuration)
        {
            BuildRequestReponseMappings(configuration.GetRegisteredRequestTypes());
        }

        private void BuildRequestReponseMappings(IEnumerable<Type> requestTypes)
        {
            foreach (var requestType in requestTypes.Where(t => t.Name.EndsWith("Request")))
            {
                var determineResponseType = DetermineResponseType(requestType);
                requestResponseMappings.Add(requestType, determineResponseType);
                responseRequestMappings.Add(determineResponseType, requestType);
            }
        }

        private static Type DetermineResponseType(Type requestType)
        {
            var requestTypeName = requestType.FullName;
            var responseTypeName = ReplaceRequestSuffix(requestTypeName);
            var reponseType = requestType.Assembly.GetType(responseTypeName);
            if (reponseType == null) throw new InvalidOperationException("Could not determine response type by convention for request of type " + requestTypeName);
            return reponseType;
        }

        private static string ReplaceRequestSuffix(string requestTypeName)
        {
            var index = requestTypeName.LastIndexOf("Request");
            return string.Concat(requestTypeName.Substring(0, index), "Response");
        }

        public Type GetResponseTypeFor(Request request)
        {
            if (request == null) throw new ArgumentNullException("request");

            return requestResponseMappings[request.GetType()];
        }

        public Type GetRequestTypeFor(Response response)
        {
            if (response == null) throw new ArgumentNullException("response");

            return responseRequestMappings[response.GetType()];
        }
    }
}