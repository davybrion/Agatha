using System;
using Agatha.Common.InversionOfControl;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Microsoft.Practices.Unity;
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

    public sealed class ComponentResolvingByKeyWithCastle : ComponentResolvingByKey<Agatha.Castle.Container>
    {
        protected override IContainer InitializeContainer()
        {
            var container = new WindsorContainer();
            container.Register(Component.For<RequestA>().Named("KeyForRequestA"));
            container.Register(Component.For<RequestB>().Named("KeyForRequestB"));

            return new Agatha.Castle.Container(container);
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

    public sealed class ComponentResolvingByKeyWithStructureMap : ComponentResolvingByKey<Agatha.StructureMap.Container>
    {
        protected override IContainer InitializeContainer()
        {
            var container = new StructureMap.Container();
            container.Configure(x => x.ForConcreteType<RequestA>().Configure.Named("KeyForRequestA"));
            container.Configure(x => x.ForConcreteType<RequestB>().Configure.Named("KeyForRequestB"));

            return new Agatha.StructureMap.Container(container);
        }
    }

    public sealed class ComponentResolvingByKeyWithSpring : ComponentResolvingByKey<Agatha.Spring.Container>
    {
        private GenericApplicationContext context;

        protected override IContainer InitializeContainer()
        {
            this.context = new GenericApplicationContext();
            RegisterByKey(typeof(RequestA), typeof(RequestA), "KeyForRequestA");
            RegisterByKey(typeof(RequestB), typeof(RequestB), "KeyForRequestB");

            return new Agatha.Spring.Container(context);
        }

        public void RegisterByKey(Type componentType, Type implementationType, String instanceKey)
        {
            if (this.context == null)
            {
                throw new InvalidOperationException("GenericApplicationContext is not initialized.");
            }

            var factory = new DefaultObjectDefinitionFactory();
            var builder = ObjectDefinitionBuilder.RootObjectDefinition(factory, implementationType);
            builder.SetAutowireMode(AutoWiringMode.AutoDetect);
            this.context.RegisterObjectDefinition(instanceKey, builder.ObjectDefinition);
        }
    }
}
