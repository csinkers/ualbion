using System;
using System.Linq.Expressions;
using System.Reflection;

namespace UAlbion.Api
{
    public class EventPartMetadata
    {
        static readonly EventPartParsers Parsers = new EventPartParsers();

        public string Name { get; }
        public string HelpText { get; }
        public bool IsOptional { get; }
        public Func<object, object> Getter { get; }
        public Type PropertyType { get; }
        public Expression Parser { get; }

        public EventPartMetadata(PropertyInfo property, ParameterExpression partsParameter, int index)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            var declaringType = property.DeclaringType;
            if (declaringType == null)
                throw new InvalidOperationException("Property must have a declaring type");

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
                        Expression.Call(Expression.Convert(instance, declaringType), getMethod),
                        typeof(object)),
                    instance).Compile();

            var part = Expression.ArrayIndex(partsParameter, Expression.Constant(index + 1));
            if (PropertyType == typeof(string))
            {
                Parser = part;
            }
            else
            {
                var parser = Parsers.GetParser(PropertyType);
                if (parser != null)
                {
                    Parser = Expression.Call(parser, part);
                }
                else throw new NotImplementedException($"The property {declaringType.Name}.{Name} of type {PropertyType.Name} is not handled.");
            }
        }
    }
}
