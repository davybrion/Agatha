using Agatha.Common;
using QuickGenerate.Uber;
using QuickNet.Transitions;
using QuickNet.Types;

namespace Tests.RequestDispatcherTests.AddRequest
{
    public class AddRequestTransition : MetaTransition<Request, Null, RequestDispatcherTestsState>
    {
        public AddRequestTransition(RequestDispatcherTestsState state)
            : base(state)
        {
            GenerateInput = () => state.GeneratorRepository.Random<Request>();
            Execute =
                input =>
                    {
                        State.RequestDispatcher.Add(input);
                        return null;
                    };
        }
    }
}