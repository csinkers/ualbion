using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Scripting;

namespace UAlbion.Formats.Assets.Maps;

public abstract class BaseMapData : IMapData, IJsonPostDeserialise
{
    protected const int OffsetX = 1; // Compensation for all absolute map coordinates being relative to (1,1).
    protected const int OffsetY = 1;

    readonly Dictionary<TriggerTypes, HashSet<MapEventZone>> _zoneTypeLookup = new();
    ushort[] _chainMapping;

    AssetId IEventSet.Id => Id;
    [JsonInclude] public MapId Id { get; protected set; }
    [JsonIgnore] public StringSetId StringSetId => Id.ToMapText();
    public abstract MapType MapType { get; }
    [JsonInclude] public MapFlags Flags { get; set; } // Wait/Rest, Light-Environment, NPC converge range
    [JsonInclude] public int Width { get; protected set; }
    [JsonInclude] public int Height { get; protected set; }
    [JsonInclude] public SongId SongId { get; set; }
    [JsonInclude] public PaletteId PaletteId { get; set; }
    [JsonInclude] public SpriteId CombatBackgroundId { get; set; }
    [JsonInclude] public List<MapNpc> Npcs { get; protected set; }
    [JsonIgnore] public IList<EventNode> Events { get; private set; } = new List<EventNode>();
    [JsonInclude] public IList<ushort> Chains { get; private set; } = new List<ushort>();
    [JsonIgnore] public DecompilationResult Decompiled { get; set; }

    [JsonIgnore]
    public IList<ushort> ExtraEntryPoints
    {
        get
        {
            var npcRefs = Npcs.Where(x => x.Node != null).Select(x => x.Node.Id).ToHashSet();
            var zoneRefs = UniqueZoneNodeIds;
            return npcRefs.Union(zoneRefs).Except(Chains).ToList();
        }
    }

    [JsonInclude] public string[] ZoneText // for JSON
    {
        get => GlobalZones.Concat(Zones).Where(x => x != null).Select(x => x.ToString()).ToArray();
        set => GlobalZones = value.Select(MapEventZone.Parse).ToList(); // Put all into global temporarily, then sort them out in the post-deserialise code
    }

    [JsonInclude] public string[] EventStrings // Used for JSON
    {
        get => Events?.Select(x => x.ToString()).ToArray();
        set => Events = value?.Select(EventNode.Parse).ToList() ?? new List<EventNode>();
    }

    [JsonIgnore] public HashSet<ushort> UniqueZoneNodeIds => GlobalZones.Concat(Zones).Where(x => x?.Node != null).Select(x => x.Node.Id).ToHashSet();
    [JsonIgnore] internal List<MapEventZone> GlobalZones { get; private set; } = new();
    [JsonIgnore] internal MapEventZone[] Zones { get; private set; } // This should only ever be modified using the Add/RemoveZone methods

    [JsonIgnore] public MapSubMode SubMode
    {
        get => (MapSubMode)(
            ((Flags & MapFlags.SubMode1) != 0 ? 1 : 0) |
            ((Flags & MapFlags.SubMode2) != 0 ? 2 : 0));
        set => Flags =
            Flags & ~MapFlags.SubModeMask
            | (((int)value &  1) != 0 ? MapFlags.SubMode1 : 0)
            | (((int)value &  2) != 0 ? MapFlags.SubMode2 : 0);
    }

    [JsonIgnore] public RestMode RestMode
    {
        get => (RestMode)(
            ((Flags & MapFlags.RestMode1) != 0 ? 1 : 0) |
            ((Flags & MapFlags.RestMode2) != 0 ? 2 : 0));
        set => Flags =
            Flags & ~MapFlags.RestModeMask
            | (((int)value &  1) != 0 ? MapFlags.RestMode1 : 0)
            | (((int)value &  2) != 0 ? MapFlags.RestMode2 : 0);
    }


#if DEBUG
    [JsonIgnore] public IList<object>[] EventReferences { get; private set; }
#endif
    public ushort GetChainForEvent(ushort index) => index >= Events.Count ? EventNode.UnusedEventId : _chainMapping[index];

    protected BaseMapData() { }
    protected BaseMapData(MapId id,
        PaletteId paletteId,
        int width, int height,
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
        Zones = new MapEventZone[width * height];

        foreach (var e in events) Events.Add(e);
        foreach (var c in chains) Chains.Add(c);
        foreach (var z in zones) SetZone(z);

        Unswizzle();
    }

    protected BaseMapData(MapId id, PaletteId paletteId, int width, int height)
    {
        Id = id;
        PaletteId = paletteId;
        Width = width;
        Height = height;
        Npcs = new List<MapNpc>();
        Zones = new MapEventZone[width * height];
    }

