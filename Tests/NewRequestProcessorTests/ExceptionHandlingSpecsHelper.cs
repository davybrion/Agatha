using System.Linq;
using xunit.extensions.quicknet;

namespace Tests.NewRequestProcessorTests
{
    public class ExceptionHandlingSpecsHelper : AcidTest<RequestProcessorTestsState>
    {
        public ExceptionHandlingSpecsHelper() : base(0,0) { }

        public ExceptionHandlingSpecsHelper(int numberOfTests, int numberOfTransitions)
            : base(numberOfTests, numberOfTransitions) { }

        protected bool ExceptionWasThrownAfterTheFirstRequest()
        {
            return
                ExceptionWasThrown()
                && FirstExceptionThrownWasntOnFirstRequest();
        }

        protected bool ExceptionWasThrownBeforeLastRequest()
        {
            return
                ExceptionWasThrown()
                && FirstExceptionThrownWasntOnLastRequest();
        }

        protected bool ExceptionWasThrown()
        {
            return GetIndexOfFirstExceptionThrown() >= 0;
        }

        protected bool NoExceptionWasThrown()
        {
            return !ExceptionWasThrown();
        }

        protected bool FirstExceptionThrownWasntOnFirstRequest()
        {
            return GetIndexOfFirstExceptionThrown() != 0;
        }

        protected bool FirstExceptionThrownWasntOnLastRequest()
        {
            return GetIndexOfFirstExceptionThrown() < (State.ExceptionsThrown.Count - 1);
        }

        protected int GetIndexOfFirstExceptionThrown()
        {
            return State.ExceptionsThrown.FindIndex(e => e != null);
        }

        protected int GetIndexOfFirstExceptionThrown<TException>()
        {
            return State.ExceptionsThrown.FindIndex(e => e != null && e.GetType() == typeof(TException));
        }

        protected bool Threw<TException>()
        {
            return State.ExceptionsThrown.Any(e => e != null && e.GetType() == typeof (TException));
        }
    }
}