using Agatha.Common;

namespace Sample.Common.RequestsAndResponses
{
	//[EnableServiceResponseCaching(Minutes = 1)]
	[EnableClientResponseCaching(Seconds = 30)]
	public class ReverseStringRequest : Request
	{
		public string StringToReverse { get; set; }

		public bool Equals(ReverseStringRequest other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.StringToReverse, StringToReverse);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(ReverseStringRequest)) return false;
			return Equals((ReverseStringRequest)obj);
		}

		public override int GetHashCode()
		{
			return (StringToReverse != null ? StringToReverse.GetHashCode() : 0);
		}
	}

	public class ReverseStringResponse : Response
	{
		public string ReversedString { get; set; }	
	}
}