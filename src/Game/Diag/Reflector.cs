using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UAlbion.Game.Diag;

public class Reflector
{
    class Configurer : IReflectorConfigurer
    {
        public ReflectorManager GetManager(Reflector reflector) => reflector._manager;
        public void AssignGetValueFunc(Reflector reflector, GetValueDelegate func) => reflector.GetValue = func;
        public void AssignSetValueFunc(Reflector reflector, SetValueDelegate func) => reflector.SetValue = func;
        public void AssignSubObjectsFunc(Reflector reflector, VisitChildrenDelegate func) => reflector.SubObjects = func;
    }

    static string DefaultGetValue(object target) => target?.ToString() ?? "null";
    static IEnumerable<ReflectorState> NoChildren(object target) => Enumerable.Empty<ReflectorState>();
    static readonly Configurer ConfigurerInstance = new();

    public delegate IEnumerable<ReflectorState> VisitChildrenDelegate(object target);
    public delegate string GetValueDelegate(object target);
    public delegate void SetValueDelegate(object target, object value);

    readonly ReflectorManager _manager;
    readonly Type _type;

    public string TypeName { get; }
    public bool HasSubObjects => SubObjects != NoChildren;
    public GetValueDelegate GetValue { get; private set; } = DefaultGetValue;
    public SetValueDelegate SetValue { get; private set; } = null;
    public VisitChildrenDelegate SubObjects { get; private set; } = NoChildren;
    public override string ToString() => $"{TypeName} reflector";

    public Reflector(ReflectorManager manager, string name, Type type, GetValueDelegate getValue)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _type = type;
        TypeName = name;
        GetValue = getValue ?? throw new ArgumentNullException(nameof(getValue));
    }

    public Reflector(ReflectorManager manager, Type type)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _type = type ?? throw new ArgumentNullException(nameof(type));
        TypeName = BuildTypeName(type);
    }

    internal void Reflect() // Only called by Reflector
    {
        if (typeof(Enum).IsAssignableFrom(_type))
            return;

        if (typeof(IEnumerable).IsAssignableFrom(_type))
        {
            EnumerableReflectorBuilder.Build(ConfigurerInstance, this, _type);
            return;
        }

        ObjectTypeReflectorBuilder.Build(ConfigurerInstance, this, _type);
    }

    static string BuildTypeName(Type type)
    {
        if (type == null)
            return "null";

        var generic = type.GetGenericArguments();
        if (generic.Length == 0)
            return type.Name;

        int index = type.Name.IndexOf('`', StringComparison.Ordinal);
        if (index == -1)
            return type.Name;

        var sb = new StringBuilder();
        sb.Append(type.Name[..index]);
        sb.Append('<');
        bool first = true;
        foreach (var arg in generic)
        {
            if (!first)
                sb.Append(", ");

            sb.Append(BuildTypeName(arg));
            first = false;
        }

        sb.Append('>');
        return sb.ToString();
    }
}