    protected int SerdesZones(ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        Zones ??= new MapEventZone[Width * Height];
        int zoneCount = s.UInt16("GlobalZoneCount", (ushort)GlobalZones.Count);
        int totalCount = zoneCount;
        GlobalZones = (List<MapEventZone>)s.List(
            nameof(GlobalZones),
            GlobalZones,
            (byte)255,
            zoneCount,
            (_, zone, y2, serializer) => MapEventZone.Serdes(zone, serializer, y2),
            n => new List<MapEventZone>(n));

        for (int y = OffsetY; y < Height + OffsetY - 1; y++)
        {
            if (s.IsCommenting())
                s.Comment($"Line {y}");

            var row = s.IsWriting() ? Zones.Skip(y * Width).Take(Width).Where(x => x != null).ToList() : null;
            zoneCount = s.UInt16("RowZones", (ushort)(row?.Count ?? 0));
            totalCount += zoneCount;

            row = (List<MapEventZone>)s.List(
                nameof(Zones),
                row, y, zoneCount,
                (_, zone, y2, s2) => MapEventZone.Serdes(zone, s2, (byte)y2),
                n => new List<MapEventZone>(n));

            if (s.IsReading())
                foreach (var zone in row)
                    Zones[Index(zone.X, zone.Y)] = zone;
        }

        return totalCount;
    }

    protected void SerdesEvents(AssetMapping mapping, MapType mapType, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        ushort eventCount = s.UInt16("EventCount", (ushort)Events.Count);

        if (Events != null) // Ensure ids match up
            for (ushort i = 0; i < Events.Count; i++)
                Events[i].Id = i;

        s.List(nameof(Events), Events, eventCount,
            (i, x, serializer) =>
        {
            var node = MapEvent.SerdesNode((ushort)i, x, serializer, mapping, mapType);
            if (serializer.IsCommenting())
                serializer.Comment(node.ToString());
            return node;
        });

        foreach (var node in Events)
            node.Unswizzle(Events);
    }

    protected void SerdesChains(ISerializer s, int chainCount)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        while (Chains.Count < chainCount)
            Chains.Add(EventNode.UnusedEventId);

