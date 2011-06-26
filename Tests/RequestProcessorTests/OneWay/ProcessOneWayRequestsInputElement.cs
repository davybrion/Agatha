using System;
using System.Collections.Generic;
using Agatha.Common;
using Agatha.ServiceLayer;
using Rhino.Mocks;

namespace Tests.RequestProcessorTests.OneWay
{
    public class ProcessOneWayRequestsInputElement
    {
        public IOneWayRequestHandler RequestHandler { get; private set; }

        public OneWayRequest Request { get; private set; }

        public ProcessOneWayRequestsInputElement(IOneWayRequestHandler requestHandler, OneWayRequest request)
        {
            RequestHandler = requestHandler;
            Request = request;
        }

        public void StubIt(Action<IList<Exception>> action, IList<Exception> exceptionsThrown)
        {
            RequestHandler
                .Stub(r => r.Handle(Request))
                .WhenCalled(arg => action(exceptionsThrown))
                .Repeat.Once();
        }
    }
}