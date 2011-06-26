using Agatha.Common;
using QuickGenerate;
using QuickGenerate.Primitives;
using Tests.RequestProcessorTests;

namespace Tests.RequestDispatcherTests
{
    public class RequestGenerator : DomainGenerator
    {
        public RequestGenerator()
        {
            With(mi => mi.ReflectedType == typeof(string), new StringGenerator(1, 1));
            With(mi => mi.ReflectedType == typeof(int), new IntGenerator(0, 5));
            With<Request>(
                opt => opt.StartingValue(
                    () =>
                    new Request[]
                        {
                            new FirstRequest(),
                            new SecondRequest(),
                            new ThirdRequest(),
                            new FourthRequest(),
                            new FifthRequest(),
                            new FirstCachedRequest(),
                            new SecondCachedRequest()
                        }
                        .PickOne()));
        }
    }
}