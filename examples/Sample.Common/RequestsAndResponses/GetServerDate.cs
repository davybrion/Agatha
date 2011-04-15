using Agatha.Common;
using System;

namespace Sample.Common.RequestsAndResponses
{
    [EnableClientResponseCaching(Seconds = 5)]
	public class GetServerDateRequest : Request
    {
        public bool Equals(GetServerDateRequest other)
        {
            return !ReferenceEquals(null, other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(GetServerDateRequest)) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }

	public class GetServerDateResponse : Response
	{
		public DateTime Date { get; set; }
	}
}
