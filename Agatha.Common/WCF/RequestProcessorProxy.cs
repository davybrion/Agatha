using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace Agatha.Common.WCF
{
	public class RequestProcessorProxy : ClientBase<IWcfRequestProcessor>, IRequestProcessor
	{
		public RequestProcessorProxy() { }

		public RequestProcessorProxy(string endpointConfigurationName) : base(endpointConfigurationName) { }

		public RequestProcessorProxy(string endpointConfigurationName, string remoteAddress) : base(endpointConfigurationName, remoteAddress) { }

		public RequestProcessorProxy(string endpointConfigurationName, EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress) { }

		public RequestProcessorProxy(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress) { }

		public RequestProcessorProxy(InstanceContext callbackInstance) : base(callbackInstance) { }

		public RequestProcessorProxy(InstanceContext callbackInstance, string endpointConfigurationName) : base(callbackInstance, endpointConfigurationName) { }

		public RequestProcessorProxy(InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : base(callbackInstance, endpointConfigurationName, remoteAddress) { }

		public RequestProcessorProxy(InstanceContext callbackInstance, string endpointConfigurationName, EndpointAddress remoteAddress) : base(callbackInstance, endpointConfigurationName, remoteAddress) { }

		public RequestProcessorProxy(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress) : base(callbackInstance, binding, remoteAddress) { }

		public Response[] Process(params Request[] requests)
		{
			return Channel.Process(requests);
		}

	    public async Task<Response[]> ProcessAsync(Request[] requests)
	    {
	        return await Channel.ProcessAsync(requests);
	    }

	    public void ProcessOneWayRequests(params OneWayRequest[] requests)
        {
			Channel.ProcessOneWayRequests(requests);
        }

		public void Dispose()
		{
			try
			{
				Close();
			}
			catch (Exception)
			{
				Abort();
			}
		}
	}
}