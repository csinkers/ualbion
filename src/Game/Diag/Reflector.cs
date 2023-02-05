using System;
using System.Collections.Generic;
using System.Numerics;

namespace UAlbion.Game.Diag;

public class Reflector
{
    readonly TypeReflector _nullReflector;
    readonly Dictionary<Type, TypeReflector> _reflectors = new();

    public static Reflector Instance { get; } = new();

    Reflector()
    {
        _nullReflector = new TypeReflector(this, "null", null, _ => "null");

        void Add<T>(string name, TypeReflector.GetValueDelegate getValue)
            => _reflectors[typeof(T)] = new TypeReflector(this, name, typeof(T), getValue);

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

    public TypeReflector GetReflectorForInstance(object target)
        => target == null
            ? _nullReflector
            : GetReflectorForType(target.GetType());

    public TypeReflector GetReflectorForType(Type type)
    {
        if (_reflectors.TryGetValue(type, out var reflector))
            return reflector;

        reflector = new TypeReflector(this, type);
        _reflectors[type] = reflector; // Needs to be added prior to reflection to prevent infinite recursion
        reflector.Reflect();
        return reflector;
    }
}
