using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UAlbion.Api
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Don't care about VB")]
    public abstract class Event : IEvent // Contains no fields, only helper methods for reflection-based parsing and serialization.
    {
        static IEnumerable<Assembly> EventAssemblies => 
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly.FullName.Contains("Albion"));

        static IEnumerable<Type> TypesFromAssembly(Assembly assembly)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }

            return types.Where(x => x != null);
        }

        public static IEnumerable<Type> AllEventTypes =>
            from assembly in EventAssemblies 
            from type in TypesFromAssembly(assembly) 
            where typeof(Event).IsAssignableFrom(type) && !type.IsAbstract 
            select type;

        static IEnumerable<Type> AllSerializableEventTypes
        {
            get
            {
                PerfTracker.StartupEvent("Building event types");
                foreach (var type in AllEventTypes)
                {
                    var eventAttribute = (EventAttribute)type.GetCustomAttribute(
                        typeof(EventAttribute),
                        false);

                    if (eventAttribute != null)
                        yield return type;
                }
                PerfTracker.StartupEvent("Built event types");
            }
        }

        static readonly IDictionary<Type, EventMetadata> Serializers =
            AllSerializableEventTypes.ToDictionary(x => x, x => new EventMetadata(x));

        static readonly IDictionary<string, EventMetadata> Events =
            Serializers
            .SelectMany(x =>
                new[] { x.Value.Name }.Concat(x.Value.Aliases)
                .Select(y => new { Name = y, Meta = x.Value }))
            .ToDictionary(x => x.Name.ToUpperInvariant(), x => x.Meta);

        public override string ToString()
        {
            if (Serializers.TryGetValue(GetType(), out var metadata))
                return metadata.Serialize(this, false);
            return GetType().Name;
        }

        public string ToStringNumeric()
        {
            if (Serializers.TryGetValue(GetType(), out var metadata))
                return metadata.Serialize(this, true);
            return GetType().Name;
        }

        public static IEnumerable<EventMetadata> GetEventMetadata() =>
            Events.Values.OrderBy(x => x.Name);

        public static Event Parse(string s)
        {
            static IEnumerable<string> Split(string input)
            {
                var sb = new StringBuilder();
                bool inString = false;
                bool inEscape = false;
                foreach (char c in input)
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
                                if (sb.Length > 0)
                                    yield return sb.ToString();
                                sb.Length = 0;
                            }

                            break;

                        case 't':
                            if(inEscape)
                            {
                                sb.Append('\t');
                                inEscape = false;
                            }
                            else sb.Append(c);
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

            if (s == null)
                return null;

            string[] parts = Split(s).ToArray();
            if (parts.Length == 0)
                return null;

            if (!Events.TryGetValue(parts[0].ToUpperInvariant(), out var metadata))
                return null;

            if (parts.Length < metadata.Parts.Count + 1)
            {
                var newParts = new string[metadata.Parts.Count + 1];
                for (int i = 0; i < newParts.Length; i++)
                {
                    if (i < parts.Length)
                        newParts[i] = parts[i];
                    else
                        newParts[i] = metadata.Parts[i - 1].Default;
                }
                parts = newParts;
            }

            try
            {
                return metadata.Parser(parts);
            }
            catch (FormatException ex)
            {
                ApiUtil.Assert($"Failed to parse \"{s}\" as a {metadata.Name} ({metadata.Type}): {ex.Message}");
                return null;
            }
            catch (NullReferenceException ex)
            {
                ApiUtil.Assert($"Failed to parse \"{s}\" as a {metadata.Name} ({metadata.Type}): {ex.Message}");
                return null;
            }
        }
    }
}
