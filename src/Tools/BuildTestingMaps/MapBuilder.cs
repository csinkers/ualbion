using System.Text;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;

namespace BuildTestingMaps;

public class MapBuilder
{
    readonly ListStringCollection _mapStrings = new();
    readonly Dictionary<int, string> _scripts = new();
    readonly BaseMapData _map;

    MapBuilder(MapId id, PaletteId palette, TilesetId tileset, byte width, byte height)
    {
        Id = id;
        Width = width;
        Height = height;
        _map = new MapData2D(id, palette, tileset, width, height) { Flags = MapFlags.V2NpcData | MapFlags.ExtraNpcs };
        while (_map.Npcs.Count < 96)
            _map.Npcs.Add(MapNpc.Unused);
    }

    MapBuilder(MapId id, PaletteId palette, LabyrinthId labyrinth, byte width, byte height)
    {
        Id = id;
        Width = width;
        Height = height;
        _map = new MapData3D(id, palette, labyrinth, width, height) { Flags = MapFlags.V2NpcData | MapFlags.ExtraNpcs };
        while (_map.Npcs.Count < 96)
            _map.Npcs.Add(MapNpc.Unused);
    }

    public int AddMapText(string text) => _mapStrings.FindOrAdd(text);
    public static MapBuilder Create2D(MapId id, PaletteId palette, TilesetId tileset, byte width, byte height) => new(id, palette, tileset, width, height);
    public static MapBuilder Create3D(MapId id, PaletteId palette, LabyrinthId lab, byte width, byte height) => new(id, palette, lab, width, height);

    public MapId Id { get; }
    public int Width { get; }
    public int Height { get; }

    public MapBuilder SetChain(int i, Func<Func<string, int>, string> func)
    {
        _scripts[i] = func(AddMapText);
        return this;
    }

    public MapBuilder AddGlobalZone(TriggerTypes trigger, ushort chain)
    {
        _map.AddGlobalZone(trigger, chain);
        return this;
    }

    public MapBuilder SetZone(byte x, byte y, TriggerTypes trigger, ushort chain)
    {
        _map.AddZone(x, y, trigger, chain);
        return this;
    }

    public MapBuilder Draw2D(Action<MapData2D> func)
    {
        func((MapData2D)_map);
        return this;
    }

    public MapBuilder Draw3D(Action<MapData3D> func)
    {
        func((MapData3D)_map);
        return this;
    }

    public (BaseMapData, ListStringCollection) Build()
    {
        var sb = new StringBuilder();
        foreach (var key in _scripts.Keys)
        {
            sb.AppendLine("{");
            sb.AppendLine("Chain" + key + ":");
            sb.AppendLine(_scripts[key]);
            sb.AppendLine("}");
        }

        var script = sb.ToString();
        var compiled = AlbionCompiler.Compile(script, _map.Id.ToMapText());

        foreach (var e in compiled.Events) _map.Events.Add(e);
        foreach (var c in compiled.Chains) _map.Chains.Add(c);

        _map.Unswizzle();
        return (_map, _mapStrings);
    }
}