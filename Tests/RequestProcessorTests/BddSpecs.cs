using Rhino.Mocks;

namespace Tests
{
    public abstract class BddSpecs
    {
        protected BddSpecs()
        {
            Given();
            When();
        }

        protected abstract void Given();
        protected abstract void When();

        protected static TMock Mocked<TMock>() where TMock : class
        {
            return MockRepository.GenerateMock<TMock>();
        }
    }
}