using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SerdesNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Exporters.Tiled;
using UAlbion.Formats.Parsers;
using UAlbion.Game.Events;
using UAlbion.Game.Scenes;
using Veldrid;

namespace UAlbion.Game.Veldrid.Assets;

public sealed class IsometricLabyrinthLoader : GameComponent, IAssetLoader<LabyrinthData>, IDisposable
{
    public const int DefaultWidth = 48;
    public const int DefaultHeight = 64;
    public const int DefaultBaseHeight = 40;
    public const int DefaultTilesPerRow = 16;

    public static readonly IntAssetProperty TilesPerRow             = new("TilesPerRow"); // int
    public static readonly IntAssetProperty TileHeight              = new("TileHeight"); // int
    public static readonly IntAssetProperty BaseHeight              = new("BaseHeight"); // int
    public static readonly IntAssetProperty TileWidth               = new("TileWidth"); // int
    public static readonly PathPatternProperty TiledFloorPattern    = new("TiledFloorPattern",    "Tiled/{0}_{2}_Floors.tsx");
    public static readonly PathPatternProperty TiledCeilingPattern  = new("TiledCeilingPattern",  "Tiled/{0}_{2}_Ceilings.tsx");
    public static readonly PathPatternProperty TiledWallPattern     = new("TiledWallPattern",     "Tiled/{0}_{2}_Walls.tsx");
    public static readonly PathPatternProperty TiledContentsPattern = new("TiledContentsPattern", "Tiled/{0}_{2}_Contents.tsx");
    public static readonly PathPatternProperty FloorPngPattern      = new("FloorPngPattern",      "Tiled/Gfx/{0}_{2}_Floors.png");
    public static readonly PathPatternProperty CeilingPngPattern    = new("CeilingPngPattern",    "Tiled/Gfx/{0}_{2}_Ceilings.png");
    public static readonly PathPatternProperty WallPngPattern       = new("WallPngPattern",       "Tiled/Gfx/{0}_{2}_Walls.png");
    public static readonly PathPatternProperty ContentsPngPattern   = new("ContentsPngPattern",   "Tiled/Gfx/{0}_{2}_Contents.png");


    // TODO: Calculate these properly
    const int HackyContentsOffsetX = -143;
    const int HackyContentsOffsetY = 235;

    readonly JsonLoader<LabyrinthData> _jsonLoader = new();
    IsometricRenderSystem _isoRsm;
    Engine _engine;
    ShaderLoader _shaderLoader;

    void SetupEngine(ModContext modContext, int tileWidth, int tileHeight, int baseHeight, int tilesPerRow)
    {
        var pathResolver = Resolve<IPathResolver>();
        AttachChild(new ShaderCache(pathResolver.ResolvePath("$(CACHE)/ShaderCache")));
        _shaderLoader = new ShaderLoader();

        foreach (var shaderPath in Resolve<IModApplier>().ShaderPaths)
            _shaderLoader.AddShaderDirectory(shaderPath);

        _engine = new Engine(GraphicsBackend.Vulkan, false, false);
        _isoRsm = new IsometricRenderSystem(modContext, tileWidth, tileHeight, baseHeight, tilesPerRow);

        AttachChild(_shaderLoader);
        AttachChild(_engine);
        AttachChild(_isoRsm);

        _isoRsm.OffScreen.IsActive = true;
        _engine.RenderSystem = _isoRsm.OffScreen;

        Raise(new SetSceneEvent(SceneId.IsometricBake));
        Raise(new SetClearColourEvent(0, 0, 0, 0));
        // Raise(new EngineFlagEvent(FlagOperation.Set, EngineFlags.ShowBoundingBoxes));
    }

