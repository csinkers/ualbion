using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UAlbion.Api;

namespace UAlbion.Game
{
    public static class ReflectionHelper
    {
        static Type[] _typeCache = BuildTypeCache();
        static Type[] BuildTypeCache()
        {
            PerfTracker.StartupEvent("Start building reflection cache");
            var types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.FullName.Contains("Albion"))
                    continue;
                Type[] assemblyTypes;
                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    assemblyTypes = e.Types;
                }

                foreach (var type in assemblyTypes.Where(x =>
                    x != null &&
                    !x.IsAbstract &&
                    x.GetCustomAttributes(false).Any()))
                {
                    types.Add(type);
                }
            }
            PerfTracker.StartupEvent("Done building reflection cache");
            return types.ToArray();
        }

        public static void ClearTypeCache() { _typeCache = null; }
        public static IEnumerable<(T, TAttribute)> GetAttributeTypes<T, TAttribute>() where TAttribute : Attribute
        {
            foreach (var type in _typeCache)
            {
                if (!typeof(T).IsAssignableFrom(type))
                    continue;

                var attribute = (TAttribute)type.GetCustomAttribute(typeof(TAttribute), false);
                if (attribute != null)
                {
                    var constructor = type.GetConstructors().Single();
                    var lambda = (Func<object>)Expression.Lambda(Expression.New(constructor)).Compile();
                    var loader = (T)lambda();
                    yield return (loader, attribute);
                }
            }
        }
    }
}
