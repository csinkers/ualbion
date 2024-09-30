using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;

namespace BuildTestingMaps;

public class MapBuilder3D : MapBuilder
{
    // readonly TestLab _lab;
    public MapBuilder3D(MapId id, PaletteId palette, TestLab lab, byte width, byte height)
        : base(new MapData3D(id, palette, lab.Lab.Id, width, height) { Flags = MapFlags.V2NpcData | MapFlags.ExtraNpcs })
    {
        // _lab = lab;
    }

    public MapBuilder Draw(Action<MapData3D> func)
    {
        func((MapData3D)Map);
        return this;
    }
}