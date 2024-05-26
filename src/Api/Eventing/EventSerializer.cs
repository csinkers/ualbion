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

    readonly object _syncRoot = new();
    readonly Dictionary<Type, EventMetadata> _serializers = new();
    readonly Dictionary<string, EventMetadata> _events = new();

    static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
    {
        Type[] types;
        try { types = assembly.GetTypes(); }
        catch (ReflectionTypeLoadException e) { types = e.Types; }
        return types.Where(x => x != null);
    }

    public IEnumerable<Type> AllEventTypes => _serializers.Keys;

    public void AddEventsFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var types =
            from type in GetTypesFromAssembly(assembly) 
            where (typeof(Event).IsAssignableFrom(type) || typeof(EventRecord).IsAssignableFrom(type)) && !type.IsAbstract 
            select type;

        lock (_syncRoot)
        {
            foreach (var type in types)
            {
                var eventAttribute = (EventAttribute) type.GetCustomAttribute(typeof(EventAttribute), false);
                if (eventAttribute == null) 
                    continue;

                var metadata = new EventMetadata(type);
                _serializers[type] = metadata;
                _events[metadata.Name.ToUpperInvariant()] = metadata;

                if (metadata.Aliases == null) 
                    continue;

                foreach (var alias in metadata.Aliases)
                    _events[alias.ToUpperInvariant()] = metadata;
            }
        }
    }

    public string ToString(IEvent e)
    {
        ArgumentNullException.ThrowIfNull(e);
        return _serializers.TryGetValue(e.GetType(), out var metadata)
            ? metadata.Serialize(e, false)
            : e.GetType().Name;
    }

    public void Format(IScriptBuilder builder, IEvent e)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(e);

        if (_serializers.TryGetValue(e.GetType(), out var metadata))
            metadata.Serialize(builder, e);
        else
            builder.Add(ScriptPartType.EventName, e.GetType().Name);
    }

    public IEnumerable<EventMetadata> GetEventMetadata() => _events.Values.OrderBy(x => x.Name);

    public IEvent Parse(string s, out string error)
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
        {
            error = "Null string";
            return null;
        }

        string[] parts = Split(s).ToArray();
        if (parts.Length == 0)
        {
            error = "No parts";
            return null;
        }

        if (!_events.TryGetValue(parts[0].ToUpperInvariant(), out var metadata))
        {
            error = $"Could not find event \"{parts[0]}\"";
            return null;
        }

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
            var result = metadata.Parser(parts);
            error = null;
            return result;
        }
        catch (Exception ex)
        {
            error = $"Failed to parse \"{s}\" as a {metadata.Name} ({metadata.Type}): {ex.Message}";
            ApiUtil.Assert(error);
            return null;
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }
}
