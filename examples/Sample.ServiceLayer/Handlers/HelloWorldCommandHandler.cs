using System.Diagnostics;
using Agatha.ServiceLayer;
using Sample.Common.RequestsAndResponses;

namespace Sample.ServiceLayer.Handlers
{
    public class HelloWorldCommandHandler : OneWayRequestHandler<HelloWorldCommand>
    {
        public override void Handle(HelloWorldCommand request)
        {
            Debug.WriteLine("HelloWorldCommand recieved!");
        }
    }
}
