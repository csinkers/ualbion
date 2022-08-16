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
    public Func<string[], Event> Parser { get; }
    public Type Type { get; }
    public override string ToString() => $"{Name} {HelpText}";

    public EventMetadata(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
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
                Parser = (Func<string[], Event>)Expression.Lambda(
                    Expression.Convert(
                        Expression.Call(parser, partsParameter), typeof(Event)),
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
        if (builder == null) throw new ArgumentNullException(nameof(builder));
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
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (part == null) throw new ArgumentNullException(nameof(part));

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

    Func<string[], Event> BuildParser(ParameterExpression partsParameter)
    {
        var constructor = Type.GetConstructors().Single();
        var parameters = constructor.GetParameters();
        if (parameters.Length != Parts.Count)
        {
            throw new FormatException(
                $"When building parser for {Type}, the public constructor had {parameters.Length} parameters but {Parts.Count} were expected. " +
                "This may be because the property corresponding to the constructor parameter does not have an EventPart attribute.");
        }

        return (Func<string[], Event>)Expression.Lambda(
            Expression.Convert(
                Expression.New(constructor, Parts.Select(x => x.Parser)), typeof(Event)),
            partsParameter).Compile();
    }
}