using System;
using System.ComponentModel;
using System.ServiceModel;

namespace Agatha.Common.WCF
{
	[ServiceContract(Name = "IWcfRequestProcessor", ConfigurationName = "Agatha.Common.WCF.IWcfRequestProcessor")]
	public interface IAsyncWcfRequestProcessor : IDisposable
	{
		[OperationContract(AsyncPattern = true, Name = "ProcessRequests")]
		[ServiceKnownType("GetKnownTypes", typeof(KnownTypeProvider))]
		IAsyncResult BeginProcessRequests(Request[] requests, AsyncCallback callback, object asyncState);
		Response[] EndProcessRequests(IAsyncResult result);
		void ProcessRequestsAsync(Request[] requests, Action<ProcessRequestsAsyncCompletedArgs> processCompleted);

		[OperationContract(AsyncPattern = true, Name = "ProcessOneWayRequests", IsOneWay = true)]
		[ServiceKnownType("GetKnownTypes", typeof(KnownTypeProvider))]
		IAsyncResult BeginProcessOneWayRequests(OneWayRequest[] requests, AsyncCallback callback, object asyncState);
		void EndProcessOneWayRequests(IAsyncResult result);
		void ProcessOneWayRequestsAsync(OneWayRequest[] requests, Action<AsyncCompletedEventArgs> processCompleted);
	}
}