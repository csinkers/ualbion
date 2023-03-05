using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Exporters.Tiled;

public static class AutomapMapping
{
    static class Prop
    {
        public const string Unk2 = "Unk2";
        public const string Unk3 = "Unk3";
        public const string Visual = "Visual";
    }

    public static IEnumerable<ObjectGroup> BuildMarkers(MapData3D map, int tileWidth, int tileHeight, ref int nextObjectGroupId, ref int nextObjectId)
    {
        if (map.Automap == null || map.Automap.Count == 0 || map.AutomapGraphics == null)
            return Enumerable.Empty<ObjectGroup>();

        int nextId = nextObjectId;
        int npcGroupId = nextObjectGroupId++;

        var group = new ObjectGroup
        {
            Id = npcGroupId,
            Name = "Map Markers",
            Objects = 
                map.Automap
                   .Zip(map.AutomapGraphics)
                   .Select(x => BuildMarker(tileWidth, tileHeight, x.First, x.Second, ref nextId))
                   .ToList()
        };

        nextObjectId = nextId;
        return new[] { group };
    }

    static MapObject BuildMarker(int tileWidth, int tileHeight, AutomapInfo marker, byte tile, ref int nextId) =>
        new()
        {
            Id = nextId++,
            Name = marker.Name,
            Type = ObjectGroupMapping.TypeName.Marker,
            X = marker.X * tileWidth,
            Y = marker.Y * tileHeight,
            Properties = new List<TiledProperty>
            {
                new(Prop.Visual, tile.ToString()),
                new(Prop.Unk2, marker.Unk2.ToString()),
                new(Prop.Unk3, marker.MarkerId.ToString())
            }
        };

    public static (AutomapInfo, byte) ParseMarker(MapObject obj, int tileWidth, int tileHeight)
    {
        var unk2 = obj.PropInt(Prop.Unk2) ?? 0;
        var unk3 = obj.PropInt(Prop.Unk3) ?? 0;
        var visual = obj.PropInt(Prop.Visual);

        if (visual == null) throw new FormatException($"Automap marker {obj.Name} is missing a \"Visual\" property");
        if (unk2 is < 0 or > byte.MaxValue) throw new FormatException($"Automap marker {obj.Name} has an out of range value for the \"{Prop.Unk2}\" property (expected [0..255]");
        if (unk3 is < 0 or > byte.MaxValue) throw new FormatException($"Automap marker {obj.Name} has an out of range value for the \"{Prop.Unk3}\" property (expected [0..255]");
        if (visual is < 0 or > byte.MaxValue) throw new FormatException($"Automap marker {obj.Name} has an out of range value for the \"{Prop.Visual}\" property (expected [0..255]");
        if (obj.Name.Length > AutomapInfo.MaxNameLength) ApiUtil.Assert($"Automap marker name \"{obj.Name}\" is longer than the maximum name length ({AutomapInfo.MaxNameLength}) and will be truncated if exported to the original formats.");

        return (new AutomapInfo
        {
            Name = obj.Name,
            X = (byte)(obj.X / tileWidth),
            Y = (byte)(obj.Y / tileHeight),
            Unk2 = (byte)unk2,
            MarkerId = (byte)unk3,
        }, (byte)visual);
    }
}
