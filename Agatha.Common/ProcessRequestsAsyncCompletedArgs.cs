using System;
using System.ComponentModel;

namespace Agatha.Common
{
	public class ProcessRequestsAsyncCompletedArgs : AsyncCompletedEventArgs
	{
		private readonly object[] results;

		public ProcessRequestsAsyncCompletedArgs(object[] results, Exception exception, bool cancelled, object userState) :
			base(exception, cancelled, userState)
		{
			this.results = results;
		}

		public Response[] Result
		{
			get
			{
				RaiseExceptionIfNecessary();
				return results[0] as Response[];
			}
		}
	}
}