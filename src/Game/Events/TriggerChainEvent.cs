using System;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Events;

public class TriggerChainEvent : IEvent
{
    public TriggerChainEvent(IEventSet eventSet, ushort entryPoint, EventSource source)
    {
        EventSet = eventSet ?? throw new ArgumentNullException(nameof(eventSet));
        Source = source ?? throw new ArgumentNullException(nameof(source));
        EntryPoint = entryPoint;
        if (entryPoint >= eventSet.Events.Count)
            throw new InvalidOperationException($"Tried to trigger chain with invalid entry point {entryPoint} (max event index {eventSet.Events.Count - 1})");
    }

    public IEventSet EventSet { get; }
    public ushort EntryPoint { get; }
    public EventSource Source { get; }

    public override string ToString()
    {
        var builder = new UnformattedScriptBuilder(false);
        Format(builder);
        return builder.Build();
    }

    public void Format(IScriptBuilder builder)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        builder.Append("Triggering chain ");
        builder.Append(EventSet.Id);
        builder.Append(":");
        builder.Append(EventSet.GetChainForEvent(EntryPoint));
        builder.Append(" due to ");
        builder.Append(Source);
        builder.Append(" (first event ");
        builder.Append(EventSet.Events[EntryPoint]);
        builder.Append(")");
    }
}