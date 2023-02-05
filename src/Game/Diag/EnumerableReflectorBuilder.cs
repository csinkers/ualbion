using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace UAlbion.Game.Diag;

public class EnumerableReflectorBuilder : IReflectorBuilder
{
    public static EnumerableReflectorBuilder Instance { get; } = new();
    EnumerableReflectorBuilder() { } 
    public Reflector Build(ReflectorManager manager, string name, Type type)
    {
        var getValueFunc = typeof(ICollection).IsAssignableFrom(type)
            ? (Reflector.GetValueDelegate)CollectionGetValue
            : DummyGetValue;

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

        return new Reflector(name, getValueFunc, null, VisitEnumerable);
    }

    static string DummyGetValue(object x) => x == null ? "null" : "<...>";
    static string CollectionGetValue(object x)
    {
        if (x == null) return "null";
        var e = (ICollection)x;
        return e.Count.ToString(CultureInfo.InvariantCulture);
    }
}