using System;
using Agatha.Common.InversionOfControl;
using Autofac;
using IAutofacContainer = Autofac.IContainer;

namespace Agatha.Autofac
{
    public class Container : Agatha.Common.InversionOfControl.IContainer
    {
        private readonly IAutofacContainer container;

        public Container() : this((new ContainerBuilder()).Build()) { }

        public Container(IAutofacContainer container)
        {
            this.container = container;
        }

        public void Register(Type componentType, Type implementationType, Lifestyle lifeStyle)
        {
            container.Update(c => c.RegisterType(implementationType).As(componentType).WithLifestyle(lifeStyle));
        }

        public void Register<TComponent, TImplementation>(Lifestyle lifestyle) 
            where TImplementation : TComponent
        {
            container.Update(c => c.RegisterType<TImplementation>().As<TComponent>().WithLifestyle(lifestyle));
        }

        public void RegisterInstance(Type componentType, object instance)
        {
            container.Update(c => c.Register(x => instance).As(componentType));
        }

        public void RegisterInstance<TComponent>(TComponent instance)
        {
            container.Update(c => c.Register(x => instance).AsSelf());
        }

        public TComponent Resolve<TComponent>()
        {
            return container.Resolve<TComponent>();
        }

        public TComponent Resolve<TComponent>(string key)
        {
            return container.ResolveKeyed<TComponent>(key);
        }

        public object Resolve(Type componentType)
        {
            return container.Resolve(componentType);
        }

        public TComponent TryResolve<TComponent>()
        {
            TComponent instance;
            return container.TryResolve(out instance) ? instance : default(TComponent); 
        }

        public void Release(object component)
        {
        }
    }
}
