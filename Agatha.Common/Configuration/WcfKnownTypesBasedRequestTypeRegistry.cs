using System;
using System.Collections.Generic;
using Agatha.Common.WCF;

namespace Agatha.Common.Configuration
{
    public class WcfKnownTypesBasedRequestTypeRegistry : IRequestTypeRegistry
    {
        public IEnumerable<Type> GetRegisteredRequestTypes()
        {
            return KnownTypeProvider.GetKnownTypes(null);
        }
    }
}