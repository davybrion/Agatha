using System;
using System.Collections.Generic;
using System.Linq;
using Agatha.Common;
using QuickGenerate.Implementation;
using QuickGenerate.Uber;
using QuickNet.Transitions;
using QuickNet.Types;

namespace Tests.NewRequestProcessorTests
{
    public class ProcessRequestsTransition : MetaTransition<IList<QuickNet.Types.Tuple<Request,DescribedAction>>, Response[], RequestProcessorTestsState>
    {
        public ProcessRequestsTransition(RequestProcessorTestsState state)
            : base(state)
        {
            GenerateInput = () =>
                new List<QuickNet.Types.Tuple<Request, DescribedAction>>(state.GeneratorRepository.Randoms<QuickNet.Types.Tuple<Request, DescribedAction>>(0, 10)); 
            Execute =
                input =>
                    {
                        state.ExceptionsThrown = new List<Exception>();
                        state.CacheManager.Clear();
                        state.CacheManager.ExceptionsThrown = state.ExceptionsThrown;
                        input.ForEach(el => state.StubHandler(el.First.GetType(), el.Second.Action));
                        return state.RequestProcessor.Process(input.ToList().ConvertAll(src => src.First).ToArray());
                    };
        }
    }
}