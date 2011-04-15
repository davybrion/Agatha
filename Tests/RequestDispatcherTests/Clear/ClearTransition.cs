using QuickNet.Transitions;
using QuickNet.Types;

namespace Tests.RequestDispatcherTests.Clear
{
    public class ClearTransition : MetaTransition<Null, Null, RequestDispatcherTestsState>
    {
        public ClearTransition(RequestDispatcherTestsState state)
            : base(state)
        {
            Execute = input => { State.RequestDispatcher.Clear(); return null; };
        }
    }
}