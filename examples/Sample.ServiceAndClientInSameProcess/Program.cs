using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Agatha.Common;
using Agatha.Common.InversionOfControl;
using Agatha.ServiceLayer;
using Sample.Common.RequestsAndResponses;
using Sample.ServiceLayer.Handlers;

namespace Sample.ServiceAndClientInSameProcess
{
	class Program
	{
		static void Main(string[] args)
		{
			InitializeAgatha();
			Console.WriteLine(@"Press a key to end this program (hopefully, you saw ""Hello World!"" twice)");
			CallTheServiceSynchronously();
			CallTheServiceAsynchronously();
			Console.ReadKey();
		}

		private static void CallTheServiceSynchronously()
		{
			IRequestDispatcher requestDispatcher = null;

			try
			{
				// NOTE: i'm using Agatha's IOC wrapper here directly... you could just as well resolve the IRequestDispatcher component
				// through _your_ container
				Console.WriteLine("Calling the service synchronously...");
				requestDispatcher = IoC.Container.Resolve<IRequestDispatcher>();
				var response = requestDispatcher.Get<HelloWorldResponse>(new HelloWorldRequest());
				Console.WriteLine(response.Message);

				Console.WriteLine("Sending HelloWorldCommand");
				requestDispatcher.Send(new HelloWorldCommand());
			}
			finally
			{
				// not really necessary in this silly example, but it's recommended to release each RequestDispatcher instance once you're
				// done with it
				if (requestDispatcher != null)
				{
					IoC.Container.Release(requestDispatcher);
				}
			}
		}

		private static void CallTheServiceAsynchronously()
		{
			// NOTE: i'm using Agatha's IOC wrapper here directly... you could just as well resolve the IAsyncRequestDispatcher component
			// through _your_ container
			Console.WriteLine("Calling the service asynchronously...");
			var requestDispatcher = IoC.Container.Resolve<IAsyncRequestDispatcher>();
			requestDispatcher.Add(new HelloWorldRequest());
			requestDispatcher.ProcessRequests(ResponsesReceived, e => Console.WriteLine(e.ToString()));
			// NOTE: this request dispatcher will be disposed once the responses have been received...

			Console.WriteLine("Sending HelloWorldCommand");
			requestDispatcher = IoC.Container.Resolve<IAsyncRequestDispatcher>();
			requestDispatcher.Add(new HelloWorldCommand());
			requestDispatcher.ProcessOneWayRequests();
			// NOTE: and this other request dispatcher instance will also be disposed once the call has been _sent_ (since there is no response)
		}

		private static void ResponsesReceived(ReceivedResponses receivedResponses)
		{
			Console.WriteLine(receivedResponses.Get<HelloWorldResponse>().Message);
		}

		private static void InitializeAgatha()
		{
			//new ServiceLayerAndClientConfiguration(typeof(HelloWorldHandler).Assembly, typeof(HelloWorldRequest).Assembly, new Agatha.Castle.Container()).Initialize();
			new ServiceLayerAndClientConfiguration(typeof(HelloWorldHandler).Assembly, typeof(HelloWorldRequest).Assembly, new Agatha.Ninject.Container()).Initialize();
            //new ServiceLayerAndClientConfiguration(typeof(HelloWorldHandler).Assembly, typeof(HelloWorldRequest).Assembly, new Agatha.Spring.Container()).Initialize();
		}
	}
}
