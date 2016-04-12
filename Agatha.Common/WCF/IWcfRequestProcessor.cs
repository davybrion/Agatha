using System;
using System.ComponentModel;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Agatha.Common.WCF
{
	[ServiceContract]
	public interface IWcfRequestProcessor
	{
		[OperationContract(Name = "ProcessRequests")]
		[ServiceKnownType("GetKnownTypes", typeof(KnownTypeProvider))]
		[TransactionFlow(TransactionFlowOption.Allowed)]
		Response[] Process(params Request[] requests);

        [OperationContract(Name = "ProcessOneWayRequests", IsOneWay = true)]
        [ServiceKnownType("GetKnownTypes", typeof(KnownTypeProvider))]
		void ProcessOneWayRequests(params OneWayRequest[] requests);

	    Task<Response[]> ProcessAsync(Request[] requests);
	}
}