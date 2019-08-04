using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace UAlbion.Core
{
    public interface IEvent { }

    public abstract class Event : IEvent
    {
        class EventPartMetadata
        {
            public string Name { get; }
            public Func<object, object> Getter { get; }
            public Type PropertyType { get; }

            public Expression Parser { get; }

            public EventPartMetadata(PropertyInfo property, ParameterExpression partsParameter, int index)
            {
                var attribute = (EventPartAttribute)property.GetCustomAttribute(typeof(EventPartAttribute), false);
                Name = attribute.Name;
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
                else throw new NotImplementedException();
            }
        }

        class EventMetadata
        {
            public string Name { get; }
            public Type Type { get; }
            public EventPartMetadata[] Parts { get; }
            public Func<string[], Event> Parser { get; }

            public EventMetadata(Type type)
            {
                var eventAttribute = (EventAttribute)type.GetCustomAttribute(typeof(EventAttribute), false);
                var properties = type.GetProperties();
                Type = type;
                Name = eventAttribute.Name;
                var partsParameter = Expression.Parameter(typeof(string[]), "parts");
                Parts = properties
                    .Where(x => x.GetCustomAttribute(typeof(EventPartAttribute)) != null)
                    .Select((x, i) => new EventPartMetadata(x, partsParameter, i))
                    .ToArray();

                Parser = BuildParser(partsParameter);
            }

            public string Serialize(object instance)
            {
                var sb = new StringBuilder();
                sb.Append(Name);
                for (int i = 0; i < Parts.Length; i++)
                {
                    var part = Parts[i];
                    sb.Append(' ');
                    if (part.PropertyType == typeof(string))
                    {
                        sb.Append('"');
                        sb.Append(((string)part.Getter(instance)).Replace("\"", "\\\""));
                        sb.Append('"');
                    }
                    else
                    {
                        sb.Append(Parts[i].Getter(instance));
                    }
                }

                return sb.ToString();
            }

            Func<string[], Event> BuildParser(ParameterExpression partsParameter)
            {
                var constructor = Type.GetConstructors().Single();
                var parameters = constructor.GetParameters();
                Debug.Assert(parameters.Length == Parts.Length);

                return (Func<string[], Event>)Expression.Lambda(
                    Expression.Convert(
                        Expression.New(constructor, Parts.Select(x => x.Parser)), typeof(Event)),
                    partsParameter).Compile();
            }
        }

        static IEnumerable<Type> GetAllEventTypes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types; try { types = assembly.GetTypes(); } catch (ReflectionTypeLoadException e) { types = e.Types; }
                foreach(var type in types.Where(x => x != null))
                {
                    if (typeof(Event).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        var eventAttribute = (EventAttribute)type.GetCustomAttribute(typeof(EventAttribute), false);
                        if (eventAttribute != null)
                            yield return type;
                    }
                }
            }
        }

        static readonly IDictionary<Type, EventMetadata> Serializers = GetAllEventTypes().ToDictionary(x => x, x => new EventMetadata(x));
        static readonly IDictionary<string, EventMetadata> Events = Serializers.ToDictionary(x => x.Value.Name, x => x.Value);

        public override string ToString()
        {
            var metadata = Serializers[GetType()];
            return metadata.Serialize(this);
        }

        public static Event Parse(string s)
        {
            IEnumerable<string> Split()
            {
                var sb = new StringBuilder();
                bool inString = false;
                bool inEscape = false;
                foreach (char c in s)
                {
                    switch (c)
                    {
                        case ' ':
                            if (inString)
                            {
                                sb.Append(' ');
                            }
                            else
                            {
                                if(sb.Length > 0)
                                    yield return sb.ToString();
                                sb.Length = 0;
                            }

                            break;

                        case '"':
                            if (inEscape)
                            {
                                sb.Append('"');
                                inEscape = false;
                            }
                            else inString = !inString;
                            break;

                        case '\\':
                            if (inString)
                            {
                                if (inEscape)
                                {
                                    sb.Append('\\');
                                    inEscape = false;
                                }
                                else inEscape = true;
                            }
                            else sb.Append('\\');
                            break;
                        default:
                            sb.Append(c);
                            break;
                    }
                }

                if (sb.Length > 0)
                    yield return sb.ToString();
            }

            var parts = Split().ToArray();
            if (parts.Length == 0)
                return null;

            var metadata = Events[parts[0]];
            return metadata.Parser(parts);
        }
    }
}