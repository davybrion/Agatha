using System;
using Agatha.Common.InversionOfControl;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Microsoft.Practices.Unity;
using Ninject;
using Spring.Context.Support;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Tests.ConfigurationTests;
using TestTypes;
using Xunit;

namespace Tests.InversionOfControlTests
{
    public abstract class ComponentResolvingByKey<TContainer> where TContainer : IContainer
    {
        protected ComponentResolvingByKey()
        {
            IoC.Container = InitializeContainer();
        }

        protected abstract IContainer InitializeContainer();

        [Fact]
        public void CanResolveRequestA()
        {
            Assert.Equal(typeof(RequestA), IoC.Container.Resolve<RequestA>("KeyForRequestA").GetType());
        }

        [Fact]
        public void CanResolveRequestB()
        {
            Assert.Equal(typeof(RequestB), IoC.Container.Resolve<RequestB>("KeyForRequestB").GetType());
        }
    }

    public sealed class ComponentResolvingByKeyWithUnity : ComponentResolvingByKey<Agatha.Unity.Container>
    {
        protected override IContainer InitializeContainer()
        {
            var container = new UnityContainer();
            container.RegisterType<RequestA>("KeyForRequestA");
            container.RegisterType<RequestB>("KeyForRequestB");

            return new Agatha.Unity.Container(container);
        }
    }
    
}
