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
        public string Default { get; }
        public Func<object, object> Getter { get; }
        public Type PropertyType { get; }
        public Expression Parser { get; }
        public override string ToString() => $"{Name} : {PropertyType} (def: {Default}) {HelpText}";

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
            Default = attribute.Default;
            PropertyType = property.PropertyType;
            Getter = BuildGetter(property, declaringType);

            var part = Expression.ArrayIndex(partsParameter, Expression.Constant(index + 1));
            Parser = Parsers.GetParser(PropertyType, part);
            if (Parser == null)
                throw new NotImplementedException($"The property {declaringType.Name}.{Name} of type {PropertyType.Name} is not handled.");
        }

        static Func<object, object> BuildGetter(PropertyInfo property, Type declaringType)
        {
            var getMethod = property.GetMethod;

            var instance = Expression.Parameter(typeof(object), "x");
            var getter = (Func<object, object>)
                Expression.Lambda(
                    Expression.Convert(
                        Expression.Call(Expression.Convert(instance, declaringType), getMethod),
                        typeof(object)),
                    instance).Compile();

            return ApiUtil.IsFlagsEnum(property.PropertyType)
                ? x => (getter(x)?.ToString() ?? "").Replace(", ", "|")
                : getter;
        }
    }
}
