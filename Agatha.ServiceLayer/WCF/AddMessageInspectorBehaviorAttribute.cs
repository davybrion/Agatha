using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Agatha.ServiceLayer.WCF
{
	public class AddMessageInspectorBehaviorAttribute : Attribute, IServiceBehavior
	{
		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }

		public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
			Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) { }

		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers)
			{
				foreach (var endpoint in dispatcher.Endpoints)
				{
					endpoint.DispatchRuntime.MessageInspectors.Add(new MessageInspector());
				}
			}
		}
	}
}