using System.Collections.Generic;
using System.Linq;
using Agatha.Common;
using QuickGenerate.Meta;
using QuickGenerate.Uber;
using QuickNet.Transitions;
using QuickNet.Types;

namespace Tests.RequestDispatcherTests.AddRequestArray
{
    public class AddRequestArrayTransition : MetaTransition<IList<Request>, Null, RequestDispatcherTestsState>
    {
        public AddRequestArrayTransition(RequestDispatcherTestsState state)
            : base(state)
        {
            GenerateInput = () => new List<Request>(state.GeneratorRepository.Randoms<Request>(0, 10));
            Execute =
                input =>
                    {
                        State.RequestDispatcher.Add(input.ToArray());
                        return null;
                    };
        }
    }
}