using System;
using System.Collections.Generic;

namespace Agatha.Common.Configuration
{
    public interface IRequestTypeRegistry
    {
        IEnumerable<Type> GetRegisteredRequestTypes();
    }
}