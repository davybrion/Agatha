using System;

namespace Agatha.Common
{
    public interface IConventions
    {
        Type GetResponseTypeFor(Request request);
        Type GetRequestTypeFor(Response response);
    }
}