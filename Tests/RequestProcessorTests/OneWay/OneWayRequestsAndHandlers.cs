using System;
using System.Collections.Generic;

namespace Tests.RequestProcessorTests.OneWay
{
    public class OneWayRequestsAndHandlers : Dictionary<Type, ProcessOneWayRequestsInputElement>
    {
        public OneWayRequestsAndHandlers()
        {
            Add(typeof(FirstOneWayRequest), new ProcessOneWayRequestsInputElement(OneWayRequestSuite.firstOneWayRequestHandler, new FirstOneWayRequest()));
            Add(typeof(SecondOneWayRequest), new ProcessOneWayRequestsInputElement(OneWayRequestSuite.secondOneWayRequestHandler, new SecondOneWayRequest()));
            Add(typeof(ThirdOneWayRequest), new ProcessOneWayRequestsInputElement(OneWayRequestSuite.thirdOneWayRequestHandler, new ThirdOneWayRequest()));
        }
    }
}