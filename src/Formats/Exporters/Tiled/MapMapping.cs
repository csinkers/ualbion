using System;
using System.Collections.Generic;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using static UAlbion.Formats.Exporters.Tiled.MapperUtil;

namespace UAlbion.Formats.Exporters.Tiled;

public static class MapMapping
{
    public static class MapPropName
    {
        public const string Ambient = "Ambient";
        public const string CombatBackground = "CombatBackground";
        public const string Flags = "Flags";
        public const string FrameRate = "FrameRate";
        public const string Labyrinth = "Labyrinth";
        public const string OriginalNpcCount = "OriginalNpcCount";
        public const string Palette = "Palette";
        public const string Song = "Song";
        public const string Sound = "Sound";
        public const string Tileset = "Tileset";
    }

    public static void ReadMapProperties(BaseMapData albionMap, Map tiledMap)
    {
        albionMap.OriginalNpcCount = (byte)(PropInt(tiledMap, MapPropName.OriginalNpcCount) ?? 96);
        albionMap.CombatBackgroundId = PropId(tiledMap, MapPropName.CombatBackground);
        albionMap.PaletteId = PropId(tiledMap, MapPropName.Palette, true);
        albionMap.SongId = PropId(tiledMap, MapPropName.Song);

        if (albionMap is MapData2D map2d)
        {
            map2d.Flags = Enum.Parse<FlatMapFlags>(PropString(tiledMap, MapPropName.Flags, true));
            map2d.FrameRate = (byte)(PropInt(tiledMap, MapPropName.FrameRate) ?? 0);
            map2d.Sound = (byte)(PropInt(tiledMap, MapPropName.Sound) ?? 0);
            map2d.TilesetId = PropId(tiledMap, MapPropName.Tileset, true);
        }

        if (albionMap is MapData3D map3d)
        {
            map3d.AmbientSongId = PropId(tiledMap, MapPropName.Ambient);
            map3d.Flags = Enum.Parse<Map3DFlags>(PropString(tiledMap, MapPropName.Flags, true));
            map3d.LabDataId = PropId(tiledMap, MapPropName.Labyrinth, true);
        }
    }

    public static List<TiledProperty> BuildMapProperties(BaseMapData map)
    {
        var props = new List<TiledProperty>();
        props.Add(new(MapPropName.OriginalNpcCount, map.OriginalNpcCount));
        props.Add(new(MapPropName.Palette, map.PaletteId.ToString()));

        if (map.SongId != SongId.None) props.Add(new(MapPropName.Song, map.SongId.ToString()));
        if (map.CombatBackgroundId != SpriteId.None) props.Add(new(MapPropName.CombatBackground, map.CombatBackgroundId.ToString()));

        if (map is MapData2D map2d)
        {
            props.Add(new(MapPropName.Flags, map2d.Flags.ToString()));
            props.Add(new(MapPropName.Tileset, map2d.TilesetId.ToString()));
            if (map2d.FrameRate > 0) props.Add(new(MapPropName.FrameRate, map2d.FrameRate));
            if (map2d.Sound > 0) props.Add(new(MapPropName.Sound, map2d.Sound));
        }

        if (map is MapData3D map3d)
        {
            props.Add(new(MapPropName.Ambient, map3d.AmbientSongId.ToString()));
            props.Add(new(MapPropName.Flags, map3d.Flags.ToString()));
            props.Add(new(MapPropName.Labyrinth, map3d.LabDataId.ToString()));
        }

        return props;
    }
}