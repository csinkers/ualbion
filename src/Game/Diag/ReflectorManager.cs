using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace UAlbion.Game.Diag;

public class ReflectorManager
{
    readonly Reflector _nullReflector;
    readonly Dictionary<Type, Reflector> _reflectors = new();

    public static ReflectorManager Instance { get; } = new();

    ReflectorManager()
    {
        _nullReflector = new Reflector("null", _ => "null");

        void Add<T>(string name, Reflector.GetValueDelegate getValue)
            => _reflectors[typeof(T)] = new Reflector(name, getValue);

        Add<string>("string", x => $"\"{((string)x)?.Replace("\"", "\\\"", StringComparison.Ordinal)}\"");
        Add<bool>("bool", x => x.ToString());
        Add<byte>("byte", x => x.ToString());
        Add<ushort>("ushort", x => x.ToString());
        Add<short>("short", x => x.ToString());
        Add<uint>("uint", x => x.ToString());
        Add<int>("int", x => x.ToString());
        Add<ulong>("ulong", x => x.ToString());
        Add<long>("long", x => x.ToString());
        Add<float>("float", x => x.ToString());
        Add<double>("double", x => x.ToString());
        Add<Vector2>("Vector2", x => { var v = (Vector2)x; return $"({v.X}, {v.Y})"; });
        Add<Vector3>("Vector3", x => { var v = (Vector3)x; return $"({v.X}, {v.Y}, {v.Z})"; });
    }

    public Reflector GetReflectorForInstance(object target)
        => target == null
            ? _nullReflector
            : GetReflectorForType(target.GetType());

    public Reflector GetReflectorForType(Type type)
    {
        if (_reflectors.TryGetValue(type, out var reflector))
            return reflector;

        reflector = Reflect(type);
        _reflectors[type] = reflector;
        return reflector;
    }

    Reflector Reflect(Type type)
    {
        var name = BuildTypeName(type);
        if (typeof(Enum).IsAssignableFrom(type))
            return new Reflector(name);

        if (typeof(IEnumerable).IsAssignableFrom(type))
            return EnumerableReflectorBuilder.Instance.Build(this, name, type);

        return ObjectTypeReflectorBuilder.Instance.Build(this, name, type);
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
