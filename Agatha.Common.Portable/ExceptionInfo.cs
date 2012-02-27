using System;
using System.Globalization;
using System.Text;

namespace Agatha.Common
{
	public class ExceptionInfo
	{
		public ExceptionInfo InnerException { get; set; }
		public string Message { get; set; }
		public string StackTrace { get; set; }
		public string Type { get; set; }
		public string FaultCode { get; set; }

		public ExceptionInfo() { }

		public ExceptionInfo(Exception exception)
		{
			Message = exception.Message;
			StackTrace = exception.StackTrace;
			Type = exception.GetType().ToString();

			if (exception.InnerException != null)
			{
				InnerException = new ExceptionInfo(exception.InnerException);
			}
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}\n{1}",
				"An ExceptionInfo whose value is:",
				ToStringHelper(false));
		}

		private string ToStringHelper(bool isInner)
		{
			var builder = new StringBuilder();
			builder.AppendFormat("{0}: {1}", Type, Message);

			if (InnerException != null)
			{
				builder.AppendFormat(" ----> {0}", InnerException.ToStringHelper(true));
			}
			else
			{
				builder.Append("\n");
			}

			builder.Append(StackTrace);

			if (isInner)
			{
				builder.AppendFormat("\n   {0}\n", "--- End of inner ExceptionInfo stack trace ---");
			}

			return builder.ToString();
		}
	}
}