using Agatha.Common;
using QuickGenerate;
using QuickGenerate.Uber;
using QuickNet.Transitions;
using QuickNet.Types;

namespace Tests.RequestDispatcherTests.AddRequestWithKey
{
    public class AddRequestWithKeyTransition : MetaTransition<Tuple<string, Request>, Null, RequestDispatcherTestsState>
    {
        public AddRequestWithKeyTransition(RequestDispatcherTestsState state)
            : base(state)
        {
            GenerateInput = () => Tuple.New(
                new ChoiceGenerator<string>(new[] { "KeyOne", "KeyTwo" }).GetRandomValue(),
                state.GeneratorRepository.Random<Request>());
            Execute =
                input =>
                    {
                        State.RequestDispatcher.Add(input.First, input.Second);
                        return null;
                    };
        }
    }
}