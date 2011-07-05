using System;
using Agatha.Common;
using Agatha.Common.InversionOfControl;
using Agatha.ServiceLayer;
using Agatha.ServiceLayer.Conventions;
using Sample.Common.RequestsAndResponses;
using Sample.ServiceLayer.Handlers;
using Sample.ServiceLayer.Interceptors;
using StructureMap;

namespace Sample.ServiceAndClientInSameProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeAgatha();
            LogInfo(@"Press a key to end this program (hopefully, you saw ""Hello World!"" twice)");
            CallTheServiceSynchronously();
            CallTheServiceWithAnInvalidRequest();
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
                LogInfo("Calling the service synchronously...");
                requestDispatcher = IoC.Container.Resolve<IRequestDispatcher>();
                var response = requestDispatcher.Get<HelloWorldResponse>(new HelloWorldRequest { Name = "World" });
                Console.WriteLine(response.Message);

                LogInfo("Sending HelloWorldCommand");
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

        private static void CallTheServiceWithAnInvalidRequest()
        {
            Console.WriteLine();
            LogInfo("Calling the service with an invalid request...");
            var requestDispatcher = IoC.Container.Resolve<IRequestDispatcher>();
            var response = requestDispatcher.Get<HelloWorldResponse>(new HelloWorldRequest());
            LogError("error processing request " + response.Exception);
            Console.WriteLine();
        }

        private static void CallTheServiceAsynchronously()
        {
            // NOTE: i'm using Agatha's IOC wrapper here directly... you could just as well resolve the IAsyncRequestDispatcher component
            // through _your_ container
            LogInfo("Calling the service asynchronously...");
            var requestDispatcher = IoC.Container.Resolve<IAsyncRequestDispatcher>();
            requestDispatcher.Add(new HelloWorldRequest { Name = "World" });
            requestDispatcher.ProcessRequests(ResponsesReceived, e => Console.WriteLine(e.ToString()));
            // NOTE: this request dispatcher will be disposed once the responses have been received...

            LogInfo("Sending HelloWorldCommand");
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
            //new ServiceLayerAndClientConfiguration(typeof(HelloWorldHandler).Assembly, typeof(HelloWorldRequest).Assembly, new Agatha.Ninject.Container()).Initialize();
            //new ServiceLayerAndClientConfiguration(typeof(HelloWorldHandler).Assembly, typeof(HelloWorldRequest).Assembly, new Agatha.Spring.Container()).Initialize();
            ObjectFactory.Configure(c => c.Scan(s =>
                {
                    s.AddAllTypesOf(typeof(IValidator<>));
                    s.AssemblyContainingType<HelloWorldRequestValidator>();
                }));
            new ServiceLayerAndClientConfiguration(typeof(HelloWorldHandler).Assembly, typeof(HelloWorldRequest).Assembly, new Agatha.StructureMap.Container(ObjectFactory.Container))
                .RegisterRequestHandlerInterceptor<ValidatingInterceptor>()
                .Use<RequestHandlerBasedConventions>()
                .Initialize();
        }

        public static void LogInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
