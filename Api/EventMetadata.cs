using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace UAlbion.Api
{
    public class EventMetadata
    {
        public string Name { get; }
        public string[] Aliases { get; }
        public string HelpText { get; }
        public EventPartMetadata[] Parts { get; }
        public Func<string[], Event> Parser { get; }
        public Type Type { get; }

        public EventMetadata(Type type)
        {
            var eventAttribute = (EventAttribute)type.GetCustomAttribute(typeof(EventAttribute), false);
            var properties = type.GetProperties();
            Type = type;
            Name = eventAttribute.Name;
            HelpText = eventAttribute.HelpText;
            Aliases = eventAttribute.Aliases ?? new string[0];

            var partsParameter = Expression.Parameter(typeof(string[]), "parts");
            Parts = properties
                .Where(x => x.GetCustomAttribute(typeof(EventPartAttribute)) != null)
                .Select((x, i) => new EventPartMetadata(x, partsParameter, i))
                .ToArray();

            var parser = type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public);
            if(parser != null && parser.ReturnParameter?.ParameterType == type)
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
                if (part.PropertyType == typeof(string))
                {
                    var value = (string)part.Getter(instance);
                    if (value != null)
                    {
                        sb.Append('"');
                        sb.Append(value
                            .Replace("\\", "\\\\")
                            .Replace("\"", "\\\"")
                            .Replace("\t", "\\t")
                        );
                        sb.Append('"');
                    }
                }
                else
                {
                    sb.Append(part.Getter(instance));
                }
            }

            return sb.ToString().TrimEnd();
        }

        Func<string[], Event> BuildParser(ParameterExpression partsParameter)
        {
            var constructor = Type.GetConstructors().Single();
            var parameters = constructor.GetParameters();
            ApiUtil.Assert(parameters.Length == Parts.Length,
                $"When building parser for {Type}, the public constructor had {parameters.Length} parameters but {Parts.Length} were expected.");

            return (Func<string[], Event>)Expression.Lambda(
                Expression.Convert(
                    Expression.New(constructor, Parts.Select(x => x.Parser)), typeof(Event)),
                partsParameter).Compile();
        }
    }
}
