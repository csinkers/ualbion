using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace UAlbion.Api
{
    public class EventMetadata
    {
        public string Name { get; }
        public string HelpText { get; }
        public ReadOnlyCollection<string> Aliases { get; }
        public ReadOnlyCollection<EventPartMetadata> Parts { get; }
        public Func<string[], Event> Parser { get; }
        public Type Type { get; }

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
            var sb = new StringBuilder();
            sb.Append(Name);

            int skipCount = 0;
            for (int i = Parts.Count - 1; i >= 0; i--)
            {
                var part = Parts[i];
                if (!part.IsOptional || part.Default == null)
                    break;

                var value = part.Getter(instance);
                if (Equals(value, part.Default))
                    skipCount++;
                else break;
            }

            for (var index = 0; index < Parts.Count - skipCount; index++)
            {
                var part = Parts[index];
                sb.Append(' ');
                var value = part.Getter(instance);
                SerializePart(sb, part, value, useNumericIds);
            }

            return sb.ToString().TrimEnd();
        }

        public static void SerializePart(StringBuilder sb, EventPartMetadata part, object value, bool useNumericIds)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            if (part == null) throw new ArgumentNullException(nameof(part));
            switch (value)
            {
                case string s when part.PropertyType == typeof(string):
                {
                    sb.Append('"');
                    foreach (var c in s)
                    {
                        switch (c)
                        {
                            case '\\': sb.Append("\\\\"); break;
                            case '"': sb.Append("\\\""); break;
                            case '\t': sb.Append("\\t"); break;
                            default: sb.Append(c); break;
                        }
                    }

                    sb.Append('"');
                    break;
                }

                case IAssetId id:
                    sb.Append(useNumericIds ? id.ToStringNumeric() : id.ToString());
                    break;

                case Enum enumValue when useNumericIds:
                {
                    object numeric = Convert.ChangeType(enumValue, enumValue.GetTypeCode(), CultureInfo.InvariantCulture);
                    sb.Append(numeric);
                    break;
                }

                case Enum enumValue: sb.Append(enumValue); break;
                default: sb.Append(value); break;
            }
        }

        Func<string[], Event> BuildParser(ParameterExpression partsParameter)
        {
            var constructor = Type.GetConstructors().Single();
            var parameters = constructor.GetParameters();
            ApiUtil.Assert(parameters.Length == Parts.Count,
                $"When building parser for {Type}, the public constructor had {parameters.Length} parameters but {Parts.Count} were expected.");

            return (Func<string[], Event>)Expression.Lambda(
                Expression.Convert(
                    Expression.New(constructor, Parts.Select(x => x.Parser)), typeof(Event)),
                partsParameter).Compile();
        }
    }
}
