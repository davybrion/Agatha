using System;
using System.Collections.Generic;

namespace Tests.RequestProcessorTests.RequestResponse.Act.Helpers
{
    public class DescribedAction
    {
        public Action<IList<Exception>> Action
        {
            get
            {
                return
                    l =>
                    {
                        l.Add(Exception);
                        if (Exception != null)
                            throw Exception;
                    };
            }
        }
        public Exception Exception { get; set; }
        public string Description { get; set; }
        public override string ToString()
        {
            return Description;
        }
    }
}