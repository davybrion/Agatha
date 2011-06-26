using System;
using System.Collections.Generic;
using System.Linq;
using Agatha.Common;

namespace Tests.RequestProcessorTests.OneWay
{
    public class ProcessOneWayRequestsInput
    {
        public IList<Action<IList<Exception>>> Actions { get; set; }
        public OneWayRequestsAndHandlers OneWayRequestsAndHandlers { get; set; }
        public OneWayRequest[] Requests
        {
            get
            {
                return OneWayRequestsAndHandlers.ToList().ConvertAll(kv => kv.Value.Request).ToArray();
            }
        }
    }
}