using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Agatha.Common;
using NLog;

namespace Agatha.ServiceLayer
{
	public class PerformanceLoggingRequestProcessor : RequestProcessor
	{
		public PerformanceLoggingRequestProcessor(ServiceLayerConfiguration serviceLayerConfiguration, IRequestProcessingErrorHandler errorHandler) : base(serviceLayerConfiguration, errorHandler) {}

		private readonly Logger performanceLogger = LogManager.GetLogger("AgathaPerformance");

		private Stopwatch requestStopwatch;
		private Stopwatch batchStopwatch;

		protected override void BeforeProcessing(IEnumerable<Request> requests)
		{
			base.BeforeProcessing(requests);
			batchStopwatch = Stopwatch.StartNew();
		}

		protected override void AfterProcessing(IEnumerable<Request> requests, IEnumerable<Response> responses)
		{
			base.AfterProcessing(requests, responses);
			batchStopwatch.Stop();

			// TODO: make the 200ms limit configurable
			if (batchStopwatch.ElapsedMilliseconds > 200)
			{
				var builder = new StringBuilder();

				foreach (var request in requests)
				{
					builder.Append(request.GetType().Name + ", ");
				}

				builder.Remove(builder.Length - 2, 2);

				performanceLogger.Warn(string.Format("Performance warning: {0}ms for the following batch: {1}", batchStopwatch.ElapsedMilliseconds, builder));
			}
		}

		protected override void BeforeHandle(Request request)
		{
			base.BeforeHandle(request);
			requestStopwatch = Stopwatch.StartNew();
		}

		protected override void AfterHandle(Request request)
		{
			base.AfterHandle(request);
			requestStopwatch.Stop();

			// TODO: make the 100ms limit configurable
			if (requestStopwatch.ElapsedMilliseconds > 100)
			{
				performanceLogger.Warn(string.Format("Performance warning: {0}ms for {1}", requestStopwatch.ElapsedMilliseconds, request.GetType().Name));
			}
		}
	}
}