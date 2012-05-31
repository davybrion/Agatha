using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agatha.Common;
using Castle.Core.Internal;
using QuickDotNetCheck;
using QuickGenerate;
using Tests.RequestProcessorTests.RequestResponse.Act.Helpers;

namespace Tests.RequestProcessorTests.RequestResponse.Act
{
    public class ProcessRequests : Fixture
    {
        protected IList<Tuple<Request, DescribedAction>> input { get; set; }
        protected Response[] output;

        public override void Arrange()
        {
            // Generating the input through :
            //  ProcessRequestsState.Generator.Many<Tuple<Request, DescribedAction>>(numberOfRequests)
            // didn't work, hence this workaround.
            var numberOfRequests = new[] {0, 10}.FromRange();
            var requests = ProcessRequestsState.Generator.Many<Request>(numberOfRequests).ToList();
            var actions = ProcessRequestsState.Generator.Many<DescribedAction>(numberOfRequests).ToList();
            input = new List<Tuple<Request, DescribedAction>>();
            for (var i = 0; i < requests.Count() ; i++)
            {
                input.Add(new Tuple<Request, DescribedAction>(requests[i], actions[i]));
            }
        }

        protected Request GetRequestFor(Response response)
        {
            return input.Select(i => i.Item1).ElementAt(output.ToList().IndexOf(response));
        }

        protected override void Act()
        {
            ProcessRequestsState.ExceptionsThrown = new List<Exception>();
            ProcessRequestsState.CacheManager.Clear();
            ProcessRequestsState.CacheManager.ExceptionsThrown = ProcessRequestsState.ExceptionsThrown;
            input.ForEach(el => ProcessRequestsState.StubHandler(el.Item1.GetType(), el.Item2.Action));
            output = ProcessRequestsState.RequestProcessor.Process(input.ToList().ConvertAll(src => src.Item1).ToArray());
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(GetType().Name);

            sb.AppendLine("Tuple<Request, DescribedAction>[] : ");
            sb.AppendLine("{");
            foreach (var tuple in input)
            {
                sb.AppendFormat("    ({0}, {1})", tuple.Item1.GetType().Name, tuple.Item2.Description);
                sb.AppendLine();
            }
            sb.AppendLine("}");
            
            return sb.ToString();
        }
    }
}
