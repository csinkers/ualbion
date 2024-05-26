using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UAlbion.Api.Eventing;

public class EventMetadata
{
    public string Name { get; }
    public string HelpText { get; }
    public ReadOnlyCollection<string> Aliases { get; }
    public ReadOnlyCollection<EventPartMetadata> Parts { get; }
    public Func<string[], IEvent> Parser { get; }
    public Type Type { get; }
    public override string ToString() => $"{Name} {HelpText}";

    public EventMetadata(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        var eventAttribute = (EventAttribute)type.GetCustomAttribute(typeof(EventAttribute), false);
        if (eventAttribute == null)
            throw new InvalidOperationException($"Event type {type} is missing an Event attribute");

        var properties = type.GetProperties();
        Type = type;
        Name = eventAttribute.Name;
        HelpText = eventAttribute.HelpText;
        Aliases = new ReadOnlyCollection<string>(eventAttribute.Aliases ?? Array.Empty<string>());

        var partsParameter = Expression.Parameter(typeof(string[]), "parts");
        // TODO: Order passed in to EventPartMetadata needs to be based on constructor parameters.
        Parts = new ReadOnlyCollection<EventPartMetadata>(properties
            .Where(x => x.GetCustomAttribute(typeof(EventPartAttribute)) != null)
            .Select((x, i) => new EventPartMetadata(x, partsParameter, i))
            .ToArray());

        var parser = type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public);
        if (parser != null && parser.ReturnParameter?.ParameterType == type)
        {
            var parameters = parser.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string[]))
            {
                Parser = (Func<string[], IEvent>)Expression.Lambda(
                    Expression.Convert(
                        Expression.Call(parser, partsParameter), typeof(IEvent)),
                    partsParameter).Compile();
            }
        }

        try { Parser ??= BuildParser(partsParameter); }
        catch (ArgumentException e)
        {
            throw new ArgumentException(e.Message + " when constructing parser for " + type.Name, e);
        }
    }

    public string Serialize(object instance, bool useNumericIds)
    {
        var builder = new UnformattedScriptBuilder(useNumericIds);
        Serialize(builder, instance);
        return builder.Build();
    }

    public void Serialize(IScriptBuilder builder, object instance)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Add(ScriptPartType.EventName, Name);

        int skipCount = 0;
        for (int i = Parts.Count - 1; i >= 0; i--)
        {
            var part = Parts[i];
            if (!part.IsOptional)
                break;

            var value = part.Getter(instance);
            if (Equals(value, part.Default))
                skipCount++;
            else break;
        }

        for (var index = 0; index < Parts.Count - skipCount; index++)
        {
            var part = Parts[index];
            builder.Append(' ');
            var value = part.Getter(instance);
            SerializePart(builder, part, value);
        }
    }

    public static void SerializePart(IScriptBuilder builder, EventPartMetadata part, object value)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(part);

        switch (value)
        {
            case string s when part.PropertyType == typeof(string):
                {
                    builder.Add(ScriptPartType.StringConstant, '\"');
                    foreach (var c in s)
                    {
                        switch (c)
                        {
                            case '\\': builder.Add(ScriptPartType.StringConstant, "\\\\"); break;
                            case '"': builder.Add(ScriptPartType.StringConstant, "\\\""); break;
                            case '\t': builder.Add(ScriptPartType.StringConstant, "\\t"); break;
                            default: builder.Add(ScriptPartType.StringConstant, c); break;
                        }
                    }

                    builder.Add(ScriptPartType.StringConstant, '"');
                    break;
                }

            case IAssetId id:
                if (builder.UseNumericIds)
                    builder.Add(ScriptPartType.Number, id.ToStringNumeric());
                else
                    builder.Add(ScriptPartType.Identifier, id.ToString());

                break;

            case Enum enumValue when builder.UseNumericIds:
                {
                    object numeric = Convert.ChangeType(enumValue, enumValue.GetTypeCode(), CultureInfo.InvariantCulture);
                    builder.Add(ScriptPartType.Number, numeric.ToString());
                    break;
                }

            case Enum enumValue:
                builder.Add(ScriptPartType.Identifier, enumValue.ToString().Replace(", ", "|", StringComparison.InvariantCulture));
                break;

            default: // ints, bytes etc
                builder.Append(value); 
                break;
        }
    }

    Func<string[], IEvent> BuildParser(ParameterExpression partsParameter)
    {
        var constructor = GetConstructor();
        var parameters = constructor.GetParameters();
        if (parameters.Length != Parts.Count)
        {
            throw new FormatException(
                $"When building parser for {Type}, the public constructor had {parameters.Length} parameters but {Parts.Count} were expected. " +
                "This may be because the property corresponding to the constructor parameter does not have an EventPart attribute.");
        }

        return (Func<string[], IEvent>)Expression.Lambda(
            Expression.Convert(
                Expression.New(constructor, Parts.Select(x => x.Parser)), typeof(IEvent)),
            partsParameter).Compile();
    }

    ConstructorInfo GetConstructor()
    {
        var constructors = Type.GetConstructors();
        ConstructorInfo constructor = null;

        if (constructors.Length == 1)
            return constructors[0];

        foreach (var x in constructors)
        {
            if (x.GetCustomAttribute<EventConstructorAttribute>() == null)
                continue;

            if (constructor != null)
                throw new FormatException($"\"{Type}\" has multiple constructors with the {nameof(EventConstructorAttribute)}!");

            constructor = x;
        }

        if (constructor == null)
            throw new FormatException($"\"{Type}\" has multiple constructors, but none of them are annotated with an {nameof(EventConstructorAttribute)} to indicate which should be used for constructing parsed events");

        return constructor;
    }
}