        s.List(nameof(Chains), Chains, chainCount, (_, x, s2) => s2.UInt16(null, x));
    }

    public void Unswizzle() // Resolve event indices to pointers
    {
        // TODO: Unify the chain mapping code with the copy in EventSet
        var sortedChains = Chains
            .Select((eventId, chainId) => (eventId, chainId))
            .OrderBy(x => x)
            .ToArray();

        _chainMapping = new ushort[Events.Count];
        for (int i = 0, j = 0; i < Events.Count; i++)
        {
            while (sortedChains.Length > j + 1 && sortedChains[j].eventId < i) j++;
            _chainMapping[i] = (ushort)sortedChains[j].chainId;
        }

        // Use map events if the event number is set, otherwise use the event set from the NPC's character sheet.
        // Note: Event set loading requires IAssetManager, so can't be done directly by UAlbion.Formats code.
        // Instead, the MapManager will call AttachEventSets with a callback to load the event sets.
        foreach (var npc in Npcs)
            npc.Unswizzle(Id, x => Events[x], GetChainForEvent);

        foreach (var zone in GlobalZones.Concat(Zones))
            zone?.Unswizzle(Id, x => Events[x], GetChainForEvent, x => Events[Chains[x]]);

        foreach (var triggerType in GlobalZones.Concat(Zones).Where(x => x != null).GroupBy(x => x.Trigger))
            _zoneTypeLookup[triggerType.Key] = triggerType.ToHashSet();

#if DEBUG
        EventReferences = new IList<object>[Events.Count];
        foreach (var zone in GlobalZones.Concat(Zones))
            if (zone?.Node != null)
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

            if (!npc.Id.IsNone) // If Id is 0 then NPC is inactive and has no waypoint data at all.
                npc.LoadWaypoints(s, npc.HasWaypoints(Flags));
            else 
                npc.Waypoints = new NpcWaypoint[1];

            s.End();
        }
    }

    public static IMapData Serdes(AssetId id, IMapData existing, AssetMapping mapping, ISerializer s)
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
        s.UInt16("DummyRead", 0); // Initial flags, will be re-read by the 2D/3D specific map loader
        mapType = s.EnumU8(nameof(mapType), mapType);
        s.Seek(startPosition);

        return mapType switch
        {
            // Indoor/outdoor maps aren't distinguished on disk - it has to be inferred from the tileset
            MapType.TwoD => MapData2D.Serdes(id, (MapData2D)existing, mapping, s),
            MapType.ThreeD => MapData3D.Serdes(id, (MapData3D)existing, mapping, s),
            _ => throw new NotImplementedException($"Unrecognised map type {mapType} found.")
        };
    }

    public void OnDeserialized()
    {
        var allZones = GlobalZones; // Split up the data from the JSON into global and regular zones
        GlobalZones = new List<MapEventZone>();
        Zones = new MapEventZone[Width * Height];
        foreach (var zone in allZones)
        {
            if (zone.Global)
                GlobalZones.Add(zone);
            else
            {
                var index = Index(zone.X, zone.Y);
                if (Zones[index] != null)
                    throw new InvalidOperationException($"Zone conflict at ({zone.X}, {zone.Y}");
                Zones[index] = zone;
            }
        }

        Unswizzle();
    }

    public void RemapChains(IList<EventNode> events, IList<ushort> chains)
    {
        if (chains == null) throw new ArgumentNullException(nameof(chains));

        foreach (var npc in Npcs)
            if (npc.Chain != EventNode.UnusedEventId)
                npc.EventIndex = chains[npc.Chain];

        foreach (var zone in GlobalZones)
            if (zone.Chain != EventNode.UnusedEventId)
                zone.EventIndex = chains[zone.Chain];

        foreach (var zone in Zones)
            if (zone != null && zone.Chain != EventNode.UnusedEventId)
                zone.EventIndex = chains[zone.Chain];

        Events.Clear();
        ((List<EventNode>)Events).AddRange(events);
        Chains.Clear();
        ((List<ushort>)Chains).AddRange(chains);

        Unswizzle();
    }

    public int Index(int x, int y) => y * Width + x;
    public MapEventZone GetZone(int x, int y) => GetZone(Index(x, y));
    public MapEventZone GetZone(int tileIndex)
    {
        if (tileIndex < 0 || tileIndex >= Zones.Length) return null;
        return Zones[tileIndex];
    }

    public IEnumerable<MapEventZone> GetZonesOfType(TriggerTypes triggerType)
    {
        var matchingKeys = _zoneTypeLookup.Keys.Where(x => (x & triggerType) == triggerType);
        return matchingKeys.SelectMany(x => _zoneTypeLookup[x]);
    }

    public void AddZone(byte x, byte y, TriggerTypes trigger, ushort chain) => SetZone(BuildZone(false, x, y, trigger, chain));
    public void AddGlobalZone(TriggerTypes trigger, ushort chain) => SetZone(BuildZone(true, 0, 0xff, trigger, chain));
    public void RemoveZone(byte x, byte y) => RemoveZone(Index(x, y));
    void RemoveZone(int index)
    {
        var zone = Zones[index];
        if (zone == null) return;
        Zones[index] = null;
        RemoveFromTypeLookup(zone);
    }

    MapEventZone BuildZone(bool global, byte x, byte y, TriggerTypes trigger, ushort chain)
    {
        var eventIndex = chain >= Chains.Count ? EventNode.UnusedEventId : Chains[chain];
        return new MapEventZone
        {
            X = x, Y = y, Global = global,
            Trigger = trigger,
            Unk1 = 0,
            Chain = chain,
            ChainSource = Id,
            Node = eventIndex == EventNode.UnusedEventId ? null : Events[eventIndex],
        };
    }

    void SetZone(MapEventZone zone)
    {
        if (zone.Global)
        {
            GlobalZones.Add(zone);
            AddToTypeLookup(zone);
            return;
        }

        int index = Index(zone.X, zone.Y);
        RemoveZone(index);
        Zones[index] = zone;
        AddToTypeLookup(zone);
    }

    void AddToTypeLookup(MapEventZone zone)
    {
        if (!_zoneTypeLookup.TryGetValue(zone.Trigger, out var ofType))
        {
            ofType = new HashSet<MapEventZone>();
            _zoneTypeLookup[zone.Trigger] = ofType;
        }
        ofType.Add(zone);
    }

    void RemoveFromTypeLookup(MapEventZone zone)
    {
        if (!_zoneTypeLookup.TryGetValue(zone.Trigger, out var ofType)) 
            return;

        ofType.Remove(zone);
        if (ofType.Count == 0)
            _zoneTypeLookup.Remove(zone.Trigger);
    }

    public void SetZoneChain(byte x, byte y, ushort value)
    {
        var zone = Zones[Index(x, y)];
        if (zone == null)
            return;

        zone.Chain = value;
        zone.Node = null;

        if (value < Chains.Count)
        {
            var nodeId = Chains[value];
            if (nodeId < Events.Count)
                zone.Node = Events[nodeId];
        }
    }

    public void SetZoneTrigger(byte x, byte y, TriggerTypes value)
    {
        var zone = Zones[Index(x, y)];
        if (zone == null)
            return;

        RemoveFromTypeLookup(zone);
        zone.Trigger = value;
        AddToTypeLookup(zone);
    }
}