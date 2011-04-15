using System;
using System.Collections.Generic;
using Agatha.Common;
using Agatha.Common.InversionOfControl;

namespace Agatha.ServiceLayer
{
	public class RequestProcessor : Disposable, IRequestProcessor
	{
		private readonly ServiceLayerConfiguration serviceLayerConfiguration;

		protected override void DisposeManagedResources()
		{
			// empty by default but you should override this in derived classes so you can clean up your resources
		}

		public RequestProcessor(ServiceLayerConfiguration serviceLayerConfiguration)
		{
			this.serviceLayerConfiguration = serviceLayerConfiguration;
		}

		protected virtual void BeforeProcessing(IEnumerable<Request> requests) { }

		protected virtual void AfterProcessing(IEnumerable<Request> requests, IEnumerable<Response> responses) { }

		protected virtual void BeforeHandle(Request request) { }

		protected virtual void AfterHandle(Request request) { }

		public Response[] Process(params Request[] requests)
		{
			if (requests == null) return null;

			var responses = new List<Response>(requests.Length);

			bool exceptionsPreviouslyOccurred = false;

			BeforeProcessing(requests);

			foreach (var request in requests)
			{
				using (var handler = (IRequestHandler)IoC.Container.Resolve(GetRequestHandlerTypeFor(request)))
				{
					try
					{
						if (!exceptionsPreviouslyOccurred)
						{
							var response = GetResponseFromHandler(request, handler);
							exceptionsPreviouslyOccurred = response.ExceptionType != ExceptionType.None;
							responses.Add(response);
						}
						else
						{
							var response = handler.CreateDefaultResponse();
							responses.Add(SetStandardExceptionInfoWhenEarlierRequestsFailed(response));
						}
					}
					finally
					{
						IoC.Container.Release(handler);
					}
				}
			}

			AfterProcessing(requests, responses);

			return responses.ToArray();
		}

		private Response SetStandardExceptionInfoWhenEarlierRequestsFailed(Response response)
		{
			response.ExceptionType = ExceptionType.EarlierRequestAlreadyFailed;
			response.Exception = new ExceptionInfo(new Exception(ExceptionType.EarlierRequestAlreadyFailed.ToString()));
			return response;
		}

		private static Type GetRequestHandlerTypeFor(Request request)
		{
			// get a type reference to IRequestHandler<ThisSpecificRequestType>
			return typeof(IRequestHandler<>).MakeGenericType(request.GetType());
		}

		private Response GetResponseFromHandler(Request request, IRequestHandler handler)
		{
			try
			{
				BeforeHandle(request);
				var response = handler.Handle(request);
				AfterHandle(request);
				return response;
			}
			catch (Exception e)
			{
				return CreateExceptionResponse(handler, e);
			}
		}

		protected virtual Response CreateExceptionResponse(IRequestHandler handler, Exception exception)
		{
			var response = handler.CreateDefaultResponse();
			response.Exception = new ExceptionInfo(exception);
			SetExceptionType(response, exception);
			return response;
		}

		private void SetExceptionType(Response response, Exception exception)
		{
			var exceptionType = exception.GetType();

			if (exceptionType.Equals(serviceLayerConfiguration.BusinessExceptionType))
			{
				response.ExceptionType = ExceptionType.Business;
				return;
			}

			if (exceptionType.Equals(serviceLayerConfiguration.SecurityExceptionType))
			{
				response.ExceptionType = ExceptionType.Security;
				return;
			}

			response.ExceptionType = ExceptionType.Unknown;
		}

		public void ProcessOneWayRequests(params OneWayRequest[] requests)
		{
			if (requests == null) return;

			BeforeProcessing(requests);

			DispatchRequestsToHandlers(requests);

			AfterProcessing(requests, null);
		}

		private void DispatchRequestsToHandlers(OneWayRequest[] requests)
		{
			foreach (var request in requests)
			{
				using (var handler = (IOneWayRequestHandler)IoC.Container.Resolve(GetHandlerTypeFor(request)))
				{
					try
					{
						ExecuteHandler(request, handler);
					}
					finally
					{
						IoC.Container.Release(handler);
					}
				}
			}
		}

		private static Type GetHandlerTypeFor(Request request)
		{
			return typeof(IOneWayRequestHandler<>).MakeGenericType(request.GetType());
		}

		private void ExecuteHandler(OneWayRequest request, IOneWayRequestHandler handler)
		{
			BeforeHandle(request);
			handler.Handle(request);
			AfterHandle(request);
		}
	}
}