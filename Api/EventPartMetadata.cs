using System;
using System.Linq.Expressions;
using System.Reflection;

namespace UAlbion.Api
{
    public class EventPartMetadata
    {
        public string Name { get; }
        public string HelpText { get; }
        public bool IsOptional { get; }
        public Func<object, object> Getter { get; }
        public Type PropertyType { get; }
        public Expression Parser { get; }

        public EventPartMetadata(PropertyInfo property, ParameterExpression partsParameter, int index)
        {
            var attribute = (EventPartAttribute)property.GetCustomAttribute(typeof(EventPartAttribute), false);
            Name = attribute.Name;
            HelpText = attribute.HelpText;
            IsOptional = attribute.IsOptional;
            PropertyType = property.PropertyType;
            var getMethod = property.GetMethod;

            var instance = Expression.Parameter(typeof(object), "x");
            Getter = (Func<object, object>)
                Expression.Lambda(
                    Expression.Convert(
                        Expression.Call(Expression.Convert(instance, property.DeclaringType), getMethod),
                        typeof(object)),
                    instance).Compile();

            var part = Expression.ArrayIndex(partsParameter, Expression.Constant(index + 1));
            if (PropertyType == typeof(string))
            {
                Parser = part;
            }
            else if (PropertyType == typeof(bool))
            {
                var method = typeof(bool).GetMethod("Parse", new[] { typeof(string) });
                Parser = Expression.Call(method, part);
            }
            else if (PropertyType == typeof(int))
            {
                var method = typeof(int).GetMethod("Parse", new[] { typeof(string) });
                Parser = Expression.Call(method, part);
            }
            else if (PropertyType == typeof(float))
            {
                var method = typeof(float).GetMethod("Parse", new[] { typeof(string) });
                Parser = Expression.Call(method, part);
            }
            else if (PropertyType == typeof(int?))
            {
                var method = GetType().GetMethod("ParseNullableInt", BindingFlags.NonPublic | BindingFlags.Static);
                Parser = Expression.Call(method, part);
            }
            else if (PropertyType == typeof(bool?))
            {
                var method = GetType().GetMethod("ParseNullableBool", BindingFlags.NonPublic | BindingFlags.Static);
                Parser = Expression.Call(method, part);
            }
            else if (PropertyType.IsEnum)
            {
                var method = GetType().GetMethod("ParseEnum", BindingFlags.NonPublic | BindingFlags.Static);
                var generic = method.MakeGenericMethod(PropertyType);
                Parser = Expression.Call(generic, part);
            }
            else throw new NotImplementedException();
        }

        static int? ParseNullableInt(string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;
            return int.Parse(s);
        }

        static bool? ParseNullableBool(string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;
            return bool.Parse(s);
        }

        static T ParseEnum<T>(string s) => (T)Enum.Parse(typeof(T), s, true);
    }
}