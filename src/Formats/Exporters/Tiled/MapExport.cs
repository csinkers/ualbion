﻿using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Scripting;

namespace UAlbion.Formats.Exporters.Tiled;

public static class MapExport
{
    public static (Map, string) FromAlbionMap2D(
        MapData2D map,
        TilesetData tileset,
        Tilemap2DProperties properties,
        string tilesetPath,
        Tileset npcTileset,
        EventFormatter eventFormatter)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(tileset);
        ArgumentNullException.ThrowIfNull(properties);
        ArgumentNullException.ThrowIfNull(npcTileset);
        ArgumentNullException.ThrowIfNull(eventFormatter);

        int npcGidOffset = tileset.Tiles.Count;
        var (script, functionsByEventId) = BuildScript(map, eventFormatter);

        (int? tileId, int w, int h) GetNpcTileInfo(AssetId id)
        {
            var assetName = id.ToString();
            var tile = npcTileset.Tiles.FirstOrDefault(x => x.Properties.Any(p => p.Name == NpcMapping.Prop.Visual && p.Value == assetName));
            return (
                tile?.Id + npcGidOffset ?? 0,
                tile?.Image.Width ?? properties.TileWidth,
                tile?.Image.Height ?? properties.TileHeight);
        }

        int nextObjectId = 1;
        int nextLayerId = 1;

        var result = new Map
        {
            TiledVersion = "1.4.2",
            Version = "1.4",
            Width = map.Width,
            Height = map.Height,
            TileWidth = properties.TileWidth,
            TileHeight = properties.TileHeight,
            Infinite = 0,
            BackgroundColor = "#000000",
            Orientation = "orthogonal",
            RenderOrder = "right-down",
            Properties = MapMapping.BuildMapProperties(map),
            Tilesets =
            [
                new() { FirstGid = 0, Source = tilesetPath, },
                new() { FirstGid = npcGidOffset, Source = npcTileset.Filename }
            ],
            Layers = LayerMapping2D.BuildMapLayers(map, tileset, ref nextLayerId),
            ObjectGroups = ObjectGroupMapping.BuildObjectGroups(map, properties.TileWidth, properties.TileHeight, GetNpcTileInfo, functionsByEventId, ref nextLayerId, ref nextObjectId)
        };

        result.NextObjectId = nextObjectId;
        result.NextLayerId = nextLayerId;
        return (result, script);
    }

    public static (Map, string) FromAlbionMap3D(MapData3D map, Tilemap3DProperties properties, EventFormatter eventFormatter)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(properties);
        ArgumentNullException.ThrowIfNull(eventFormatter);

        if (string.IsNullOrEmpty(properties.FloorPath)) throw new ArgumentException("No floor path given", nameof(properties));
        if (string.IsNullOrEmpty(properties.CeilingPath)) throw new ArgumentException("No ceiling path given", nameof(properties));
        if (string.IsNullOrEmpty(properties.WallPath)) throw new ArgumentException("No wall path given", nameof(properties));
        if (string.IsNullOrEmpty(properties.ContentsPath)) throw new ArgumentException("No contents path given", nameof(properties));
        if (properties.TileWidth is <= 0 or > 255) throw new ArgumentException("Width must be in the range [1..255]", nameof(properties));
        if (properties.TileHeight is <= 0 or > 255) throw new ArgumentException("Height must be in the range [1..255]", nameof(properties));

        var (script, functionsByEventId) = BuildScript(map, eventFormatter);

        (int? tileId, int w, int h) GetNpcTileInfo(AssetId id)
        {
            var tile = id.Id == 0 ? null : (int?)id.Id;
            return (tile + LayerMapping3D.ContentsGid, properties.TileHeight, properties.TileHeight);
        }

        int nextObjectId = 1;
        int nextLayerId = 1;

        var result = new Map
        {
            TiledVersion = "1.4.2",
            Version = "1.4",
            Width = map.Width,
            Height = map.Height,
            TileWidth = properties.TileWidth,
            TileHeight = properties.TileHeight,
            Infinite = 0,
            Orientation = "isometric",
            RenderOrder = "right-down",
            BackgroundColor = "#000000",
            Properties = MapMapping.BuildMapProperties(map),
            Tilesets =
            [
                new() { FirstGid = LayerMapping3D.FloorGid, Source = properties.FloorPath, },
                new() { FirstGid = LayerMapping3D.WallGid, Source = properties.WallPath, },
                new() { FirstGid = LayerMapping3D.ContentsGid, Source = properties.ContentsPath },
                new() { FirstGid = LayerMapping3D.CeilingGid, Source = properties.CeilingPath, }
            ],
            Layers = LayerMapping3D.BuildLayers(map, ref nextLayerId),
            ObjectGroups = ObjectGroupMapping.BuildObjectGroups(map, properties.TileHeight, properties.TileHeight, GetNpcTileInfo, functionsByEventId, ref nextLayerId, ref nextObjectId)
        };

        result.NextObjectId = nextObjectId;
        result.NextLayerId = nextLayerId;
        return (result, script);
    }

    static (string script, Dictionary<ushort, string> functionsByEventId) BuildScript(IEventSet eventSet, EventFormatter eventFormatter)
    {
        var functionMapping = new Dictionary<ushort, string>();

        if (eventSet.Events.Count <= 0)
            return ("", functionMapping);

        var extraEntries = eventSet.ExtraEntryPoints;
        var decompiled = eventFormatter.Decompile(eventSet.Events, eventSet.Chains, extraEntries);

        foreach (var entryEventId in extraEntries)
            functionMapping[entryEventId] = ScriptConstants.BuildAdditionalEntryLabel(entryEventId);

        for (int chainId = 0; chainId < eventSet.Chains.Count; chainId++)
            functionMapping[eventSet.Chains[chainId]] = ScriptConstants.BuildChainLabel(chainId);

        return (decompiled.Script, functionMapping);
    }
}