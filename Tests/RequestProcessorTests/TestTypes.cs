using System;
using Agatha.Common;

namespace Tests.RequestProcessorTests
{
	public class FirstRequest : Request {}
	public class FirstResponse : Response {}
	public class SecondRequest : Request {}
	public class SecondResponse : Response {}
	public class ThirdRequest : Request {}
	public class ThirdResponse : Response {}
	public class FourthRequest : Request {}
	public class FourthResponse : Response {}
	public class FifthRequest : Request {}
	public class FifthResponse : Response {}

	[EnableServiceResponseCaching(Seconds = 1)]
	public class FirstCachedRequest : Request
	{
		public string String { get; set; }

		public bool Equals(FirstCachedRequest other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.String, String);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(FirstCachedRequest)) return false;
			return Equals((FirstCachedRequest)obj);
		}

		public override int GetHashCode()
		{
			return (String != null ? String.GetHashCode() : 0);
		}
	}

	public class FirstCachedResponse : Response { }

	[EnableServiceResponseCaching(Seconds = 2)]
	public class SecondCachedRequest : Request
	{
		public int Integer { get; set; }

		public bool Equals(SecondCachedRequest other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return other.Integer == Integer;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(SecondCachedRequest)) return false;
			return Equals((SecondCachedRequest)obj);
		}

		public override int GetHashCode()
		{
			return Integer;
		}
	}

	public class SecondCachedResponse : Response {}

	public class FirstOneWayRequest : OneWayRequest { }
	public class SecondOneWayRequest : OneWayRequest { }
	public class ThirdOneWayRequest : OneWayRequest { }

	public class BusinessException : Exception { }
	public class SecurityException : Exception { }
	public class UnknownException : Exception { }
	public class AnotherUnknownException : Exception { }
}