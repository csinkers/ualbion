using System.Text;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;

namespace BuildTestingMaps;

public class MapBuilder
{
    readonly ListStringCollection _mapStrings = new();
    readonly Dictionary<int, string> _scripts = new();
    readonly BaseMapData _map;
    readonly TestTilemap _tilemap;
    readonly TestLab _lab;

    MapBuilder(MapId id, PaletteId palette, TestTilemap tilemap, byte width, byte height)
    {
        Id = id;
        Width = width;
        Height = height;
        _tilemap = tilemap ?? throw new ArgumentNullException(nameof(tilemap));
        _map = new MapData2D(id, palette, tilemap.Tileset.Id, width, height) { Flags = MapFlags.V2NpcData | MapFlags.ExtraNpcs };
        while (_map.Npcs.Count < 96)
            _map.Npcs.Add(MapNpc.Unused);
    }

    MapBuilder(MapId id, PaletteId palette, TestLab lab, byte width, byte height)
    {
        Id = id;
        Width = width;
        Height = height;
        _lab = lab;
        _map = new MapData3D(id, palette, lab.Lab.Id, width, height) { Flags = MapFlags.V2NpcData | MapFlags.ExtraNpcs };
        while (_map.Npcs.Count < 96)
            _map.Npcs.Add(MapNpc.Unused);
    }

    public int AddMapText(string text) => _mapStrings.FindOrAdd(text);
    public static MapBuilder Create2D(MapId id, PaletteId palette, TestTilemap tilemap, byte width, byte height) => new(id, palette, tilemap, width, height);
    public static MapBuilder Create3D(MapId id, PaletteId palette, TestLab lab, byte width, byte height) => new(id, palette, lab, width, height);

    public MapId Id { get; }
    public int Width { get; }
    public int Height { get; }

    public MapBuilder SetChain(int i, Func<Func<string, int>, string> func)
    {
        _scripts[i] = func(AddMapText);
        return this;
    }

    public MapBuilder AddGlobalZone(TriggerTypes trigger, int chain)
    {
        if (chain is > ushort.MaxValue or < 0)
            throw new ArgumentOutOfRangeException(nameof(chain));
        _map.AddGlobalZone(trigger, (ushort)chain);
        return this;
    }

    public MapBuilder SetZone(byte x, byte y, TriggerTypes trigger, int chain)
    {
        if (chain is > ushort.MaxValue or < 0)
            throw new ArgumentOutOfRangeException(nameof(chain));
        _map.AddZone(x, y, trigger, (ushort)chain);
        return this;
    }

    public void DrawBorder()
    {
        Draw2D(m =>
        {
            for (int i = 0; i < m.Tiles.Length; i++)
            {
                var y = i / m.Width;
                var x = i % m.Width;
                m.Tiles[i] = new MapTile(
                    x == 0 || y == 0 || x == m.Width - 1 || y == m.Height - 1
                        ? _tilemap.SolidOffset
                        : _tilemap.BlankOffset, 0);
            }
        });
    }

    public void Marker(int index, int x, int y, string description, Func<Func<string,int>, string> script)
    {
        SetChain(index, s => @$"
if (query_verb examine) {{
    text {s(description)}
}} else {{
   {script(s)}
}}
");

        Draw2D(m =>
        {
            m.AddZone((byte)x, (byte)y, TriggerTypes.Examine | TriggerTypes.Manipulate, (ushort)index);
            m.Tiles[ y * m.Width + x].Underlay = (ushort)(_tilemap.TextOffset + description[0]);
        });
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