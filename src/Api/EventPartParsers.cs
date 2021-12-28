using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace UAlbion.Api;

public class EventPartParsers
{
    readonly IDictionary<Type, MethodInfo> _parsers = new Dictionary<Type, MethodInfo>();
    readonly MethodInfo _isNullOrEmpty = typeof(string).GetMethod("IsNullOrEmpty", BindingFlags.Static | BindingFlags.Public);

    public EventPartParsers()
    {
        _parsers[typeof(bool)]    =   typeof(bool).GetMethod("Parse", new[] { typeof(string) });
        _parsers[typeof(byte)]    =   typeof(byte).GetMethod("Parse", new[] { typeof(string) });
        _parsers[typeof(ushort)]  = typeof(ushort).GetMethod("Parse", new[] { typeof(string) });
        _parsers[typeof(short)]   =  typeof(short).GetMethod("Parse", new[] { typeof(string) });
        _parsers[typeof(int)]     =    typeof(int).GetMethod("Parse", new[] { typeof(string) });
        _parsers[typeof(uint)]    =   typeof(uint).GetMethod("Parse", new[] { typeof(string) });
        _parsers[typeof(float)]   =  typeof(float).GetMethod("Parse", new[] { typeof(string) });
    }

    public Expression GetParser(Type type, Expression argument)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (type == typeof(string))
            return argument;

        if (Nullable.GetUnderlyingType(type) is { } underlying)
        {
            var underlyingParse = GetParseMethod(underlying);
            var expression =
                Expression.Condition(
                    Expression.Call(null, _isNullOrEmpty, argument),
                    Expression.Constant(null, type),
                    Expression.Convert(Expression.Call(null, underlyingParse, argument), type));
            return expression;
        }

        var method = GetParseMethod(type);
        return method == null ? null : Expression.Call(method, argument);
    }

    MethodInfo GetParseMethod(Type type)
    {
        if (_parsers.TryGetValue(type, out var parser))
            return parser;

        if (type.IsEnum)
        {
            var methodName = ApiUtil.IsFlagsEnum(type) ? "ParseFlags" : "ParseEnum";
            var method = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
                throw new InvalidOperationException("Method ParseEnum could not be found");

            parser = method.MakeGenericMethod(type);
        }
        else
        {
            parser = type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .SingleOrDefault(x => x.Name == "Parse" && x.GetParameters().Length == 1);
        }

        if (parser != null)
            _parsers[type] = parser;

        return parser;
    }

    // Methods called via reflection
#pragma warning disable IDE0051 // Remove unused private members
    static T ParseEnum<T>(string s) => (T)Enum.Parse(typeof(T), s, true);
    static T? ParseNullableEnum<T>(string s) where T : struct, Enum => string.IsNullOrEmpty(s) ? null : (T?)Enum.Parse(typeof(T), s, true);
    static T ParseFlags<T>(string s) where T : unmanaged, Enum
    {
        if (string.IsNullOrEmpty(s))
            return default;

        int value = 0;
        var parts = s.Split('|');

        foreach (var part in parts)
        {
            if (!Enum.TryParse<T>(part, true, out var id))
                throw new FormatException($"Could not parse \"{part}\" in \"{s}\" to enum {typeof(T).Name}");

            unsafe
            {
                value |=
                    sizeof(T) == 1 ? Unsafe.As<T, byte>(ref id)
                    : sizeof(T) == 2 ? Unsafe.As<T, ushort>(ref id)
                    : sizeof(T) == 4 ? Unsafe.As<T, int>(ref id)
                    : throw new InvalidOperationException($"Type {typeof(T)} is of non-enum type, or has an unsupported underlying type");
            }
        }

        unsafe
        {
            if (sizeof(T) == 1) { byte byteVal = (byte)value; return Unsafe.As<byte, T>(ref byteVal); } 
            if (sizeof(T) == 2) { ushort ushortVal = (ushort)value; return Unsafe.As<ushort, T>(ref ushortVal); } 
            if (sizeof(T) == 4) { return Unsafe.As<int, T>(ref value); }
            throw new InvalidOperationException($"Type {typeof(T)} is of non-enum type, or has an unsupported underlying type");
        }
    }
#pragma warning restore IDE0051 // Remove unused private members
}