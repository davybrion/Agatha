using System;
using Agatha.Common.InversionOfControl;
using Agatha.Common.WCF;
using Microsoft.Practices.Unity;

namespace Agatha.Unity
{
	public class Container : IContainer
	{
		private readonly IUnityContainer unityContainer;

		public Container() : this(new UnityContainer()) { }

		public Container(IUnityContainer unityContainer)
		{
			this.unityContainer = unityContainer;
		}

		public void Register(Type componentType, Type implementationType, Lifestyle lifeStyle)
		{
			LifetimeManager lifetimeManager = GetLifetimeManager(lifeStyle);

			if (!AppliedSpecificRegistrionToDealWithTheStupidityOfUnity(componentType, implementationType, lifetimeManager))
			{
				unityContainer.RegisterType(componentType, implementationType, lifetimeManager);
			}
		}

        public void Register<TComponent, TImplementation>(Lifestyle lifestyle) where TImplementation : TComponent
		{
			Register(typeof(TComponent), typeof(TImplementation), lifestyle);
		}

		public void RegisterInstance(Type componentType, object instance)
		{
			unityContainer.RegisterInstance(componentType, instance);
		}

		public void RegisterInstance<TComponent>(TComponent instance)
		{
			RegisterInstance(typeof(TComponent), instance);
		}

		public TComponent Resolve<TComponent>()
		{
			return unityContainer.Resolve<TComponent>();
		}

        public TComponent Resolve<TComponent>(string key)
        {
            return unityContainer.Resolve<TComponent>(key);
        }

		public object Resolve(Type componentType)
		{
			return unityContainer.Resolve(componentType);
		}

		public void Release(object component)
		{
			unityContainer.Teardown(component);
		}

		private LifetimeManager GetLifetimeManager(Lifestyle lifestyle)
		{
			if (lifestyle == Lifestyle.Transient)
			{
				return new TransientLifetimeManager();
			}

			return new ContainerControlledLifetimeManager();
		}

		private bool AppliedSpecificRegistrionToDealWithTheStupidityOfUnity(Type componentType, Type implementationType, LifetimeManager lifetimeManager)
		{
			// HACK: Unity is downright stupid when it comes to selecting the constructor to use when instantiating components... (http://codebetter.com/blogs/david.hayden/archive/2008/10/27/specifying-injection-constructor-using-unity-fluent-interface-for-loose-coupling-and-poco.aspx)

#if !SILVERLIGHT
			if (implementationType == typeof(RequestProcessorProxy))
			{
				unityContainer.RegisterType(componentType, implementationType, lifetimeManager, new InjectionConstructor());
				return true;
			}
#endif

			if (implementationType == typeof(AsyncRequestProcessorProxy))
			{
				unityContainer.RegisterType(componentType, implementationType, lifetimeManager, new InjectionConstructor());
				return true;
			}

			return false;
		}
	}
}