    IEnumerable<(string, byte[])> Save(LabyrinthData labyrinth, AssetLoadContext context, IsometricMode mode, string pngPath, string tsxPath)
    {
        var tileWidth = context.GetProperty(TileWidth, DefaultWidth);
        var tileHeight = context.GetProperty(TileHeight, DefaultHeight);
        var baseHeight = context.GetProperty(BaseHeight, DefaultBaseHeight);
        var tilesPerRow = context.GetProperty(TilesPerRow, DefaultTilesPerRow);

        if (_engine == null)
            SetupEngine(context.ModContext, tileWidth, tileHeight, baseHeight, tilesPerRow);

        var frames = _isoRsm.Builder.Build(labyrinth, context, mode, Assets);

        _engine.RenderFrame(false);
        Image<Bgra32> image = _engine.ReadTexture2D(_isoRsm.IsoBuffer.GetColorTexture(0));

        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        stream.Position = 0;
        var pngBytes = stream.ToArray();
        yield return (pngPath, pngBytes);

        int expansionFactor = mode == IsometricMode.Contents ? IsometricBuilder.ContentsExpansionFactor : 1;
        var properties = new Tilemap3DProperties
        {
            // TilesetId = labyrinth.Id.Id,
            IsoMode = mode,
            TileWidth = expansionFactor * tileWidth,
            TileHeight = expansionFactor * tileHeight,
            ImagePath = pngPath,
            TilesetPath = tsxPath,
            ImageWidth = image.Width,
            ImageHeight = image.Height,
            OffsetX = mode == IsometricMode.Contents ? HackyContentsOffsetX : 0,
            OffsetY = mode == IsometricMode.Contents ? HackyContentsOffsetY : 0
        };

        var tiledTileset = TilesetMapping.FromLabyrinth(labyrinth, properties, frames);
        var tsxBytes = FormatUtil.BytesFromTextWriter(tiledTileset.Serialize);
        yield return (tsxPath, tsxBytes);
    }

    byte[] SaveJson(LabyrinthData labyrinth, AssetLoadContext context) =>
        FormatUtil.SerializeToBytes(s =>
            _jsonLoader.Serdes(labyrinth, s, context));

    public LabyrinthData Serdes(LabyrinthData existing, ISerializer s, AssetLoadContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var json = context.GetProperty(AssetProps.Pattern, AssetPathPattern.Build("{0}_{2}.json"));
        var path = context.BuildAssetPath();
        string BuildPath(AssetPathPattern pattern) => pattern.Format(path);

        if (s.IsReading())
        {
            var chunks = PackedChunks.Unpack(s);
            var (chunk, _) = chunks.Single();

            return FormatUtil.DeserializeFromBytes(chunk, s2 =>
                _jsonLoader.Serdes(null, s2, context));
        }

        if (existing == null) throw new ArgumentNullException(nameof(existing));
        var floorTsx    = context.GetProperty(TiledFloorPattern);
        var ceilingTsx  = context.GetProperty(TiledCeilingPattern);
        var wallTsx     = context.GetProperty(TiledWallPattern);
        var contentsTsx = context.GetProperty(TiledContentsPattern);
        var floorPng    = context.GetProperty(FloorPngPattern);
        var ceilingPng  = context.GetProperty(CeilingPngPattern);
        var wallPng     = context.GetProperty(WallPngPattern);
        var contentsPng = context.GetProperty(ContentsPngPattern);

        var files = new List<(string, byte[])> {(json.Format(path), SaveJson(existing, context))};
        files.AddRange(Save(existing, context, IsometricMode.Floors,   BuildPath(floorPng),    BuildPath(floorTsx)));
        files.AddRange(Save(existing, context, IsometricMode.Ceilings, BuildPath(ceilingPng),  BuildPath(ceilingTsx)));
        files.AddRange(Save(existing, context, IsometricMode.Walls,    BuildPath(wallPng),     BuildPath(wallTsx)));
        files.AddRange(Save(existing, context, IsometricMode.Contents, BuildPath(contentsPng), BuildPath(contentsTsx)));

        PackedChunks.PackNamed(s, files.Count, i => (files[i].Item2, files[i].Item1));
        return existing;
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((LabyrinthData)existing, s, context);

    public void Dispose()
    {
        _engine?.Dispose();
        _isoRsm?.Dispose();
        _shaderLoader?.Dispose();
    }
}

