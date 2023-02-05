using System;
using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Game.Diag;

public class Reflector
{
    static string DefaultGetValue(object target) => target?.ToString() ?? "null";
    static IEnumerable<ReflectorState> NoChildren(object target) => Enumerable.Empty<ReflectorState>();

    public delegate string GetValueDelegate(object target);
    public delegate void SetValueDelegate(object target, object value);
    public delegate IEnumerable<ReflectorState> VisitChildrenDelegate(object target);

    public string TypeName { get; }
    public bool HasSubObjects => SubObjects != NoChildren;
    public GetValueDelegate GetValue { get; } = DefaultGetValue;
    public SetValueDelegate SetValue { get; } = null;
    public VisitChildrenDelegate SubObjects { get; } = NoChildren;
    public override string ToString() => $"{TypeName} reflector";

    public Reflector(
        string name,
        GetValueDelegate getValue = null,
        SetValueDelegate setValue = null,
        VisitChildrenDelegate subObjects = null)
    {
        TypeName = name;
        GetValue = getValue ?? DefaultGetValue;
        SetValue = setValue;
        SubObjects = subObjects ?? NoChildren;
    }
}