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
    readonly Dictionary<TriggerTypes, List<int>> _zoneTypeLookup = new();
    int[] _zoneLookup;

    [JsonInclude] public MapId Id { get; protected set; }
    public abstract MapType MapType { get; }
    [JsonInclude] public byte Width { get; protected set; }
    [JsonInclude] public byte Height { get; protected set; }
    [JsonInclude] public SongId SongId { get; set; }
    [JsonInclude] public PaletteId PaletteId { get; set; }
    [JsonInclude] public SpriteId CombatBackgroundId { get; set; }
    [JsonInclude] public byte OriginalNpcCount { get; set; }
    [JsonInclude] public List<MapNpc> Npcs { get; protected set; }

    [JsonInclude] public List<MapEventZone> Zones { get; private set; } = new();
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
        PaletteId paletteId,
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
        PaletteId = paletteId;
        Width = width;
        Height = height;
        Npcs = npcs.ToList();

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
        _zoneLookup = new int[Width * Height];
        _zoneTypeLookup.Clear();
        Array.Fill(_zoneLookup, -1);

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
        foreach (var npc in Npcs)
            npc.Unswizzle(Id, x => Events[x], x => chainMapping[x]);

        foreach (var zone in Zones)
            zone.Unswizzle(Id, x => Events[x], x => chainMapping[x]);

        foreach (var position in Zones.Where(x => !x.Global).Select((x,i) => (zoneIndex: i, tileIndex: Index(x.X, x.Y))).GroupBy(x => x.tileIndex))
        {
            var zone = position.Single();
            _zoneLookup[zone.tileIndex] = zone.zoneIndex;
        }

        foreach (var triggerType in Zones.Select((x,i) => (zoneIndex: i, type: x.Trigger)).GroupBy(x => x.type))
            _zoneTypeLookup[triggerType.Key] = triggerType.Select(x => x.zoneIndex).ToList();

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
        for (var index = 0; index < Npcs.Count; index++)
        {
            var npc = Npcs[index];
            if (npc == null) continue;
            s.Begin("NpcWaypoints" + index);
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

    public int Index(int x, int y) => y * Width + x;
    public MapEventZone GetZone(int x, int y) => GetZone(Index(x, y));
    public MapEventZone GetZone(int tileIndex)
    {
        int zoneIndex = _zoneLookup[tileIndex];
        return zoneIndex == -1 ? null : Zones[zoneIndex];
    }

    public IEnumerable<MapEventZone> GetZonesOfType(TriggerTypes triggerType)
    {
        var matchingKeys = _zoneTypeLookup.Keys.Where(x => (x & triggerType) == triggerType);
        return matchingKeys.SelectMany(x => _zoneTypeLookup[x]).Select(x => Zones[x]);
    }

    public void AddZone(byte x, byte y, TriggerTypes trigger, ushort chain) => AddZoneInner(false, x, y, trigger, chain);
    public void AddGlobalZone(TriggerTypes trigger, ushort chain) => AddZoneInner(true, 0, 0xff, trigger, chain);
    public void RemoveZone(byte x, byte y) => RemoveZone(_zoneLookup[Index(x, y)]);
    void RemoveZone(int zoneIndex)
    {
        if (zoneIndex == -1)
            return;

        var zone = Zones[zoneIndex];
        if (zone == null)
            return;

        if (!zone.Global)
        {
            var tileIndex = Index(zone.X, zone.Y);
            _zoneLookup[tileIndex] = -1;
        }

        if (_zoneTypeLookup.TryGetValue(zone.Trigger, out var ofType))
            ofType.Remove(zoneIndex);

        Zones.RemoveAt(zoneIndex);
    }

    void AddZoneInner(bool global, byte x, byte y, TriggerTypes trigger, ushort chain)
    {
        var eventIndex = Chains[chain];
        var zone = new MapEventZone
        {
            X = x,
            Y = y,
            Global = global,
            Trigger = trigger,
            Unk1 = 0,
            Chain = chain,
            ChainSource = Id,
            EventIndex = eventIndex,
            Node = Events[eventIndex],
        };

        int index = Index(x, y);
        int existingZoneIndex = _zoneLookup[index];
        if (existingZoneIndex != -1)
            RemoveZone(existingZoneIndex);

        if (!_zoneTypeLookup.TryGetValue(trigger, out var ofType))
        {
            ofType = new List<int>();
            _zoneTypeLookup[trigger] = ofType;
        }

        _zoneLookup[index] = Zones.Count;
        ofType.Add(Zones.Count);
        Zones.Add(zone);
    }

    public void SetZoneChain(byte x, byte y, ushort value)
    {
        var zoneIndex = _zoneLookup[Index(x, y)];
        if (zoneIndex == -1)
            return;

        if (value >= Chains.Count)
        {
            RemoveZone(zoneIndex);
            return;
        }

        var zone = Zones[zoneIndex];
        zone.Chain = value;
        zone.ChainSource = Id;
        var firstEventId = Chains[value];
        zone.Node = firstEventId >= Events.Count ? null : Events[firstEventId];
    }

    public void SetZoneTrigger(byte x, byte y, TriggerTypes value)
    {
        var zoneIndex = _zoneLookup[Index(x, y)];
        if (zoneIndex != -1)
            Zones[zoneIndex].Trigger = value;
    }
}