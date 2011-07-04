using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Agatha.Common.WCF
{
	public static class KnownTypeProvider
	{
        private static List<Type> knownTypes = new List<Type>();

        public static void ClearAllKnownTypes()
        {
            knownTypes = new List<Type>();
        }

        public static void Register<T>()
        {
            Register(typeof(T));
        }

        public static void Register(Type type)
        {
            knownTypes.Add(type);
        }

        public static void RegisterDerivedTypesOf<T>(Assembly assembly)
        {
            RegisterDerivedTypesOf(typeof(T), assembly.GetTypes());
        }

        public static void RegisterDerivedTypesOf<T>(IEnumerable<Type> types)
        {
            RegisterDerivedTypesOf(typeof(T), types);
        }

        public static void RegisterDerivedTypesOf(Type type, Assembly assembly)
        {
            RegisterDerivedTypesOf(type, assembly.GetTypes());
        }

        public static void RegisterDerivedTypesOf(Type type, IEnumerable<Type> types)
        {
            List<Type> derivedTypes = GetDerivedTypesOf(type, types);
            knownTypes = Union(knownTypes, derivedTypes);
        }

        public static IEnumerable<Type> GetKnownTypes(ICustomAttributeProvider provider)
        {
            return knownTypes;
        }

        private static List<Type> GetDerivedTypesOf(Type baseType, IEnumerable<Type> types)
        {
            return types.Where(t => !t.IsAbstract && t.IsSubclassOf(baseType)).ToList();
        }

        private static List<T> Union<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            return first.Union(second).ToList();
        }
    }
}