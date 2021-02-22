using System;
using System.Collections.ObjectModel;
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

            Parser ??= BuildParser(partsParameter);
        }

        public string Serialize(object instance)
        {
            var sb = new StringBuilder();
            sb.Append(Name);
            foreach (var part in Parts)
            {
                sb.Append(' ');
                var value = part.Getter(instance);
                if (part.PropertyType == typeof(string) && value is string s)
                {
                    sb.Append('"');
                    for (int i = 0; i < s.Length; i++)
                    {
                        char c = s[i];
                        switch (c)
                        {
                            case '\\': sb.Append("\\\\"); break;
                            case '"': sb.Append("\\\""); break;
                            case '\t': sb.Append("\\t"); break;
                            default: sb.Append(c); break;
                        }
                    }
                    sb.Append('"');
                }
                else sb.Append(value);
            }

            return sb.ToString().TrimEnd();
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
