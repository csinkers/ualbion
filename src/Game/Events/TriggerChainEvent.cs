using System;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Events;

public class TriggerChainEvent : IAsyncEvent
{
    public TriggerChainEvent(AssetId chainSource, ushort chain, IEventNode node, EventSource source)
    {
        ChainSource = chainSource;
        Chain = chain;
        Node = node ?? throw new ArgumentNullException(nameof(node));
        Source = source ?? throw new ArgumentNullException(nameof(source));
    }

    public string ToStringNumeric() => ToString();
    public override string ToString() =>
        $"Triggering chain {ChainSource}:{Chain} due to {Source} (first event {Node})";


    public AssetId ChainSource { get; }
    public ushort Chain { get; }
    public IEventNode Node { get; }
    public EventSource Source { get; }
}