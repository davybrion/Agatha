using System;
using System.Reflection;
using Agatha.Common;
using Agatha.Common.InversionOfControl;
using Agatha.ServiceLayer;
using Xunit;

namespace Tests
{
    public class UncheckedConventions
    {
        // Local test types
        public class FirstRequest : Request { }
        public class SecondRequest : Request { }
        public class ThirdRequest : Request { }

        public class FirstResponse : Response { }
        public class SecondResponse : Response { }
        //---------------------------------------------------------------------------

        public IRequestDispatcher RequestDispatcher;

        public UncheckedConventions()
        {
            IoC.Container = new Agatha.Castle.Container();

            new ServiceLayerAndClientConfiguration(
                Assembly.GetExecutingAssembly(),
                Assembly.GetExecutingAssembly(),
                IoC.Container)
                .Initialize();

            RequestDispatcher = IoC.Container.Resolve<IRequestDispatcher>();
        }

        //---------------------------------------------------------------------------
        [Fact]
        public void ConflictOne()
        {
            RequestDispatcher.Add(new FirstRequest());
            Assert.NotNull(RequestDispatcher.Get<FirstResponse>());
            RequestDispatcher.Clear();
            // But, no way to get at to the second handler : 
            RequestDispatcher.Add(new FirstRequest());
            Assert.Throws<InvalidOperationException>(() => RequestDispatcher.Get<SecondResponse>());
        }

        public class FirstHandler : RequestHandler<FirstRequest, FirstResponse>
        {
            public override Response Handle(FirstRequest request) { return CreateDefaultResponse(); }
        }

        public class SecondHandler : RequestHandler<FirstRequest, SecondResponse>
        {
            public override Response Handle(FirstRequest request) { return CreateDefaultResponse(); }
        }
        //---------------------------------------------------------------------------


        //---------------------------------------------------------------------------
        [Fact]
        public void ConflictTwo()
        {
            RequestDispatcher.Add("1", new SecondRequest());
            RequestDispatcher.Add("2", new ThirdRequest());
            Assert.NotNull(RequestDispatcher.Get<FirstResponse>("1"));
            Assert.NotNull(RequestDispatcher.Get<FirstResponse>("2"));
            RequestDispatcher.Clear();

            // But :
            RequestDispatcher.Add(new SecondRequest());
            RequestDispatcher.Add(new ThirdRequest());
            Assert.Throws<InvalidOperationException>(() => RequestDispatcher.Get<FirstResponse>());
        }

        public class ThirdHandler : RequestHandler<SecondRequest, FirstResponse>
        {
            public override Response Handle(SecondRequest request) { return CreateDefaultResponse(); }
        }

        public class FourthHandler : RequestHandler<ThirdRequest, FirstResponse>
        {
            public override Response Handle(ThirdRequest request) { return CreateDefaultResponse(); }
        }
        //---------------------------------------------------------------------------
    }
}
