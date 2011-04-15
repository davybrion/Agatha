using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using Agatha.Common;
using Agatha.Common.WCF;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Agatha.ServiceLayer.WCF.Rest
{
    public class RestRequestBuilder
    {
        private readonly RestRequestValidator requestValidator;

        public RestRequestValidator RequestValidator
        {
            get { return requestValidator; }
        }

        public RestRequestBuilder()
        {
            requestValidator = new RestRequestValidator();
        }

        public Request[] GetRequests(NameValueCollection collection)
        {
            return GetRequests<Request>(collection);
        }

        public OneWayRequest[] GetOneWayRequests(NameValueCollection collection)
        {
            return GetRequests<OneWayRequest>(collection);
        }

        protected T[] GetRequests<T>(NameValueCollection collection)
        {
            ValidateCollection(collection);

            var requests = new List<T>();

            if (IsSingleRequestCollection(collection))
            {
                var type = KnownTypeProvider.GetKnownTypes(null).Where(x => x.Name == collection.Get("request")).FirstOrDefault();

                if (type == null)
                    throw new InvalidOperationException("Cannot resolve the name of the Request Type");

                requests.Add((T)RecurseType(collection, type, null));
            }
            else
            {
                var collections = SplitTypeCollections(collection);
                foreach (var splitCollection in collections)
                {
                    var type = KnownTypeProvider.GetKnownTypes(null).Where(x => x.Name == splitCollection.Get("request")).FirstOrDefault();
                    if (type == null)
                        throw new InvalidOperationException("Cannot resolve the name of the Request Type");
                    var request = (T)RecurseType(splitCollection, type, null);
                    requests.Add(request);
                }
            }

            return requests.ToArray();
        }

        protected NameValueCollection[] SplitTypeCollections(NameValueCollection collection)
        {
            var collections = new List<NameValueCollection>();

            var indexes = GetIndexes(collection);

            foreach (var index in indexes)
            {
                var subCollection = new NameValueCollection();
                foreach (var key in collection.AllKeys)
                {
                    if (key.Contains("[" + index.ToString() + "]"))
                    {
                        subCollection.Add(key.Replace("[" + index.ToString() + "]", ""), collection.Get(key));
                    }
                }
                collections.Add(subCollection);
            }
            return collections.ToArray();
        }

        protected object RecurseType(NameValueCollection collection, Type type, string prefix)
        {
            try
            {
                var returnObject = Activator.CreateInstance(type);

                foreach (var property in type.GetProperties())
                {
                    foreach (var key in collection.AllKeys)
                    {
                        if (String.IsNullOrEmpty(prefix) || key.Length > prefix.Length)
                        {
                            var propertyNameToMatch = String.IsNullOrEmpty(prefix) ? key : key.Substring(property.Name.IndexOf(prefix) + prefix.Length + 1);

                            if (property.Name == propertyNameToMatch)
                            {
                                property.SetValue(returnObject, Convert.ChangeType(collection.Get(key), property.PropertyType), null);
                            }
                            else if (property.GetValue(returnObject, null) == null)
                            {
                                property.SetValue(returnObject, RecurseType(collection, property.PropertyType, String.Concat(prefix, property.PropertyType.Name)), null);
                            }
                        }
                    }
                }

                return returnObject;
            }
            catch (MissingMethodException)
            {
                //Quite a blunt way of dealing with Types without default constructor
                return null;
            }
        }

        protected void ValidateCollection(NameValueCollection collection)
        {
            if (!requestValidator.HasSpecifiedRequestKey(collection))
                throw new InvalidOperationException("At least one request type must be specified");

            if (!requestValidator.HasValidRequestKeys(collection))
                throw new InvalidOperationException("An invalid request key has been specified.");

            if (requestValidator.HasDuplicateRequestTypes(collection))
                throw new InvalidOperationException("The name of the request type must be valid, and an index must be unique");

            if (!requestValidator.ValidRequestIndexing(collection))
                throw new InvalidOperationException("Cannot mix indexed requests with non indexed requests");
        }

        protected bool IsSingleRequestCollection(NameValueCollection collection)
        {
            return collection.AllKeys.Where(x => x == "request").Count() == 1 &&
                !collection.AllKeys.Any(x => x.IndexOf("[") != -1);
        }

        protected int[] GetIndexes(NameValueCollection collection)
        {
            var numbers = new Collection<int>();
            var requestKeys = collection.AllKeys.Where(x => x.IndexOf("request") != -1);
            foreach (var key in requestKeys)
            {
                var startParse = key.IndexOf("[") + 1;
                var endParse = key.IndexOf("]");
                numbers.Add(Convert.ToInt32(key.Substring(startParse, endParse - startParse)));
            }
            return numbers.ToArray();
        }


    }
}
