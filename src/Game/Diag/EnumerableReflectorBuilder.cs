using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace UAlbion.Game.Diag;

public static class EnumerableReflectorBuilder
{
    public static void Build(IReflectorConfigurer config, Reflector reflector, Type type)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));
        var getValueFunc = typeof(ICollection).IsAssignableFrom(type)
            ? (Reflector.GetValueDelegate)CollectionGetValue
            : DummyGetValue;

        var manager = config.GetManager(reflector);
        IEnumerable<ReflectorState> VisitEnumerable(object target)
        {
            if (target == null) yield break;
            int index = 0;
            foreach (var child in (IEnumerable)target)
            {
                var childReflector = manager.GetReflectorForInstance(child);
                yield return new ReflectorState(index.ToString(CultureInfo.InvariantCulture), child, childReflector, target, index);
                index++;
            }
        }

        config.AssignGetValueFunc(reflector, getValueFunc);
        config.AssignSubObjectsFunc(reflector, VisitEnumerable);
    }

    static string DummyGetValue(object x) => x == null ? "null" : "<...>";
    static string CollectionGetValue(object x)
    {
        if (x == null) return "null";
        var e = (ICollection)x;
        return e.Count.ToString(CultureInfo.InvariantCulture);
    }
}