using System;
using Agatha.Common.InversionOfControl;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace Agatha.Castle
{
	public class Container : IContainer
	{
		private readonly IWindsorContainer windsorContainer;

		public Container() : this(new WindsorContainer()) {}

		public Container(IWindsorContainer windsorContainer)
		{
			this.windsorContainer = windsorContainer;
		}

		public void Register(Type componentType, Type implementationType, Lifestyle lifeStyle)
		{
			var registration = Component.For(componentType).ImplementedBy(implementationType);
			windsorContainer.Register(AddLifeStyleToRegistration(lifeStyle, registration));
		}

        public void Register<TComponent, TImplementation>(Lifestyle lifestyle) where TImplementation : TComponent
		{
			Register(typeof(TComponent), typeof(TImplementation), lifestyle);
		}

		public void RegisterInstance(Type componentType, object instance)
		{
			windsorContainer.Register(Component.For(componentType).Instance(instance));
		}

		public void RegisterInstance<TComponent>(TComponent instance)
		{
			RegisterInstance(typeof(TComponent), instance);
		}

		public TComponent Resolve<TComponent>()
		{
			return windsorContainer.Resolve<TComponent>();
		}

        public TComponent Resolve<TComponent>(string key)
        {
            return windsorContainer.Resolve<TComponent>(key);
        }

		public object Resolve(Type componentType)
		{
			return windsorContainer.Resolve(componentType);
		}

		public void Release(object component)
		{
			windsorContainer.Release(component);
		}

		private static ComponentRegistration<TInterface> AddLifeStyleToRegistration<TInterface>(Lifestyle lifestyle, ComponentRegistration<TInterface> registration)
		{
			if (lifestyle == Lifestyle.Singleton)
			{
				registration = registration.LifeStyle.Singleton;
			}
			else if (lifestyle == Lifestyle.Transient)
			{
				registration = registration.LifeStyle.Transient;
			}
			else
			{
				throw new ArgumentOutOfRangeException("lifestyle", "Only Transient and Singleton is supported");
			}

			return registration;
		}
	}
}