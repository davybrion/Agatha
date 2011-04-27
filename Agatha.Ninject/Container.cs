using System;
using Agatha.Common.InversionOfControl;
using Ninject;
using Ninject.Syntax;

namespace Agatha.Ninject
{
	public class Container : IContainer
	{
		private readonly IKernel kernel;

		public Container() : this(new StandardKernel()) { }

		public Container(IKernel kernel)
		{
			this.kernel = kernel;
		}

		public void Register(Type componentType, Type implementationType, Lifestyle lifeStyle)
		{
			var binding = kernel.Bind(componentType).To(implementationType);
			AddLifeStyleToRegistration(lifeStyle, binding);
		}

        public void Register<TComponent, TImplementation>(Lifestyle lifestyle) where TImplementation : TComponent
		{
			Register(typeof(TComponent), typeof(TImplementation), lifestyle);
		}

		public void RegisterInstance(Type componentType, object instance)
		{
			kernel.Bind(componentType).ToMethod(context => instance);
		}

		public void RegisterInstance<TComponent>(TComponent instance)
		{
			RegisterInstance(typeof(TComponent), instance);
		}

		public TComponent Resolve<TComponent>()
		{
			return kernel.Get<TComponent>();
		}

        public TComponent Resolve<TComponent>(string key)
        {
            return kernel.Get<TComponent>(key);
        }

		public object Resolve(Type componentType)
		{
			return kernel.Get(componentType);
		}

		public void Release(object component)
		{
			// not supported
		}

		private static void AddLifeStyleToRegistration<TInterface>(Lifestyle lifestyle, IBindingInSyntax<TInterface> registration)
		{
			if (lifestyle == Lifestyle.Singleton)
			{
				registration.InSingletonScope();
			}
			else if (lifestyle == Lifestyle.Transient)
			{
				registration.InTransientScope();
			}
			else
			{
				throw new ArgumentOutOfRangeException("lifestyle", "Only Transient and Singleton is supported");
			}
		}
	}
}