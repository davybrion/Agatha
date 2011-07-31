using System;
using Agatha.Common.InversionOfControl;
using Spring.Context.Support;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

namespace Agatha.Spring
{
    public class Container : IContainer
    {
        private readonly GenericApplicationContext context;

        public Container(GenericApplicationContext context)
        {
            this.context = context;
        }
        public Container() : this(new GenericApplicationContext()) { }

        public void Register(Type componentType, Type implementationType, Lifestyle lifeStyle)
        {
            var factory = new DefaultObjectDefinitionFactory();
            var builder = ObjectDefinitionBuilder.RootObjectDefinition(factory, implementationType);
            builder.SetAutowireMode(AutoWiringMode.AutoDetect);
            builder.SetSingleton(lifeStyle == Lifestyle.Singleton);
            context.RegisterObjectDefinition(componentType.FullName, builder.ObjectDefinition);
        }

        public void Register<TComponent, TImplementation>(Lifestyle lifestyle) where TImplementation : TComponent
        {
            Register(typeof(TComponent), typeof(TImplementation), lifestyle);
        }

        public void RegisterInstance(Type componentType, object instance)
        {
            context.ObjectFactory.RegisterSingleton(componentType.FullName, instance);
        }

        public void RegisterInstance<TComponent>(TComponent instance)
        {
            context.ObjectFactory.RegisterSingleton(typeof(TComponent).FullName, instance);
        }

        public TComponent Resolve<TComponent>()
        {
            return (TComponent)Resolve(typeof(TComponent));
        }

        public TComponent Resolve<TComponent>(string key)
        {
            return (TComponent)context.GetObject(key);
        }

        public object Resolve(Type componentType)
        {
            var instance = context.GetObject(componentType.FullName);
            context.ConfigureObject(instance, componentType.FullName);
            return instance;
        }

        public TComponent TryResolve<TComponent>()
        {
            if (context.ContainsObjectDefinition(typeof(TComponent).FullName)) return Resolve<TComponent>();
            return default(TComponent);
        }

        public void Release(object component)
        {
            //not supported?
        }
    }
}
