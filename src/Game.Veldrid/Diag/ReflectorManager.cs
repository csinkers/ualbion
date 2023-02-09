using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using ImGuiNET;

namespace UAlbion.Game.Veldrid.Diag;

public class ReflectorManager
{
    readonly Reflector _nullReflector;
    readonly Dictionary<Type, Reflector> _reflectors = new();
    public static ReflectorManager Instance { get; } = new();

    ReflectorManager()
    {
        _nullReflector = (in ReflectorState state) =>
        {
            var description =
                state.Meta?.Name == null
                    ? "null (null)"
                    : $"{state.Meta.Name}: null (null)";

            ImGui.Indent();
            ImGui.TextWrapped(description);
            ImGui.Unindent();
        };

        void Add<T>(string name) => _reflectors[typeof(T)] = ValueReflectorBuilder.Build(name);
        void Add2<T>(string name, Func<object, string> toString) => _reflectors[typeof(T)] = ValueReflectorBuilder.Build(name, toString);

        Add<bool>("bool");
        Add<byte>("byte");
        Add<ushort>("ushort");
        Add<short>("short");
        Add<uint>("uint");
        Add<int>("int");
        Add<ulong>("ulong");
        Add<long>("long");
        Add<float>("float");
        Add<double>("double");
        Add2<string>("string", x => $"\"{((string)x)?.Replace("\"", "\\\"", StringComparison.Ordinal)}\"");
        Add2<Vector2>("Vector2", x => { var v = (Vector2)x; return $"({v.X}, {v.Y})"; });
        Add2<Vector3>("Vector3", x => { var v = (Vector3)x; return $"({v.X}, {v.Y}, {v.Z})"; });
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

    public static string BuildTypeName(Type type)
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