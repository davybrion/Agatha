using System;
using System.Collections.Generic;
using Agatha.Common.InversionOfControl;
using Agatha.Common.WCF;
using sm = StructureMap;

namespace Agatha.StructureMap
{
    public class Container : Agatha.Common.InversionOfControl.IContainer
    {
        private readonly sm.IContainer structureMapContainer;

        private readonly Dictionary<Lifestyle, sm.InstanceScope> lifeStyleMappings = new Dictionary<Lifestyle, sm.InstanceScope>
        {
            {Lifestyle.Singleton, sm.InstanceScope.Singleton},
            {Lifestyle.Transient, sm.InstanceScope.Transient}
        };

        public Container() : this(new sm.Container()) { }

        public Container(sm.IContainer structureMapContainer)
        {
            this.structureMapContainer = structureMapContainer;
        }

        public void Register(Type componentType, Type implementationType, Lifestyle lifeStyle)
        {
            structureMapContainer.Configure(x => x.For(componentType).LifecycleIs(lifeStyleMappings[lifeStyle]).Use(implementationType));

            OverrideConstructorResolvingWhenUsingRequestProcessorProxy(implementationType);
        }

        private void OverrideConstructorResolvingWhenUsingRequestProcessorProxy(Type implementationType)
        {
            if (implementationType == typeof(RequestProcessorProxy))
            {
                structureMapContainer.Configure(x => x.SelectConstructor(() => new RequestProcessorProxy()));
            }
            if (implementationType == typeof(AsyncRequestProcessorProxy))
            {
                structureMapContainer.Configure(x => x.SelectConstructor(() => new AsyncRequestProcessorProxy()));
            }
        }

        public void Register<TComponent, TImplementation>(Lifestyle lifestyle) where TImplementation : TComponent
        {
            Register(typeof(TComponent), typeof(TImplementation), lifestyle);
        }

        public void RegisterInstance(Type componentType, object instance)
        {
            structureMapContainer.Configure(x => x.For(componentType).Use(instance));
        }

        public void RegisterInstance<TComponent>(TComponent instance)
        {
            structureMapContainer.Configure(x => x.For<TComponent>().Use(instance));
        }

        public TComponent Resolve<TComponent>()
        {
            return structureMapContainer.GetInstance<TComponent>();
        }

        public TComponent Resolve<TComponent>(string key)
        {
            return structureMapContainer.GetInstance<TComponent>(key);
        }

        public object Resolve(Type componentType)
        {
            return structureMapContainer.GetInstance(componentType);
        }

        public TComponent TryResolve<TComponent>()
        {
            return structureMapContainer.TryGetInstance<TComponent>();
        }

        public void Release(object component)
        {
            // NOTE: i think this needs to be a no-op in the case of structuremap... the code below was in the original patch but
            // i don't think its equivalent to Windsor's Release
            //structureMapContainer.Model.EjectAndRemove(component.GetType());
        }
    }
}
