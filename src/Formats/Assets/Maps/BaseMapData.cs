using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets.Maps;

public abstract class BaseMapData : IMapData, IJsonPostDeserialise
{
    [JsonInclude] public MapId Id { get; protected set; }
    public abstract MapType MapType { get; }
    [JsonInclude] public byte Width { get; protected set; }
    [JsonInclude] public byte Height { get; protected set; }
    [JsonInclude] public SongId SongId { get; set; }
    [JsonInclude] public PaletteId PaletteId { get; set; }
    [JsonInclude] public SpriteId CombatBackgroundId { get; set; }
    [JsonInclude] public byte OriginalNpcCount { get; set; }
    [JsonInclude] public MapNpc[] Npcs { get; protected set; }

    [JsonInclude] public List<MapEventZone> Zones { get; private set; } = new();
    [JsonIgnore] public Dictionary<int, MapEventZone> ZoneLookup { get; } = new();
    [JsonIgnore] public Dictionary<TriggerTypes, MapEventZone[]> ZoneTypeLookup { get; } = new();
    [JsonIgnore] public List<EventNode> Events { get; private set; } = new();
    [JsonInclude] public List<ushort> Chains { get; private set; } = new();
    public string[] EventStrings // Used for JSON
    {
        get => Events?.Select(x => x.ToString()).ToArray();
        set
        {
            Events = value?.Select(EventNode.Parse).ToList() ?? new List<EventNode>();
            foreach (var e in Events)
                e.Unswizzle(Events);
        }
    }
#if DEBUG
    [JsonIgnore] public IList<object>[] EventReferences { get; private set; }
#endif

    protected BaseMapData() { }
    protected BaseMapData(MapId id,
        byte width, byte height,
        IList<EventNode> events, IList<ushort> chains,
        IEnumerable<MapNpc> npcs,
        IList<MapEventZone> zones)
    {
        if (events == null) throw new ArgumentNullException(nameof(events));
        if (chains == null) throw new ArgumentNullException(nameof(chains));
        if (npcs == null) throw new ArgumentNullException(nameof(npcs));
        if (zones == null) throw new ArgumentNullException(nameof(zones));

        Id = id;
        Width = width;
        Height = height;
        Npcs = npcs.ToArray();

        foreach (var e in events) Events.Add(e);
        foreach (var c in chains) Chains.Add(c);
        foreach (var z in zones) Zones.Add(z);
        Unswizzle();
    }

    protected void SerdesZones(ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        int zoneCount = s.UInt16("GlobalZoneCount", (ushort)Zones.Count(x => x.Global));
        // TODO: This is assuming that global events will always come first in the in-memory list, may need
        // to add some code to preserve this invariant later on when handling editing / patching functionality.
        s.List(nameof(Zones), Zones, (byte)255, zoneCount, (i, x, y2, serializer) => MapEventZone.Serdes(x, serializer, y2));
        s.Check();

        int zoneOffset = zoneCount;
        for (byte y = 0; y < Height; y++)
        {
            if (s.IsCommenting())
                s.Comment($"Line {y}");

            zoneCount = s.UInt16("RowZones", (ushort)Zones.Count(x => x.Y == y && !x.Global));
            s.List(nameof(Zones), Zones, y, zoneCount, zoneOffset,
                (i, x, y2, s2) => MapEventZone.Serdes(x, s2, y2));
            zoneOffset += zoneCount;
        }
    }

    protected void SerdesEvents(AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        ushort eventCount = s.UInt16("EventCount", (ushort)Events.Count);

        if (Events != null) // Ensure ids match up
            for (ushort i = 0; i < Events.Count; i++)
                Events[i].Id = i;

        s.List(nameof(Events), Events, eventCount, (i, x, serializer)
            => MapEvent.SerdesNode((ushort)i, x, serializer, Id, Id.ToMapText(), mapping));

        foreach (var node in Events)
            node.Unswizzle(Events);

        s.Check();
    }

    protected void SerdesChains(ISerializer s, int chainCount)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        while (Chains.Count < chainCount)
            Chains.Add(EventNode.UnusedEventId);

