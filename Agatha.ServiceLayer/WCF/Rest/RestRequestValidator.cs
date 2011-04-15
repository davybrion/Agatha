using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Agatha.ServiceLayer.WCF.Rest
{
    public class RestRequestValidator
    {
        protected static Regex RequestKeyMatcher;

        static RestRequestValidator()
        {
            RequestKeyMatcher = new Regex(@"request(\[\d+\])?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public bool HasSpecifiedRequestKey(NameValueCollection collection)
        {
            return collection.AllKeys.Any(x => x.IndexOf("request") > -1);
        }

        public bool HasValidRequestKeys(NameValueCollection collection)
        {
            return !collection.AllKeys.Any(x => x.IndexOf("request") > -1 && !RequestKeyMatcher.IsMatch(x));
        }

        public bool ValidRequestIndexing(NameValueCollection collection)
        {
            var requestKeys = collection.AllKeys.Where(x => x.Contains("request"));
            //Crude way to see if the user is mixing indexed requests with non-indexed requests
            //This is not accounting for the user badly formatting the request
            return !(requestKeys.Any(x => x.IndexOf("request[") != -1) &&
                requestKeys.Any(x => x == "request"));
        }

        public bool HasDuplicateRequestTypes(NameValueCollection collection)
        {
            return collection.AllKeys.Any(x => x.IndexOf("request") > -1 && collection.Get(x).Contains(","));
        }
    }
}
