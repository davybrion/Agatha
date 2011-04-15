using System.ServiceModel;
using System.ServiceModel.Web;

namespace Agatha.Common.WCF
{
    [ServiceContract]
    public interface IWcfRestJsonRequestProcessor
    {
        [OperationContract(Name = "ProcessJsonRequests")]
        [ServiceKnownType("GetKnownTypes", typeof(KnownTypeProvider))]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        [WebGet(UriTemplate="/", BodyStyle=WebMessageBodyStyle.WrappedResponse,ResponseFormat = WebMessageFormat.Json)]
        Response[] Process();

        [OperationContract(Name = "ProcessOneWayJsonRequests", IsOneWay = true)]
        [ServiceKnownType("GetKnownTypes", typeof(KnownTypeProvider))]
        [WebGet(UriTemplate = "/oneway", BodyStyle = WebMessageBodyStyle.WrappedResponse, ResponseFormat = WebMessageFormat.Json)]
        void ProcessOneWayRequests();
    }
}
