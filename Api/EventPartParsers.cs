using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace UAlbion.Api
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class EventPartParsers
    {
        readonly IDictionary<Type, MethodInfo> _parsers = new Dictionary<Type, MethodInfo>();

        public EventPartParsers()
        {
            _parsers[typeof(bool)]   = typeof(bool).GetMethod("Parse", new[] { typeof(string) });
            _parsers[typeof(byte)]   = typeof(byte).GetMethod("Parse", new[] { typeof(string) });
            _parsers[typeof(ushort)] = typeof(ushort).GetMethod("Parse", new[] { typeof(string) });
            _parsers[typeof(short)]  = typeof(short).GetMethod("Parse", new[] { typeof(string) });
            _parsers[typeof(int)]    = typeof(int).GetMethod("Parse", new[] { typeof(string) });
            _parsers[typeof(uint)]   = typeof(uint).GetMethod("Parse", new[] { typeof(string) });
            _parsers[typeof(float)]  = typeof(float).GetMethod("Parse", new[] { typeof(string) });
            _parsers[typeof(bool?)]  = GetType().GetMethod("ParseNullableBool", BindingFlags.NonPublic | BindingFlags.Static);
            _parsers[typeof(int?)]   = GetType().GetMethod("ParseNullableInt", BindingFlags.NonPublic | BindingFlags.Static);
            _parsers[typeof(float?)] = GetType().GetMethod("ParseNullableFloat", BindingFlags.NonPublic | BindingFlags.Static);
        }

        public MethodInfo GetParser(Type type)
        {
            if (_parsers.TryGetValue(type, out var parser))
                return parser;

            if (type.IsEnum)
            {
                var method = GetType().GetMethod("ParseEnum", BindingFlags.NonPublic | BindingFlags.Static);
                if(method == null)
                    throw new InvalidOperationException("Method ParseEnum could not be found");

                parser = method.MakeGenericMethod(type);
                _parsers[type] = parser;
                return parser;
            }

            if (Nullable.GetUnderlyingType(type) is { } underlying && underlying.IsEnum)
            {
                var method = GetType().GetMethod("ParseNullableEnum", BindingFlags.NonPublic | BindingFlags.Static);
                if(method == null)
                    throw new InvalidOperationException("Method ParseNullableEnum could not be found");

                parser = method.MakeGenericMethod(underlying);
                _parsers[type] = parser;
                return parser;
            }

            return null;
        }

        static bool? ParseNullableBool(string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;
            return bool.Parse(s);
        }

        static int? ParseNullableInt(string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;
            return int.Parse(s);
        }

        static float? ParseNullableFloat(string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;
            return float.Parse(s);
        }

        static T ParseEnum<T>(string s) => (T)Enum.Parse(typeof(T), s, true);
        static T? ParseNullableEnum<T>(string s) where T : struct, Enum => string.IsNullOrEmpty(s) ? null : (T?)Enum.Parse(typeof(T), s, true);
    }
}
