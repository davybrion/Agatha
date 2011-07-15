using System;
using System.Threading;
using Agatha.Common;
using Agatha.Common.InversionOfControl;
using Sample.Common.RequestsAndResponses;

namespace Sample.DotNetClient
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

				Console.WriteLine("requesting a hello world... (synchronously)");
				requestDispatcher.Add(new HelloWorldRequest());
				Console.WriteLine("asking the server to reverse this string... (synchronously)");
				requestDispatcher.Add(new ReverseStringRequest { StringToReverse = "asking the server to reverse this string" });

				Console.WriteLine(requestDispatcher.Get<HelloWorldResponse>().Message);
				Console.WriteLine(requestDispatcher.Get<ReverseStringResponse>().ReversedString);

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

			Console.WriteLine("requesting a hello world... (asynchronously)");
			requestDispatcher.Add(new HelloWorldRequest());
			Console.WriteLine("asking the server to reverse this string... (asynchronously)");
			requestDispatcher.Add(new ReverseStringRequest { StringToReverse = "asking the server to reverse this string" });
			requestDispatcher.ProcessRequests(ResponsesReceived, e => Console.WriteLine(e.ToString()));
			// NOTE: this request dispatcher will be disposed once the responses have been received...

            Console.WriteLine("Sending HelloWorldCommand");
            requestDispatcher = IoC.Container.Resolve<IAsyncRequestDispatcher>();
            requestDispatcher.Add(new HelloWorldCommand());
            requestDispatcher.ProcessOneWayRequests();
            // NOTE: and this other request dispatcher instance will also be disposed once the call has been _sent_ (since there is no response)

            new Timer(t => SendReverseStringRequestAsync(), null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        }

        private static void SendReverseStringRequestAsync()
	    {
	        var requestDispatcher = IoC.Container.Resolve<IAsyncRequestDispatcher>();
            requestDispatcher.Add(new HelloWorldRequest());
            requestDispatcher.Add(new ReverseStringRequest { StringToReverse = "asking the server to reverse this string" });
	        requestDispatcher.ProcessRequests(ResponsesReceived, e => Console.WriteLine(e.ToString()));
	    }

	    private static void ResponsesReceived(ReceivedResponses receivedResponses)
		{
            if (receivedResponses.HasResponse<HelloWorldResponse>())
		    {
                Console.WriteLine(receivedResponses.Get<HelloWorldResponse>().Message);
            }

			Console.WriteLine(receivedResponses.Get<ReverseStringResponse>().ReversedString);
		    Console.WriteLine(receivedResponses.Get<ReverseStringResponse>().IsCached);
		}

		private static void InitializeAgatha()
		{
            //new ClientConfiguration(typeof(HelloWorldRequest).Assembly, typeof(Agatha.Castle.Container)).Initialize();
			//new ClientConfiguration(typeof(HelloWorldRequest).Assembly, typeof(Agatha.Ninject.Container)).Initialize();
			//new ClientConfiguration(typeof(HelloWorldRequest).Assembly, typeof(Agatha.Unity.Container)).Initialize();
            //new ClientConfiguration(typeof(HelloWorldRequest).Assembly, typeof(Agatha.Spring.Container)).Initialize();
            new ClientConfiguration(typeof(HelloWorldRequest).Assembly, typeof(Agatha.StructureMap.Container)).Initialize();
        }
	}
}
