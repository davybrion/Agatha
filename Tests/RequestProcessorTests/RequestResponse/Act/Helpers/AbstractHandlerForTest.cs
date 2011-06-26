using System;
using System.Collections.Generic;

namespace Tests.RequestProcessorTests.RequestResponse.Act.Helpers
{
    public abstract class AbstractHandlerForTest
    {
        public int HandledRequest { get; set; }
        public int DefaultResponseReturned { get; set; }

        public Exception ExceptionThrown { get; set; }

        public Action<IList<Exception>> HandleAction { get; set; }
        public List<Exception> ExceptionsThrown { get; set; }
    }
}