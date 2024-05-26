using System;
using System.Linq.Expressions;
using System.Reflection;

namespace UAlbion.Api.Eventing;

public class EventPartMetadata
{
    static readonly EventPartParsers Parsers = new();
    public static void AddParser(Type type, MethodInfo method) => Parsers.AddParser(type, method);

    public string Name { get; }
    public string HelpText { get; }
    public bool IsOptional { get; }
    public object Default { get; }
    public Func<object, object> Getter { get; }
    public Type PropertyType { get; }
    public Expression Parser { get; }
    public override string ToString() => $"{Name} : {PropertyType} (def: {Default}) {HelpText}";

    public EventPartMetadata(PropertyInfo property, ParameterExpression partsParameter, int index)
    {
        ArgumentNullException.ThrowIfNull(property);
        var declaringType = property.DeclaringType;
        if (declaringType == null)
            throw new InvalidOperationException("Property must have a declaring type");

        var attribute = (EventPartAttribute)property.GetCustomAttribute(typeof(EventPartAttribute), false);
        if (attribute == null)
            throw new InvalidOperationException("Property must have an event part attribute");
        Name = attribute.Name;
        HelpText = attribute.HelpText;
        IsOptional = attribute.IsOptional;
        Default = attribute.DefaultValue;
        PropertyType = property.PropertyType;
        Getter = BuildGetter(property, declaringType);

        var part = Expression.ArrayIndex(partsParameter, Expression.Constant(index + 1));
        Parser = Parsers.GetParser(PropertyType, part);
        if (Parser == null)
            throw new NotImplementedException($"The property {declaringType.Name}.{Name} of type {PropertyType.Name} is not handled.");

        if (Default != null && Default.GetType() != PropertyType)
        {
            if (Default is string s && PropertyType.IsAssignableTo(typeof(IAssetId)))
            {
                // A filthy hack. Need to properly auto-generate all this code at compile time w/ a Roslyn code generator.
                var lambda = (Func<string[], object>)Expression.Lambda(Expression.Convert(Parser, typeof(object)), partsParameter).Compile();
                var dummyArray = new string[index + 2];
                dummyArray[index + 1] = s;
                Default = lambda(dummyArray);
            }
            else throw new InvalidOperationException($"Property {Name} of {declaringType} must have a default value of type {PropertyType}");
        }
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

        return getter;
    }
}
