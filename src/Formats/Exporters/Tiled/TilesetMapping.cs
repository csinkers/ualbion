using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Exporters.Tiled;

public static class TilesetMapping
{
    static class Prop
    {
        public const string Frame = "Frame";
        public const string Visual = "Visual";
    }

    public static Tileset FromAlbion(TilesetData tileset, Tilemap2DProperties properties)
    {
        if (tileset == null) throw new ArgumentNullException(nameof(tileset));
        if (properties == null) throw new ArgumentNullException(nameof(properties));

        var graphicsPattern = AssetPathPattern.Build(properties.GraphicsTemplate);
        List<Tile> tiles =
            tileset.Tiles
                .Where(x => !x.IsBlank)
                .Select(x =>
                    TileMapping.BuildTile(
                        tileset.Id.Id,
                        x.Index,
                        x.FrameCount > 0 ? x.ImageNumber : null,
                        TileMapping.BuildTileProperties(x),
                        properties,
                        graphicsPattern))
                .ToList();

        // Add tiles for the extra frames of animated tiles
        int nextId = tileset.Tiles[^1].Index + 1;
        int maxTile = tiles.Count;
        for (int i = 0; i < maxTile; i++)
        {
            var tile = tiles[i];
            var sourceTile = tileset.Tiles[tile.Id];
            if (sourceTile.FrameCount <= 1)
                continue;

            tile.Frames = new List<TileFrame> { new(tile.Id, properties.FrameDurationMs) };
            for (int f = 1; f < sourceTile.FrameCount; f++)
            {
                tiles.Add(TileMapping.BuildTile(
                    tileset.Id.Id,
                    nextId,
                    (ushort)(sourceTile.ImageNumber + f),
                    new List<TiledProperty> { new(Prop.Frame, true) },
                    properties,
                    graphicsPattern));

                tile.Frames.Add(new TileFrame(nextId, properties.FrameDurationMs));
                nextId++;
            }
        }

        return new Tileset
        {
            Name = tileset.Id.ToString(),
            Version = "1.4",
            TiledVersion = "1.4.2",
            TileCount = tiles.Count,
            TileWidth = properties.TileWidth,
            TileHeight = properties.TileHeight,
            // Margin = properties.Margin,
            // Spacing = properties.Spacing,
            // Columns = columns,
            BackgroundColor = "#000000",
            Tiles = tiles,
            // TODO: Terrain
            // wang sets
        };
    }

    public static TilesetData ToAlbion(Tileset tileset, TilesetId id, Tilemap2DProperties properties)
    {
        if (properties == null) throw new ArgumentNullException(nameof(properties));

        var t = new TilesetData(id);
        var graphicsPattern = AssetPathPattern.Build(properties.GraphicsTemplate);
        var tileLookup =
            tileset.Tiles
                .Select(x => TileMapping.InterpretTile(x, properties, graphicsPattern))
                .Where(x => x != null)
                .ToDictionary(x => x.Index);

        for (ushort i = 0; i < TilesetData.TileCount; i++)
        {
            tileLookup.TryGetValue(i, out var tile);
            t.Tiles.Add(tile ?? new TileData { Index = i });
        }

        return t;
    }

    public static Tileset FromLabyrinth(LabyrinthData labyrinth, Tilemap3DProperties properties, List<int>[] allFrames)
    {
        if (labyrinth == null) throw new ArgumentNullException(nameof(labyrinth));
        if (properties == null) throw new ArgumentNullException(nameof(properties));
        if (allFrames == null) throw new ArgumentNullException(nameof(allFrames));

        List<Tile> tiles =
            allFrames
                .Select((_, i) => new Tile { Id = i, Properties = TileMapping.BuildIsoTileProperties(labyrinth, i, properties.IsoMode) })
                .ToList();

        // Add tiles for the extra frames of animated tiles
        for (int i = 0; i < allFrames.Length; i++)
        {
            var frames = allFrames[i];
            if (frames.Count <= 1)
                continue;

            var tile = tiles[i];
            tile.Frames = frames.Select(x => new TileFrame(x, properties.FrameDurationMs)).ToList();
            for (int f = 1; f < frames.Count; f++)
            {
                tiles.Add(new Tile
                {
                    Id = (ushort)frames[f],
                    Properties = new List<TiledProperty> { new(Prop.Frame, true) }
                });
            }
        }

        var relativeGraphicsPath = ConfigUtil.GetRelativePath(
            properties.ImagePath,
            Path.GetDirectoryName(properties.TilesetPath),
            true);

        return new Tileset
        {
            Name = $"{labyrinth.Id}.{properties.IsoMode}",
            Version = "1.4",
            TiledVersion = "1.4.2",
            TileCount = tiles.Count,
            TileWidth = properties.TileWidth,
            TileHeight = properties.TileHeight,
            BackgroundColor = "#000000",
            Tiles = tiles,
            Offset =
                properties.OffsetX == 0 && properties.OffsetY == 0 
                    ? null 
                    : new TileOffset { X = properties.OffsetX, Y = properties.OffsetY },

            Grid = new TiledGrid
            {
                Orientation = "isometric",
                Width = properties.TileWidth,
                Height = properties.TileHeight,
            },
            Image = new TilesetImage
            {
                Source = relativeGraphicsPath,
                Width = properties.ImageWidth,
                Height = properties.ImageHeight,
            }
        };
    }

    public static Tileset FromSprites(string name, string type, IList<TileProperties> tiles) // (name, source, w, h)
    {
        if (tiles == null) throw new ArgumentNullException(nameof(tiles));
        return new Tileset
        {
            Name = name,
            Version = "1.4",
            TiledVersion = "1.4.2",
            TileCount = tiles.Count,
            TileWidth = tiles.Max(x => x.Width),
            TileHeight = tiles.Max(x => x.Height),
            Columns = 1,
            BackgroundColor = "#000000",
            Tiles = tiles.Select((x, i) => new Tile
            {
                Id = i,
                Type = type,
                Image = new TilesetImage
                {
                    Source = x.Source,
                    Width = x.Width,
                    Height = x.Height
                },
                Properties = new List<TiledProperty>
                {
                    new(Prop.Visual, x.Name)
                }
            }).ToList(),
        };
    }
}
