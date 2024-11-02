using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;

namespace UAlbion.Core.Veldrid.Reflection;

public class ReflectorManager
{
    readonly Reflector _nullReflector;
    readonly Dictionary<Type, Reflector> _reflectors = new();
    public static ReflectorManager Instance { get; } = new();

    ReflectorManager()
    {
        // void Add<T>(string name) => _reflectors[typeof(T)] = new ValueReflector(name).Reflect;
        void Add2<T>(string name, Func<object, string> toString) => _reflectors[typeof(T)] = new ValueReflector(name, toString).Reflect;

        _nullReflector              = NullReflector.Instance.Reflect;
        _reflectors[typeof(bool)]   = BoolReflector.Instance.Reflect;
        _reflectors[typeof(string)] = StringReflector.Instance.Reflect;
        _reflectors[typeof(Vector3)] = Vec3Reflector.Instance.Reflect;
        _reflectors[typeof(Vector4)] = Vec4Reflector.Instance.Reflect;
        _reflectors[typeof(byte)]   = new IntReflector("byte",   x => (byte)x).Reflect;
        _reflectors[typeof(sbyte)]  = new IntReflector("sbyte",  x => (sbyte)x).Reflect;
        _reflectors[typeof(ushort)] = new IntReflector("ushort", x => (ushort)x).Reflect;
        _reflectors[typeof(short)]  = new IntReflector("short",  x => (short)x).Reflect;
        _reflectors[typeof(uint)]   = new IntReflector("uint",   x => (int)(uint)x).Reflect;
        _reflectors[typeof(int)]    = new IntReflector("int",    x => (int)x).Reflect;
        _reflectors[typeof(ulong)]  = new IntReflector("ulong",  x => (int)(ulong)x).Reflect;
        _reflectors[typeof(long)]   = new IntReflector("long",   x => (int)(long)x).Reflect;
        _reflectors[typeof(float)]  = new FloatReflector("float", x => (float)x).Reflect;
        _reflectors[typeof(double)] = new FloatReflector("double", x => (float)(double)x).Reflect;

        Add2<Vector2>("Vector2", x => { var v = (Vector2)x; return $"({v.X}, {v.Y})"; });
    }

    public void RenderNode(string name, object target)
    {
        var meta = new ReflectorMetadata(name, null, null, null);
        var state = new ReflectorState(target, null, -1, meta);
        var reflector = GetReflectorForInstance(state.Target);
        reflector(state);
    }

    public Reflector GetReflectorForInstance(object target)
        => target == null
            ? _nullReflector
            : GetReflectorForType(target.GetType());

    Reflector GetReflectorForType(Type type)
    {
        if (_reflectors.TryGetValue(type, out var reflector))
            return reflector;

        reflector = BuildReflector(type);
        _reflectors[type] = reflector;
        return reflector;
    }

    Reflector BuildReflector(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (typeof(Enum).IsAssignableFrom(type))
            return EnumReflector.Build(type);

        if (typeof(IEnumerable).IsAssignableFrom(type))
            return new EnumerableReflector(this, type).Reflect;

        if (typeof(Component).IsAssignableFrom(type))
            return new ObjectReflector(this, type).ReflectComponent;

        return new ObjectReflector(this, type).Reflect;
    }
}