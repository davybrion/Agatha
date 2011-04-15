using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agatha.Common;

namespace Sample.Common.RequestsAndResponses
{
    public class GetAgeRequest : Request
    {
        public DateTime DateOfBirth { get; set; }
    }

    public class GetAgeResponse : Response
    {
        public int Age { get; set; }
    }
}