        s.List(nameof(Chains), Chains, chainCount, (_, x, s2) => s2.UInt16(null, x));
        s.Check();
    }

    protected void Unswizzle() // Resolve event indices to pointers
    {
        var chainMapping = new ushort[Events.Count];
        var sortedChains = Chains
            .Select((eventId, chainId) => (eventId, chainId))
            .OrderBy(x => x)
            .ToArray();

        for (int i = 0, j = 0; i < Events.Count; i++)
        {
            while (sortedChains.Length > j + 1 && sortedChains[j].eventId < i) j++;
            chainMapping[i] = (ushort)sortedChains[j].chainId;
        }

        // Use map events if the event number is set, otherwise use the event set from the NPC's character sheet.
        // Note: Event set loading requires IAssetManager, so can't be done directly by UAlbion.Formats code.
        // Instead, the MapManager will call AttachEventSets with a callback to load the event sets.
        int npcNumber = 0;
        foreach (var npc in Npcs)
            npc.Unswizzle(Id, npcNumber++, x => Events[x], x => chainMapping[x]);

        foreach (var zone in Zones)
            zone.Unswizzle(Id, x => Events[x], x => chainMapping[x]);

        foreach (var position in Zones.Where(x => !x.Global).GroupBy(x => x.Y * Width + x.X))
        {
            var zone = position.SingleOrDefault();
            if (zone != null)
                ZoneLookup[position.Key] = zone;
        }

        foreach (var triggerType in Zones.GroupBy(x => x.Trigger))
            ZoneTypeLookup[triggerType.Key] = triggerType.ToArray();

#if DEBUG
        EventReferences = new IList<object>[Events.Count];
        foreach (var zone in Zones)
            if (zone.Node != null)
                AddEventReference(zone.Node.Id, zone);

        foreach (var npc in Npcs)
            if (npc.Node != null)
                AddEventReference(npc.Node.Id, npc);

        foreach (var e in Events)
        {
            if (e.Next != null)
                AddEventReference(e.Next.Id, e);

            if (e is BranchNode { NextIfFalse: { } } branch)
                AddEventReference(branch.NextIfFalse.Id, e);
        }
#endif
    }

#if DEBUG
    void AddEventReference(int id, object referrer)
    {
        EventReferences[id] ??= new List<object>();
        EventReferences[id].Add(referrer);
    }
#endif

    protected void SerdesNpcWaypoints(ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        foreach (var npc in Npcs)
        {
            if (npc == null) continue;
            s.Begin("NpcWaypoints" + npc.Index);
            if (npc.Id.Type != AssetType.None)
                npc.LoadWaypoints(s);
            else
                npc.Waypoints = new NpcWaypoint[1];
            s.End();
        }
    }

    public static IMapData Serdes(AssetInfo info, IMapData existing, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (s.BytesRemaining == 0)
            return null;

        var mapType = existing switch
        {
            MapData2D => MapType.TwoD, // TwoDOutdoors is never written to disk
            MapData3D => MapType.ThreeD,
            _ => MapType.Unknown
        };

        var startPosition = s.Offset;
        s.UInt16("DummyRead", 0); // Initial flags + npc count, will be re-read by the 2D/3D specific map loader
        mapType = s.EnumU8(nameof(mapType), mapType);
        s.Seek(startPosition);

        return mapType switch
        {
            // Indoor/outdoor maps aren't distinguished on disk - it has to be inferred from the tileset
            MapType.TwoD => MapData2D.Serdes(info, (MapData2D)existing, mapping, s),
            MapType.ThreeD => MapData3D.Serdes(info, (MapData3D)existing, mapping, s),
            _ => throw new NotImplementedException($"Unrecognised map type {mapType} found.")
        };
    }

    public void OnDeserialized() => Unswizzle();
    public void RemapChains(List<EventNode> events, List<ushort> chains)
    {
        foreach (var npc in Npcs)
            if (npc.Chain != EventNode.UnusedEventId)
                npc.EventIndex = chains[npc.Chain];

        foreach (var zone in Zones)
            if (zone.Chain != EventNode.UnusedEventId)
                zone.EventIndex = chains[zone.Chain];

        Events.Clear();
        Events.AddRange(events);
        Chains.Clear();
        Chains.AddRange(chains);

        Unswizzle();
    }
}