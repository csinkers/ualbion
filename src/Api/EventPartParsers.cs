using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace UAlbion.Api
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
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
                var method = GetType().GetMethod("ParseEnum", BindingFlags.NonPublic | BindingFlags.Static);
                if(method == null)
                    throw new InvalidOperationException("Method ParseEnum could not be found");

                parser = method.MakeGenericMethod(type);
            }
            else
            {
                parser = type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public);
            }

            if (parser != null)
                _parsers[type] = parser;

            return parser;
        }

        // Methods called via reflection
#pragma warning disable IDE0051 // Remove unused private members
        static T ParseEnum<T>(string s) => (T)Enum.Parse(typeof(T), s, true);
        static T? ParseNullableEnum<T>(string s) where T : struct, Enum => string.IsNullOrEmpty(s) ? null : (T?)Enum.Parse(typeof(T), s, true);
#pragma warning restore IDE0051 // Remove unused private members
    }
}
