using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Scripting;

namespace UAlbion.Base.Tests
{
    public static class LayoutTestUtil
    {
        delegate (List<EventNode> events, List<ushort> chains, List<ushort> extra) LayoutExtractor<in T>(T value);
        delegate T LayoutConstructor<T>(T value, EventLayout layout);

        public static Formats.Assets.EventSet CanonicalizeEventSet(Formats.Assets.EventSet set) => Canonicalize(set, ExtractSetLayout, ConstructSet);
        public static T CanonicalizeMap<T>(T map) where T : BaseMapData => Canonicalize(map, ExtractMapLayout, ConstructMap);
        public static void CompareMap(BaseMapData map1, BaseMapData map2) => Compare(map1, map2, ExtractMapLayout);
        public static void CompareSet(Formats.Assets.EventSet set1, Formats.Assets.EventSet set2) => Compare(set1, set2, ExtractSetLayout);

        static void Compare<T>(T expected, T actual, LayoutExtractor<T> extractor)
        {
            var (e, c, x) = extractor(expected);
            var expectedLayout = new EventLayout(e, c, x);
            var actualLayout = BuildLayout(actual, extractor);

            if (!CompareLayoutBytes(actualLayout, expectedLayout, out var message))
                throw new InvalidOperationException($"Difference detected: {message}");
        }

        static EventLayout BuildLayout<T>(T value, LayoutExtractor<T> extractor)
        {
            var (events, chains, extra) = extractor(value);
            var regions = Decompiler.BuildEventRegions(events, chains, extra);
            return EventLayout.Build(regions);
        }

        static T Canonicalize<T>(T value, LayoutExtractor<T> extractor, LayoutConstructor<T> constructor) => constructor(value, BuildLayout(value, extractor)); 
        static Formats.Assets.EventSet ConstructSet(Formats.Assets.EventSet x, EventLayout layout) => new(x.Id, layout.Events, layout.Chains);
        static (List<EventNode> events, List<ushort> chains, List<ushort> extra) ExtractSetLayout(Formats.Assets.EventSet x) => (x.Events, x.Chains, null);

        static T ConstructMap<T>(T x, EventLayout layout) where T : BaseMapData
        {
            x.RemapChains(layout.Events, layout.Chains);
            return x;
        }

        static (List<EventNode> events, List<ushort> chains, List<ushort> extra) ExtractMapLayout(BaseMapData map)
        {
            var entryPoints = map.Npcs.Select(x => x.EventIndex)
                .Concat(map.Zones.Select(x => x.EventIndex))
                .ToHashSet();
            entryPoints.ExceptWith(map.Chains);
            entryPoints.Remove(0xffff);

            return (map.Events, map.Chains, entryPoints.ToList());
        }

        public static bool CompareLayoutBytes(EventLayout actual, EventLayout expected, out string message)
        {
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

            for (int i = 0; i < actual.Chains.Count; i++)
            {
                // TODO
            }

            for (int i = 0; i < actual.ExtraEntryPoints.Count; i++)
            {
                // TODO
            }

            message = null;
            return true;
        }
    }
}