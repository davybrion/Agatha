using System;

namespace Agatha.Common
{
    public class RequestProcessingContext
    {
        public Request Request { get; private set; }
        public Response Response { get; private set; }

        public RequestProcessingContext(Request request)
        {
            Request = request;
        }

        public void MarkAsProcessed(Response response)
        {
            if (response == null) throw new ArgumentNullException("response");
            Response = response;
            IsProcessed = true;
        }

        public bool IsProcessed { get; private set; }
    }
}