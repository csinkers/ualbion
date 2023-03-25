using System;
using System.Linq.Expressions;
using ImGuiNET;

namespace UAlbion.Core.Veldrid.Reflection;

public class EnumReflector
{
    protected string TypeName { get; }

    protected EnumReflector(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        TypeName = ReflectorUtil.BuildTypeName(type);
    }

    public static Reflector Build(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (!type.IsEnum) throw new ArgumentException($"EnumReflector was given a non-enum type \"{type}\"", nameof(type));

        Type reflectorType;
        object toNum, fromNum;

        var underlying = Enum.GetUnderlyingType(type);
        if (underlying == typeof(byte)
            || underlying == typeof(ushort)
            || underlying == typeof(uint)
            || underlying == typeof(ulong))
        {
            reflectorType = typeof(UnsignedEnumReflector<>).MakeGenericType(type);
            toNum = BuildToNum(type, underlying, typeof(ulong));
            fromNum = BuildFromNum(type, underlying, typeof(ulong));
        }
        else if (underlying == typeof(sbyte)
            || underlying == typeof(short)
            || underlying == typeof(int)
            || underlying == typeof(long))
        {
            reflectorType = typeof(SignedEnumReflector<>).MakeGenericType(type);
            toNum = BuildToNum(type, underlying, typeof(long));
            fromNum = BuildFromNum(type, underlying, typeof(long));
        }
        else 
            throw new ArgumentException($"Tried to build EnumReflector for \"{type}\" with unsupported underlying type \"{underlying}\"");

        var constructor = reflectorType.GetConstructors()[0];
		var instance = (IReflector)constructor.Invoke(new[] { toNum, fromNum });
        if (instance == null)
            throw new InvalidOperationException($"Could not instantiate EnumReflector for \"{type}\"");

        return instance.Reflect;
    }

    protected void RenderLabel(in ReflectorState state)
    {
        var description = ReflectorUtil.Describe(state, TypeName, state.Target);
        ImGui.Indent();
        ImGui.TextWrapped(description);
        ImGui.Unindent();
    }

    static object BuildToNum(Type type, Type underlying, Type numeric)
    {
        // e.g. underlying = uint, numeric = ulong: x => (ulong)(uint)x;
        // e.g. underlying = long, numeric = long:  x => (long)x;
        var parameter = Expression.Parameter(type, "x");
        var conversion = Expression.Convert(parameter, underlying);

        if (underlying != numeric)
            conversion = Expression.Convert(conversion, numeric);

        return Expression.Lambda(conversion, parameter).Compile();
    }

    static object BuildFromNum(Type type, Type underlying, Type numeric)
    {
        // e.g. underlying = uint, numeric = ulong: x => (type)(uint)x;
        // e.g. underlying = long, numeric = long:  x => (type)x;
        var parameter = Expression.Parameter(numeric, "x");
        var conversion = underlying == numeric 
            ? Expression.Convert(parameter, type) 
            : Expression.Convert(Expression.Convert(parameter, underlying), type);

        return Expression.Lambda(conversion, parameter).Compile();
    }
}
