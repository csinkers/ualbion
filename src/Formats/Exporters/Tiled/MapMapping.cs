using System;
using System.Collections.Generic;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;
using static UAlbion.Formats.Exporters.Tiled.MapperUtil;

namespace UAlbion.Formats.Exporters.Tiled;

public static class MapMapping
{
    public static class Prop
    {
        public const string Ambient = "Ambient";
        public const string CombatBackground = "CombatBackground";
        public const string Flags = "Flags";
        public const string FrameRate = "FrameRate";
        public const string Labyrinth = "Labyrinth";
        public const string Palette = "Palette";
        public const string Song = "Song";
        public const string Sound = "Sound";
        public const string Tileset = "Tileset";
    }

    public static void ReadMapProperties(BaseMapData albionMap, Map tiledMap)
    {
        ArgumentNullException.ThrowIfNull(albionMap);

        albionMap.CombatBackgroundId = PropId(tiledMap, Prop.CombatBackground);
        albionMap.SongId = PropId(tiledMap, Prop.Song);

        if (albionMap is MapData2D map2d)
        {
            map2d.Flags = Enum.Parse<MapFlags>(PropString(tiledMap, Prop.Flags, true));
            map2d.FrameRate = (byte)(PropInt(tiledMap, Prop.FrameRate) ?? 0);
            map2d.Sound = (byte)(PropInt(tiledMap, Prop.Sound) ?? 0);
        }

        if (albionMap is MapData3D map3d)
        {
            map3d.AmbientSongId = PropId(tiledMap, Prop.Ambient);
            map3d.Flags = Enum.Parse<MapFlags>(PropString(tiledMap, Prop.Flags, true));
        }
    }

    public static List<TiledProperty> BuildMapProperties(BaseMapData map)
    {
        ArgumentNullException.ThrowIfNull(map);

        var props = new List<TiledProperty>();
        props.Add(new(Prop.Palette, map.PaletteId.ToString()));

        if (map.SongId != SongId.None) props.Add(new(Prop.Song, map.SongId.ToString()));
        if (map.CombatBackgroundId != CombatBackgroundId.None)
            props.Add(new(Prop.CombatBackground, map.CombatBackgroundId.ToString()));

        if (map is MapData2D map2d)
        {
            props.Add(new(Prop.Flags, map2d.Flags.ToString()));
            props.Add(new(Prop.Tileset, map2d.TilesetId.ToString()));
            if (map2d.FrameRate > 0) props.Add(new(Prop.FrameRate, map2d.FrameRate));
            if (map2d.Sound > 0) props.Add(new(Prop.Sound, map2d.Sound));
        }

        if (map is MapData3D map3d)
        {
            props.Add(new(Prop.Ambient, map3d.AmbientSongId.ToString()));
            props.Add(new(Prop.Flags, map3d.Flags.ToString()));
            props.Add(new(Prop.Labyrinth, map3d.LabDataId.ToString()));
        }

        return props;
    }
}