using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Config;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;
using static UAlbion.Formats.Exporters.Tiled.MapperUtil;

namespace UAlbion.Formats.Exporters.Tiled;

public static class TileMapping
{
    static class Prop
    {
        public const string AutoGfx = "AutoGfx";
        public const string Collision = "Collision";
        public const string DebugDot = "DebugDot";
        public const string Flags = "Flags";
        public const string Layer = "Layer";
        public const string Bouncy = "Bouncy";
        public const string NoDraw = "NoDraw";
        public const string SitMode = "SitMode";
        public const string Type = "Type";
        public const string Unk1 = "Unk1";
        public const string Unk2 = "Unk2";
        public const string Unk3 = "Unk3";
        public const string Unk5 = "Unk5";
        public const string Unk7 = "Unk7";
        public const string Unk8 = "Unk8";
        public const string Unk9 = "Unk9";
        public const string Unk12 = "Unk12";
        public const string Unk18 = "Unk18";
        public const string UseUnderlayFlags =  "UseUnderlayFlags";
    }

    public static Tile BuildTile(TilesetId id, int index, ushort? imageNumber, List<TiledProperty> tileProperties, Tilemap2DProperties properties, AssetPathPattern graphicsPattern)
    {
        if (properties == null) throw new ArgumentNullException(nameof(properties));
        if (graphicsPattern == null) throw new ArgumentNullException(nameof(graphicsPattern));

        var source = imageNumber switch
        {
            null => null,
            0xffff => properties.BlankTilePath,
            _ => graphicsPattern.Format(new AssetPath(id, imageNumber.Value))
        };

        return new Tile
        {
            Id = index,
            Properties = tileProperties,
            Image = source == null ? null : new TilesetImage
            {
                Width = properties.TileWidth,
                Height = properties.TileHeight,
                Source = source
            }
        };
    }

    public static TileData InterpretTile(Tile tile, Tilemap2DProperties properties, AssetPathPattern graphicsPattern)
    {
        if (tile == null) throw new ArgumentNullException(nameof(tile));
        if (properties == null) throw new ArgumentNullException(nameof(properties));
        if (graphicsPattern == null) throw new ArgumentNullException(nameof(graphicsPattern));

        var result = new TileData
        {
            Index = (ushort)tile.Id,
            ImageNumber = SourceStringToImageNumber(tile.Image?.Source, properties, graphicsPattern),
            FrameCount = (byte)(tile.Frames?.Count ?? 0)
        };

        if (result.FrameCount == 0 && tile.Image != null)
            result.FrameCount = 1;

        result.Layer     =   (TileLayer)Enum.Parse(typeof(TileLayer),   PropString(tile, Prop.Layer)     ?? "0");
        result.Type      =    (TileType)Enum.Parse(typeof(TileType),    PropString(tile, Prop.Type)      ?? "0");
        result.Collision = (Passability)Enum.Parse(typeof(Passability), PropString(tile, Prop.Collision) ?? "0");
        result.SitMode   =     (SitMode)Enum.Parse(typeof(SitMode),     PropString(tile, Prop.SitMode)   ?? "0");
        result.Unk7      = (byte)(PropInt(tile, Prop.Unk7) ?? 0);

        result.Bouncy   = PropBool(tile, Prop.Bouncy) ?? false;
        result.UseUnderlayFlags = PropBool(tile, Prop.UseUnderlayFlags) ?? false;
        result.Unk12    = PropBool(tile, Prop.Unk12) ?? false;
        result.Unk18    = PropBool(tile, Prop.Unk18) ?? false;
        result.NoDraw   = PropBool(tile, Prop.NoDraw) ?? false;
        result.DebugDot = PropBool(tile, Prop.DebugDot) ?? false;
        return result;
    }

    public static List<TiledProperty> BuildTileProperties(TileData x)
    {
        if (x == null) throw new ArgumentNullException(nameof(x));

        var properties = new List<TiledProperty>
        {
            new(Prop.Layer, x.Layer.ToString()),
            new(Prop.Type, x.Type.ToString())
        };

        if (x.Bouncy) properties.Add(new(Prop.Bouncy, true));
        if (x.UseUnderlayFlags) properties.Add(new(Prop.UseUnderlayFlags, true));
        if (x.Collision != 0) properties.Add(new(Prop.Collision, ((int)x.Collision)));
        if (x.SitMode != 0) properties.Add(new(Prop.SitMode, x.SitMode.ToString()));
        if (x.Unk7 != 0) properties.Add(new(Prop.Unk7, x.Unk7));
        if (x.Unk12) properties.Add(new(Prop.Unk12, true));
        if (x.Unk18) properties.Add(new(Prop.Unk18, true));
        if (x.NoDraw) properties.Add(new(Prop.NoDraw, true));
        if (x.DebugDot) properties.Add(new(Prop.DebugDot, true));

        return properties;
    }

    static ushort SourceStringToImageNumber(string source, Tilemap2DProperties properties, AssetPathPattern graphicsPattern)
    {
        if (string.IsNullOrEmpty(source)) return 0; 
        if (source == properties.BlankTilePath) return 0xffff;
        if (!graphicsPattern.TryParse(source, AssetType.Unknown, out var assetPath)) return 0xffff;
        return (ushort)assetPath.SubAsset;
    }

    public static List<TiledProperty> BuildIsoTileProperties(LabyrinthData labyrinth, int index, IsometricMode isoMode)
    {
        if (labyrinth == null) throw new ArgumentNullException(nameof(labyrinth));

        var properties = new List<TiledProperty>();
        if (index == 0) // First tile always blank
            return properties;

        if (isoMode is IsometricMode.Floors or IsometricMode.Ceilings)
        {
            var floor = labyrinth.FloorAndCeilings[index - 1];
            if (floor == null) return properties;
            properties.Add(new(Prop.Flags, floor.Properties.ToString()));
            properties.Add(new(Prop.Unk1, floor.Unk1));
            properties.Add(new(Prop.Unk2, floor.Unk2));
            properties.Add(new(Prop.Unk3, floor.Unk3));
            properties.Add(new(Prop.Unk5, floor.Unk5));
            properties.Add(new(Prop.Unk8, floor.Unk8));
        }

        if (isoMode == IsometricMode.Walls)
        {
            var wall = labyrinth.Walls[index - 1];
            if (wall == null) return properties;
            properties.Add(new(Prop.AutoGfx, wall.AutoGfxType));
            properties.Add(new(Prop.Collision, wall.Collision));
            properties.Add(new(Prop.Flags, wall.Properties.ToString()));
            properties.Add(new(Prop.Unk9, wall.Unk9));
        }

        if (isoMode == IsometricMode.Contents)
        {
            var group = labyrinth.ObjectGroups[index - 1];
            properties.Add(new(Prop.AutoGfx, group.AutoGraphicsId));
            var objects = group.SubObjects
                .Where(x => x != null)
                .Select(x => x.ObjectInfoNumber >= labyrinth.Objects.Count ? null : labyrinth.Objects[x.ObjectInfoNumber])
                .Where(x => x != null)
                .ToList();

            properties.Add(new(Prop.Collision, string.Join("; ", objects.Select(x => x.Collision))));
            properties.Add(new(Prop.Flags, string.Join("; ", objects.Select(x => x.Properties))));
            properties.Add(new(Prop.Unk7, string.Join("; ", objects.Select(x => x.Unk7))));
        }

        return properties;
    }
}