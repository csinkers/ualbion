using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;
using UAlbion.Scripting;

namespace UAlbion.Base.Tests;

public static class LayoutTestUtil
{
    public delegate (List<EventNode> events, List<ushort> chains, List<ushort> extra) LayoutExtractor<in T>(T value);
    public delegate T LayoutConstructor<T>(T value, EventLayout layout);

    public static Formats.Assets.EventSet CanonicalizeEventSet(Formats.Assets.EventSet set) => Canonicalize(set, ExtractSetLayout, ConstructSet);
    public static T CanonicalizeMap<T>(T map) where T : BaseMapData => Canonicalize(map, ExtractMapLayout, ConstructMap);

    public static EventLayout BuildLayout<T>(T value, LayoutExtractor<T> extractor)
    {
        var (events, chains, extra) = extractor(value);
        var regions = Decompiler.BuildEventRegions(events, chains, extra);
        return EventLayout.Build(regions);
    }

    public static T Canonicalize<T>(T value, LayoutExtractor<T> extractor, LayoutConstructor<T> constructor) => constructor(value, BuildLayout(value, extractor)); 
    public static Formats.Assets.EventSet ConstructSet(Formats.Assets.EventSet x, EventLayout layout) => new(x.Id, layout.Events, layout.Chains);
    public static (List<EventNode> events, List<ushort> chains, List<ushort> extra) ExtractSetLayout(Formats.Assets.EventSet x) => (x.Events, x.Chains, null);

    public static T ConstructMap<T>(T x, EventLayout layout) where T : BaseMapData
    {
        x.RemapChains(layout.Events, layout.Chains);
        return x;
    }

    public static (List<EventNode> events, List<ushort> chains, List<ushort> extra) ExtractMapLayout(IMapData map)
    {
        var entryPoints = map.Npcs.Select(x => x.EventIndex).ToHashSet();
        entryPoints.UnionWith(map.UniqueZoneNodeIds);
        entryPoints.ExceptWith(map.Chains);
        entryPoints.Remove(0xffff);

        return (map.Events, map.Chains, entryPoints.ToList());
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

        AssetId textSource = assetId.Type switch
        {
            AssetType.EventSet => ((EventSetId)assetId).ToEventText(),
            AssetType.Map => ((MapId)assetId).ToMapText(),
            _ => throw new ArgumentOutOfRangeException(nameof(assetId), $"Unexpected asset {assetId}, expected event set or map")
        };

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
            var expectedBytes = BytesForEventChain(expected.Events, expected.Chains[i], assetId, textSource);
            var actualBytes = BytesForEventChain(actual.Events, actual.Chains[i], assetId, textSource);
            if (!expectedBytes.SequenceEqual(actualBytes))
                sb.AppendLine($"Chain{i} mismatch on {assetId}");
        }

        for (int i = 0; i < actual.ExtraEntryPoints.Count; i++)
        {
            var expectedBytes = BytesForEventChain(expected.Events, expected.ExtraEntryPoints[i], assetId, textSource);
            var actualBytes = BytesForEventChain(actual.Events, actual.ExtraEntryPoints[i], assetId, textSource);
            if (!expectedBytes.SequenceEqual(actualBytes))
                sb.AppendLine($"Extra entry {i} ({actual.ExtraEntryPoints[i]}) mismatch on {assetId}");
        }

        message = sb.ToString();
        return sb.Length == 0;
    }

    static byte[] BytesForEventChain(IList<EventNode> events, int entryPoint, AssetId assetId, AssetId textSource) =>
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

                MapEvent.SerdesEvent(mapEvent, s, assetId, textSource, AssetMapping.Global);
            }
        });
}