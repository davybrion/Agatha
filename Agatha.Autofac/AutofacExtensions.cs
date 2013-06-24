using System;
using Agatha.Common.InversionOfControl;
using Autofac;
using Autofac.Builder;
using IAutofacContainer = Autofac.IContainer;

namespace Agatha.Autofac
{
    internal static class AutofacExtensions
    {
        public static IAutofacContainer Update(this IAutofacContainer container, Action<ContainerBuilder> updateAction)
        {
            var updater = new ContainerBuilder();
            updateAction(updater);
            updater.Update(container);
            return container;
        }

        public static IRegistrationBuilder<TComponent, ConcreteReflectionActivatorData, SingleRegistrationStyle> WithLifestyle<TComponent>(this IRegistrationBuilder<TComponent, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration, Lifestyle lifeStyle)
        {
            switch (lifeStyle)
            {
                case Lifestyle.Singleton:
                    registration.SingleInstance();
                    break;
                case Lifestyle.Transient:
                    registration.InstancePerDependency();
                    break;
                default:
                    throw new NotSupportedException("lifeStyle " + lifeStyle + " is not supported");
            }
            return registration;
        }
    }
}
