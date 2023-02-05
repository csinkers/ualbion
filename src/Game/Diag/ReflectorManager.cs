using System;
using System.Collections.Generic;
using System.Numerics;

namespace UAlbion.Game.Diag;

public class ReflectorManager
{
    readonly Reflector _nullReflector;
    readonly Dictionary<Type, Reflector> _reflectors = new();

    public static ReflectorManager Instance { get; } = new();

    ReflectorManager()
    {
        _nullReflector = new Reflector(this, "null", null, _ => "null");

        void Add<T>(string name, Reflector.GetValueDelegate getValue)
            => _reflectors[typeof(T)] = new Reflector(this, name, typeof(T), getValue);

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

        reflector = new Reflector(this, type);
        _reflectors[type] = reflector; // Needs to be added prior to reflection to prevent infinite recursion
        reflector.Reflect();
        return reflector;
    }
}
