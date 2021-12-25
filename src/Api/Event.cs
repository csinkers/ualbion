using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UAlbion.Api
{
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Don't care about VB")]
    public abstract class Event : IEvent // Contains no fields, only helper methods for reflection-based parsing and serialization.
    {
        static readonly object SyncRoot = new();
        static readonly IDictionary<Type, EventMetadata> Serializers = new Dictionary<Type, EventMetadata>();
        static readonly IDictionary<string, EventMetadata> Events = new Dictionary<string, EventMetadata>();

        static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
        {
            Type[] types;
            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException e) { types = e.Types; }
            return types.Where(x => x != null);
        }

        public static IEnumerable<Type> AllEventTypes => Serializers.Keys;

        public static void AddEventsFromAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            var types =
                from type in GetTypesFromAssembly(assembly) 
                where typeof(Event).IsAssignableFrom(type) && !type.IsAbstract 
                select type;

            lock (SyncRoot)
            {
                foreach (var type in types)
                {
                    var eventAttribute = (EventAttribute) type.GetCustomAttribute(typeof(EventAttribute), false);
                    if (eventAttribute == null) 
                        continue;

                    var metadata = new EventMetadata(type);
                    Serializers[type] = metadata;
                    Events[metadata.Name.ToUpperInvariant()] = metadata;

                    if (metadata.Aliases == null) 
                        continue;

                    foreach (var alias in metadata.Aliases)
                        Events[alias.ToUpperInvariant()] = metadata;
                }
            }
        }

        public override string ToString() =>
            Serializers.TryGetValue(GetType(), out var metadata)
                ? metadata.Serialize(this, false)
                : GetType().Name;

        public string ToStringNumeric() =>
            Serializers.TryGetValue(GetType(), out var metadata)
                ? metadata.Serialize(this, true)
                : GetType().Name;

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
                    {
                        var sb = new StringBuilder();
                        var part = metadata.Parts[i - 1];
                        EventMetadata.SerializePart(sb, part, part.Default, false);
                        newParts[i] = sb.ToString();
                    }
                }
                parts = newParts;
            }

#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                return metadata.Parser(parts);
            }
            catch (Exception ex)
            {
                ApiUtil.Assert($"Failed to parse \"{s}\" as a {metadata.Name} ({metadata.Type}): {ex.Message}");
                return null;
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }
}
