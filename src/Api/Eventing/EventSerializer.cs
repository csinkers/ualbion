using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UAlbion.Api.Eventing;

public class EventSerializer
{
    public static EventSerializer Instance { get; } = new();
    EventSerializer() {}

    readonly object SyncRoot = new();
    readonly IDictionary<Type, EventMetadata> Serializers = new Dictionary<Type, EventMetadata>();
    readonly IDictionary<string, EventMetadata> Events = new Dictionary<string, EventMetadata>();

    static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
    {
        Type[] types;
        try { types = assembly.GetTypes(); }
        catch (ReflectionTypeLoadException e) { types = e.Types; }
        return types.Where(x => x != null);
    }

    public IEnumerable<Type> AllEventTypes => Serializers.Keys;

    public void AddEventsFromAssembly(Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));

        var types =
            from type in GetTypesFromAssembly(assembly) 
            where (typeof(Event).IsAssignableFrom(type) || typeof(EventRecord).IsAssignableFrom(type)) && !type.IsAbstract 
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

    public string ToString(IEvent e)
    {
        if (e == null) throw new ArgumentNullException(nameof(e));
        return Serializers.TryGetValue(e.GetType(), out var metadata)
            ? metadata.Serialize(e, false)
            : e.GetType().Name;
    }

    public void Format(IScriptBuilder builder, IEvent e)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (e == null) throw new ArgumentNullException(nameof(e));

        if (Serializers.TryGetValue(e.GetType(), out var metadata))
            metadata.Serialize(builder, e);
        else
            builder.Add(ScriptPartType.EventName, e.GetType().Name);
    }

    public IEnumerable<EventMetadata> GetEventMetadata() => Events.Values.OrderBy(x => x.Name);

    public IEvent Parse(string s)
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
            var builder = new UnformattedScriptBuilder(false);
            var newParts = new string[metadata.Parts.Count + 1];
            for (int i = 0; i < newParts.Length; i++)
            {
                if (i < parts.Length)
                    newParts[i] = parts[i];
                else
                {
                    builder.Clear();
                    var part = metadata.Parts[i - 1];
                    EventMetadata.SerializePart(builder, part, part.Default);
                    newParts[i] = builder.Build();
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