using System.Collections.Generic;
using System.Linq;
using Agatha.Common;

namespace Tests.RequestDispatcherTests
{
    public class RequestDispatcherTestsState
    {
        public RequestDispatcher RequestDispatcher { get; set; }
        private readonly List<string> keysUsed = new List<string>();

        public RequestDispatcherTestsState()
        {
            RequestDispatcher = new RequestDispatcher(null, null);
        }

        public void UseKey(string key)
        {
            keysUsed.Add(key);
        }

        public bool KeyAllreadyUsed(string key)
        {
            return
                keysUsed
                    .Take(keysUsed.Count - 1)
                    .Any(el => el == key);
        }

        public void ClearKeysUsed()
        {
            keysUsed.Clear();
        }
    }
}
