using System;
using System.Collections.Generic;
using System.IO;
using SerdesNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Veldrid.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Exporters.Tiled;
using UAlbion.Formats.Parsers;
using UAlbion.Game.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Scenes;
using UAlbion.Game.State;
using Veldrid;

namespace UAlbion.Game.Veldrid.Assets
{
    public class IsometricLabyrinthLoader : Component, IAssetLoader<LabyrinthData>
    {
        public const int DefaultWidth = 48;
        public const int DefaultHeight = 64;
        public const int DefaultBaseHeight = 40;
        public const int DefaultTilesPerRow = 16;

        readonly JsonLoader<LabyrinthData> _jsonLoader = new JsonLoader<LabyrinthData>();
        VeldridEngine _engine;
        VeldridRendererContext _context;
        IsometricBuilder _builder;

        void SetupEngine(int width, int height, int baseHeight, int tilesPerRow)
        {
#pragma warning disable CA2000 // Dispose objects before losing scopes
            var config = Resolve<IGeneralConfig>();
            var shaderCache = new ShaderCache(config.ResolvePath("$(CACHE)/ShaderCache"));
            var fbSource = new FramebufferSource(DefaultWidth * DefaultTilesPerRow, DefaultHeight);

            foreach (var shaderPath in Resolve<IModApplier>().ShaderPaths)
                shaderCache.AddShaderPath(shaderPath);

            _engine = new VeldridEngine(
                GraphicsBackend.Vulkan, false, false, false)
                .AddRenderer(new ExtrudedTileMapRenderer())
                .AddRenderer(new SpriteRenderer())
                ;

            _engine.ChangeBackend();
#pragma warning restore CA2000 // Dispose objects before losing scopes

            var services = new Container("IsometricLayoutServices");
            _builder = new IsometricBuilder(fbSource, width, height, baseHeight, tilesPerRow);
            services
                .Add(shaderCache)
                .Add(fbSource)
                .Add(_engine)
                .Add(new DeviceObjectManager())
                .Add(new SpriteManager())
                .Add(new TextureManager())
                .Add(new SceneStack())
                .Add(new SceneManager()
                    .AddScene(new EmptyScene())
                    .AddScene((Scene)new IsometricBakeScene()
                        .Add(new PaletteManager())
                        .Add(_builder)))
                ;

            AttachChild(services);
            Raise(new SetSceneEvent(SceneId.IsometricBake));
            Raise(new SetClearColourEvent(0, 0, 0, 0));
            // Raise(new EngineFlagEvent(FlagOperation.Set, EngineFlags.ShowBoundingBoxes));

            _context = _engine.BuildContext(fbSource);
        }

        IEnumerable<(string, byte[])> Save(LabyrinthData labyrinth, AssetInfo info, IsometricMode mode, string pngPath, string tsxPath)
        {
            var width = info.Get(AssetProperty.TileWidth, DefaultWidth);
            var height = info.Get(AssetProperty.TileHeight, DefaultHeight);
            var baseHeight = info.Get(AssetProperty.BaseHeight, DefaultBaseHeight);
            var tilesPerRow = info.Get(AssetProperty.TilesPerRow, DefaultTilesPerRow);

            if (_engine == null)
                SetupEngine(width, height, baseHeight, tilesPerRow);

            var assets = Resolve<IAssetManager>();
            var frames = _builder.Build(labyrinth, info, mode, assets);

            Image<Bgra32> image = _engine.RenderFrame(_context, false);

            using var stream = new MemoryStream();
            image.SaveAsPng(stream);
            stream.Position = 0;
            var pngBytes = stream.ToArray();
            yield return (pngPath, pngBytes);

            var properties = new Tilemap3DProperties
            {
                TilesetId = labyrinth.Id.Id,
                IsoMode = mode,
                TileWidth = width,
                TileHeight = height,
                ImagePath = pngPath,
                TilesetPath = tsxPath,
                ImageWidth = image.Width,
                ImageHeight = image.Height,
            };

            var tiledTileset = Tileset.FromLabyrinth(labyrinth, properties, frames);
            var tsxBytes = FormatUtil.BytesFromTextWriter(tiledTileset.Serialize);
            yield return (tsxPath, tsxBytes);
        }

        byte[] SaveJson(LabyrinthData labyrinth, AssetInfo info, AssetMapping mapping) =>
            FormatUtil.SerializeToBytes(s =>
                _jsonLoader.Serdes(labyrinth, info, mapping, s));

        public LabyrinthData Serdes(LabyrinthData existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (s.IsWriting())
            {
                if (existing == null) throw new ArgumentNullException(nameof(existing));
                var json        = info.Get(AssetProperty.Pattern, "{0}_{2}.json");
                var floorTsx    = info.Get(AssetProperty.TiledFloorPattern, "Tiled/{0}_{2}_Floors.tsx");
                var ceilingTsx  = info.Get(AssetProperty.TiledCeilingPattern, "Tiled/{0}_{2}_Ceilings.tsx");
                var wallTsx     = info.Get(AssetProperty.TiledWallPattern, "Tiled/{0}_{2}_Walls.tsx");
                var contentsTsx = info.Get(AssetProperty.TiledContentsPattern, "Tiled/{0}_{2}_Contents.tsx");
                var floorPng    = info.Get(AssetProperty.FloorPngPattern, "Tiled/Gfx/{0}_{2}_Floors.png");
                var ceilingPng  = info.Get(AssetProperty.CeilingPngPattern, "Tiled/Gfx/{0}_{2}_Ceilings.png");
                var wallPng     = info.Get(AssetProperty.WallPngPattern, "Tiled/Gfx/{0}_{2}_Walls.png");
                var contentsPng = info.Get(AssetProperty.ContentsPngPattern, "Tiled/Gfx/{0}_{2}_Contents.png");

                string B(string pattern) => info.BuildFilename(pattern, 0);

                var files = new List<(string, byte[])> { (B(json), SaveJson(existing, info, mapping)) };
                files.AddRange(Save(existing, info, IsometricMode.Floors, B(floorPng), B(floorTsx)));
                files.AddRange(Save(existing, info, IsometricMode.Ceilings, B(ceilingPng), B(ceilingTsx)));
                files.AddRange(Save(existing, info, IsometricMode.Walls, B(wallPng), B(wallTsx)));
                files.AddRange(Save(existing, info, IsometricMode.Contents, B(contentsPng), B(contentsTsx)));

                PackedChunks.PackNamed(s, files.Count, i => (files[i].Item2, files[i].Item1));
                return existing;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((LabyrinthData)existing, info, mapping, s);
    }
}
