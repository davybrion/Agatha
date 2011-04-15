using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Agatha.Common;
using Agatha.Common.InversionOfControl;
using Sample.Common.RequestsAndResponses;
using Sample.SilverlightClient.PresentationModels;

namespace Sample.SilverlightClient
{
	public partial class MainPage : UserControl
	{
        private readonly IAsyncRequestDispatcherFactory asyncRequestDispatcherFactory;
        private readonly ResponsesModel responsesModel;

		public MainPage()
		{
			InitializeComponent();
            responsesModel = new ResponsesModel();
            DataContext = responsesModel;

            // NOTE: i'm using Agatha's IOC wrapper here directly... 
            // you could just as well resolve the IAsyncRequestDispatcherFactory component through _your_ container
            asyncRequestDispatcherFactory = IoC.Container.Resolve<IAsyncRequestDispatcherFactory>();			
		}

        private void SendSingleRequestButton_Click(object sender, RoutedEventArgs e)
        {
            var requestDispatcher = asyncRequestDispatcherFactory.CreateAsyncRequestDispatcher();
            requestDispatcher.Add(new HelloWorldRequest());
            requestDispatcher.ProcessRequests(SingleRequestSucceeded, ExceptionOccurred);
        }

        private void SendOneWayRequestButton_Click(object sender, RoutedEventArgs e)
        {
            var requestDispatcher = asyncRequestDispatcherFactory.CreateAsyncRequestDispatcher();
            requestDispatcher.Add(new HelloWorldCommand());
            requestDispatcher.ProcessOneWayRequests();
            AddSucceededRequestsToView("One-way request");
        }

        private void SendBatchRequestButton_Click(object sender, RoutedEventArgs e)
        {
            var requestDispatcher = asyncRequestDispatcherFactory.CreateAsyncRequestDispatcher();
            requestDispatcher.Add(new HelloWorldRequest());
            requestDispatcher.Add(new GetServerDateRequest());
            requestDispatcher.ProcessRequests(BatchRequestsSucceeded, ExceptionOccurred);
        }

        private void SingleRequestSucceeded(ReceivedResponses receivedResponses)
        {
            var helloWorldResponse = receivedResponses.Get<HelloWorldResponse>();
            AddSucceededRequestsToView("Single response", helloWorldResponse.Message);
        }

        private void BatchRequestsSucceeded(ReceivedResponses receivedResponses)
        {
            var helloWorldResponse = receivedResponses.Get<HelloWorldResponse>();
            var getServerDateResponse = receivedResponses.Get<GetServerDateResponse>();
            AddSucceededRequestsToView("Batch response", helloWorldResponse.Message, 
                "The server date is: " + getServerDateResponse.Date);
        }

        private void AddSucceededRequestsToView(string requestBatchTitle, params string[] responseTexts)
        {
            responsesModel.Responses.Add(new ResponseBatchModel(requestBatchTitle, responseTexts));
        }

        private void ExceptionOccurred(ExceptionInfo exceptionInfo)
        {
            throw new Exception(exceptionInfo.Message);
        }
	}
}
