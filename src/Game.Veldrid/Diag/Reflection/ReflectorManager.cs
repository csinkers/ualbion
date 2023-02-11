using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace UAlbion.Game.Veldrid.Diag.Reflection;

public class ReflectorManager
{
    readonly Reflector _nullReflector;
    readonly Dictionary<Type, Reflector> _reflectors = new();
    public static ReflectorManager Instance { get; } = new();

    ReflectorManager()
    {
        void Add<T>(string name) => _reflectors[typeof(T)] = ValueReflector.Build(name);
        void Add2<T>(string name, Func<object, string> toString) => _reflectors[typeof(T)] = ValueReflector.Build(name, toString);

        _nullReflector              = NullReflector.Instance.Reflect;
        _reflectors[typeof(bool)]   = BoolReflector.Instance.Reflect;
        _reflectors[typeof(string)] = StringReflector.Instance.Reflect;
        _reflectors[typeof(byte)]   = IntegralValueReflector.Build("byte",   x => (byte)x);
        _reflectors[typeof(sbyte)]  = IntegralValueReflector.Build("sbyte",  x => (sbyte)x);
        _reflectors[typeof(ushort)] = IntegralValueReflector.Build("ushort", x => (ushort)x);
        _reflectors[typeof(short)]  = IntegralValueReflector.Build("short",  x => (short)x);
        _reflectors[typeof(uint)]   = IntegralValueReflector.Build("uint",   x => (int)(uint)x);
        _reflectors[typeof(int)]    = IntegralValueReflector.Build("int",    x => (int)x);
        _reflectors[typeof(ulong)]  = IntegralValueReflector.Build("ulong",  x => (int)(ulong)x);
        _reflectors[typeof(long)]   = IntegralValueReflector.Build("long",   x => (int)(long)x);

        Add<float>("float");
        Add<double>("double");
        Add2<Vector2>("Vector2", x => { var v = (Vector2)x; return $"({v.X}, {v.Y})"; });
        Add2<Vector3>("Vector3", x => { var v = (Vector3)x; return $"({v.X}, {v.Y}, {v.Z})"; });
        Add2<Vector4>("Vector4", x => { var v = (Vector4)x; return $"({v.X}, {v.Y}, {v.Z}, {v.W})"; });
    }

    public Reflector GetReflectorForInstance(object target)
        => target == null
            ? _nullReflector
            : GetReflectorForType(target.GetType());

    Reflector GetReflectorForType(Type type)
    {
        if (_reflectors.TryGetValue(type, out var reflector))
            return reflector;

        reflector = Reflect(type);
        _reflectors[type] = reflector;
        return reflector;
    }

    Reflector Reflect(Type type)
    {
        if (typeof(Enum).IsAssignableFrom(type))
            return EnumReflector.Build(type);

        if (typeof(IEnumerable).IsAssignableFrom(type))
            return EnumerableReflectorBuilder.Build(this, type);

        return ObjectReflectorBuilder.Build(this, type);
    }
}