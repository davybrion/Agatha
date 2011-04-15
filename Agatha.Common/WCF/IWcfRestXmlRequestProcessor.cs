using System.ServiceModel;
using System.ServiceModel.Web;

namespace Agatha.Common.WCF
{
    [ServiceContract]
    public interface IWcfRestXmlRequestProcessor
    {
        [OperationContract(Name = "ProcessXmlRequests")]
        [ServiceKnownType("GetKnownTypes", typeof(KnownTypeProvider))]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        [WebGet(UriTemplate="/", ResponseFormat = WebMessageFormat.Xml)]
        Response[] Process();

        [OperationContract(Name = "ProcessOneWayXmlRequests", IsOneWay = true)]
        [ServiceKnownType("GetKnownTypes", typeof(KnownTypeProvider))]
        [WebGet(UriTemplate="/oneway", ResponseFormat = WebMessageFormat.Xml)]
        void ProcessOneWayRequests();
    }
}
