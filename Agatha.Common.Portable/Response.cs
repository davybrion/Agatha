using System.Runtime.Serialization;

namespace Agatha.Common
{
	[DataContract]
	public class Response
	{
		[DataMember]
		public ExceptionInfo Exception { get; set; }
		[DataMember]
		public ExceptionType ExceptionType { get; set; }
		[DataMember]
		public bool IsCached { get; set; }

		public Response GetShallowCopy()
		{
			return (Response)MemberwiseClone();
		}
	}
}