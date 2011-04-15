using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agatha.Common.WCF;
using Xunit;
using Agatha.ServiceLayer.WCF.Rest;
using System.Collections.Specialized;
using Agatha.Common;

namespace Tests.RequestProcessorTests.Rest
{
    public abstract class RestRequestBuilderTests<TRequestType1, TRequestType2>
        where TRequestType1 : IRestTestClass, new()
        where TRequestType2 : IRestTestClass, new()
    {
        public RestRequestBuilderTests()
        {
            KnownTypeProvider.Register(typeof(TRequestType1));
            KnownTypeProvider.Register(typeof(TRequestType2));
        }

        [Fact]
        public void Should_resolve_non_indexed_type()
        {
            var builder = new RestRequestBuilder();
            var collection = new NameValueCollection
            {
                {"request",typeof(TRequestType1).Name}
            };
            var requests = builder.GetRequests(collection);

            Assert.IsAssignableFrom(typeof(IRestTestClass), requests[0]);
        }

        [Fact]
        public void Should_resolve_indexed_type()
        {
            var builder = new RestRequestBuilder();
            var collection = new NameValueCollection
            {
                {"request[0]",typeof(TRequestType1).Name},
                {"request[1]",typeof(TRequestType2).Name},
                {"Name[1]","Joe Bloggs"}
            };
            var requests = builder.GetRequests(collection);

            Assert.IsAssignableFrom(typeof(IRestTestClass), requests[0]);
            Assert.IsAssignableFrom(typeof(IRestTestClass), requests[1]);
        }

        [Fact]
        public void Should_fail_with_duplicate_request_index()
        {
            var builder = new RestRequestBuilder();
            var collection = new NameValueCollection
            {
                {"request[0]",typeof(TRequestType1).Name},
                {"request[0]",typeof(TRequestType2).Name}
            };

            var exception = Record.Exception(() => builder.GetRequests(collection));

            Assert.NotNull(exception);
            Assert.IsAssignableFrom(typeof(InvalidOperationException), exception);
            Assert.True("The name of the request type must be valid, and an index must be unique" == ((InvalidOperationException)exception).Message);
        }

        [Fact]
        public void Should_fail_with_non_indexed_duplicate_request_key()
        {
            var builder = new RestRequestBuilder();
            var collection = new NameValueCollection
            {
                {"request",typeof(TRequestType1).Name},
                {"request",typeof(TRequestType2).Name}
            };

            var exception = Record.Exception(() => builder.GetRequests(collection));

            Assert.NotNull(exception);
            Assert.IsAssignableFrom(typeof(InvalidOperationException), exception);
            Assert.True("The name of the request type must be valid, and an index must be unique" == ((InvalidOperationException)exception).Message);
        }

        [Fact]
        public void Should_fail_when_mixing_indexed_requests_with_non_indexed_requests()
        {
            var builder = new RestRequestBuilder();
            var collection = new NameValueCollection
            {
                {"request[0]",typeof(TRequestType1).Name},
                {"request",typeof(TRequestType2).Name}
            };

            var exception = Record.Exception(() => builder.GetRequests(collection));

            Assert.NotNull(exception);
            Assert.IsAssignableFrom(typeof(InvalidOperationException), exception);
            Assert.True("Cannot mix indexed requests with non indexed requests" == ((InvalidOperationException)exception).Message);
        }

        [Fact]
        public void Should_fail_when_no_request_key_is_passed()
        {
            var builder = new RestRequestBuilder();
            var collection = new NameValueCollection
            {
                {"Name","JoeBloggs"}
            };

            var exception = Record.Exception(() => builder.GetRequests(collection));

            Assert.NotNull(exception);
            Assert.IsAssignableFrom(typeof(InvalidOperationException), exception);
            Assert.True("At least one request type must be specified" == ((InvalidOperationException)exception).Message);
        }

        [Fact]
        public void Should_resolve_complex_type()
        {
            var builder = new RestRequestBuilder();
            var collection = new NameValueCollection
            {
                 {"request",typeof(TRequestType1).Name},
                 {"Age","27"},
                 {"NestedClass_1Name","Joe Bloggs"},
            };

            var requests = builder.GetRequests(collection);

            Assert.IsAssignableFrom(typeof(IRestTestClass), requests[0]);
            Assert.True(27 == ((IRestTestClass)requests[0]).Age);
            Assert.True("Joe Bloggs" == ((IRestTestClass)requests[0]).NestedProperty.Name);
        }

        [Fact]
        public void Should_fail_when_a_request_key_contains_invalid_characters()
        {
            var builder = new RestRequestBuilder();
            var collection = new NameValueCollection
            {
                {"request_[0]","Something"}
            };

            var exception = Record.Exception(() => builder.GetRequests(collection));

            Assert.NotNull(exception);
            Assert.IsAssignableFrom(typeof(InvalidOperationException), exception);
            Assert.True("An invalid request key has been specified." == ((InvalidOperationException)exception).Message);
        }
    }

    public class TestRequests : RestRequestBuilderTests<TestRequest_1, TestRequest_2> { }

    public class TestOneWayRequests : RestRequestBuilderTests<TestOneWayRequest_1, TestOneWayRequest_2> { }

    #region Test Helpers

    public class TestRequest_1 : Request, IRestTestClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public NestedClass_1 NestedProperty { get; set; }
    }

    public class TestRequest_2 : Request, IRestTestClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public NestedClass_1 NestedProperty { get; set; }
    }

    public class TestOneWayRequest_1 : OneWayRequest, IRestTestClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public NestedClass_1 NestedProperty { get; set; }
    }

    public class TestOneWayRequest_2 : OneWayRequest, IRestTestClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public NestedClass_1 NestedProperty { get; set; }
    }

    public class NestedClass_1 { public string Name { get; set; } }

    public interface IRestTestClass
    {
        string Name { get; set; }
        int Age { get; set; }
        NestedClass_1 NestedProperty { get; set; }
    }

    #endregion
}
