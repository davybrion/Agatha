using System.ServiceModel;
using System.ServiceModel.Activation;
using Agatha.Common;
using Agatha.Common.InversionOfControl;
using Agatha.Common.WCF;
using System.ServiceModel.Web;
using System.Collections.Specialized;
using System.Linq;
using System;
using Agatha.ServiceLayer.WCF.Rest;

namespace Agatha.ServiceLayer.WCF
{
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
	[AddMessageInspectorBehavior]
	[AddErrorLoggingBehavior]
	public class WcfRequestProcessor : IWcfRequestProcessor, IWcfRestJsonRequestProcessor, IWcfRestXmlRequestProcessor
	{
		[TransactionFlow(TransactionFlowOption.Allowed)]
		public Response[] Process(params Request[] requests)
		{
			using (var processor = IoC.Container.Resolve<IRequestProcessor>())
			{
				Response[] responses;

				try
				{
					responses = processor.Process(requests);
				}
				finally
				{
					// IRequestProcessor is a transient component so we must release it
					IoC.Container.Release(processor);
				}
                
				return responses;
			}
		}

		public void ProcessOneWayRequests(params OneWayRequest[] requests)
		{
			using (var processor = IoC.Container.Resolve<IRequestProcessor>())
			{
				try
				{
					processor.ProcessOneWayRequests(requests);
				}
				finally
				{
					// IRequestProcessor is a transient component so we must release it
					IoC.Container.Release(processor);
				}
			}
		}

        public Response[] Process()
        {
            var collection = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

            var builder = new RestRequestBuilder();

            var requests = builder.GetRequests(collection);

            return Process(requests);
        }

        public void ProcessOneWayRequests()
        {
            var collection = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

            var builder = new RestRequestBuilder();

            var requests = builder.GetOneWayRequests(collection);

            ProcessOneWayRequests(requests);
        }
    }
}