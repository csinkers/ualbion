using System;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Events;

public class TriggerChainEvent : IAsyncEvent
{
    public TriggerChainEvent(IEventSet eventSet, ushort entryPoint, EventSource source)
    {
        EventSet = eventSet ?? throw new ArgumentNullException(nameof(eventSet));
        Source = source ?? throw new ArgumentNullException(nameof(source));
        EntryPoint = entryPoint;
        if (entryPoint >= eventSet.Events.Count)
            throw new InvalidOperationException($"Tried to trigger chain with invalid entry point {entryPoint} (max event index {eventSet.Events.Count - 1})");
    }

    public string ToStringNumeric() => ToString();
    public override string ToString() =>
        $"Triggering chain {EventSet.Id}:{EventSet.GetChainForEvent(EntryPoint)} due to {Source} (first event {EventSet.Events[EntryPoint]})";

    public IEventSet EventSet { get; }
    public ushort EntryPoint { get; }
    public EventSource Source { get; }
}