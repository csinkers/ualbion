using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;
using UAlbion.Scripting;

namespace UAlbion.Base.Tests;

public static class LayoutTestUtil
{
    public delegate T LayoutConstructor<T>(T value, EventLayout layout) where T : IEventSet;

    public static Formats.Assets.EventSet CanonicalizeEventSet(Formats.Assets.EventSet set) => Canonicalize(set, ConstructSet);
    public static T CanonicalizeMap<T>(T map) where T : BaseMapData => Canonicalize(map, ConstructMap);

    public static EventLayout BuildLayout(IEventSet set)
    {
        var regions = Decompiler.BuildEventRegions(set.Events, set.Chains, set.ExtraEntryPoints);
        return EventLayout.Build(regions);
    }

    public static T Canonicalize<T>(T value, LayoutConstructor<T> constructor) where T : IEventSet
        => constructor(value, BuildLayout(value)); 
    public static Formats.Assets.EventSet ConstructSet(Formats.Assets.EventSet x, EventLayout layout)
        => new(x.Id, layout.Events, layout.Chains);

    public static T ConstructMap<T>(T x, EventLayout layout) where T : BaseMapData
    {
        x.RemapChains(layout.Events, layout.Chains);
        return x;
    }

    static void TrimTrailingEmptyChains(IList<ushort> chains)
    {
        for (int i = chains.Count - 1; i >= 0; i--)
        {
            if (chains[i] != EventNode.UnusedEventId)
                break;
            chains.RemoveAt(i);
        }
    }

    public static bool CompareLayoutBytes(EventLayout actual, EventLayout expected, AssetId assetId, out string message)
    {
        TrimTrailingEmptyChains(actual.Chains);
        TrimTrailingEmptyChains(expected.Chains);

        if (actual.Chains.Count != expected.Chains.Count)
        {
            message = $"Chain count mismatch: expected {expected.Chains.Count}, but found {actual.Chains.Count}";
            return false;
        }

        if (actual.ExtraEntryPoints.Count != expected.ExtraEntryPoints.Count)
        {
            message = $"Extra entry point count mismatch: expected {expected.ExtraEntryPoints.Count}, but found {actual.ExtraEntryPoints.Count}";
            return false;
        }

        var sb = new StringBuilder();
        for (int i = 0; i < actual.Chains.Count; i++)
        {
            ReadOnlyMemory<byte> expectedBytes = BytesForEventChain(expected.Events, expected.Chains[i]);
            ReadOnlyMemory<byte> actualBytes = BytesForEventChain(actual.Events, actual.Chains[i]);

            if (!expectedBytes.Span.SequenceEqual(actualBytes.Span))
                sb.AppendLine($"Chain{i} mismatch on {assetId}");
        }

        for (int i = 0; i < actual.ExtraEntryPoints.Count; i++)
        {
            var expectedBytes = BytesForEventChain(expected.Events, expected.ExtraEntryPoints[i]);
            var actualBytes = BytesForEventChain(actual.Events, actual.ExtraEntryPoints[i]);
            if (!expectedBytes.Span.SequenceEqual(actualBytes.Span))
                sb.AppendLine($"Extra entry {i} ({actual.ExtraEntryPoints[i]}) mismatch on {assetId}");
        }

        message = sb.ToString();
        return sb.Length == 0;
    }

    static ReadOnlyMemory<byte> BytesForEventChain(IList<EventNode> events, int entryPoint) =>
        FormatUtil.SerializeToBytes(s =>
        {
            var visited = new bool[events.Count];
            var stack = new Stack<int>();
            stack.Push(entryPoint);
            while (stack.TryPop(out var index))
            {
                if (index == EventNode.UnusedEventId || visited[index])
                    continue;

                visited[index] = true;
                var node = events[index];

                if (node.Event is not MapEvent mapEvent)
                    throw new FormatException($"Event {node.Event} is not a map event and cannot be serialized to bytes");

                if (node is IBranchNode { NextIfFalse: { } } branch)
                    stack.Push(branch.NextIfFalse.Id);

                if (node.Next != null)
                    stack.Push(node.Next.Id);

                MapEvent.SerdesEvent(mapEvent, s, AssetMapping.Global, MapType.Unknown);
            }
        });
}