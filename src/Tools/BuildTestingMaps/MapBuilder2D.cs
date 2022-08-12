using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;

namespace BuildTestingMaps;

public class MapBuilder2D : MapBuilder
{
    readonly TestTilemap _tilemap;
    public MapBuilder2D(MapId id, PaletteId palette, TestTilemap tilemap, byte width, byte height)
        : base(new MapData2D(id, palette, tilemap.Tileset.Id, width, height) { Flags = MapFlags.V2NpcData | MapFlags.ExtraNpcs })
    {
        _tilemap = tilemap;
    }

    public void DrawBorder()
    {
        Draw(m =>
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

        Draw(m =>
        {
            m.AddZone((byte)x, (byte)y, TriggerTypes.Examine | TriggerTypes.Manipulate, (ushort)index);
            m.Tiles[ y * m.Width + x].Underlay = (ushort)(_tilemap.TextOffset + description[0]);
        });
    }

    public MapBuilder Draw(Action<MapData2D> func)
    {
        func((MapData2D)Map);
        return this;
    }
}