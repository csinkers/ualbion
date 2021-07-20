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
using UAlbion.Formats;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Exporters.Tiled;
using UAlbion.Formats.Parsers;
using UAlbion.Game.Events;
using UAlbion.Game.Scenes;
using Veldrid;

namespace UAlbion.Game.Veldrid.Assets
{
    public sealed class IsometricLabyrinthLoader : Component, IAssetLoader<LabyrinthData>, IDisposable
    {
        public const int DefaultWidth = 48;
        public const int DefaultHeight = 64;
        public const int DefaultBaseHeight = 40;
        public const int DefaultTilesPerRow = 16;

        readonly JsonLoader<LabyrinthData> _jsonLoader = new();
        Engine _engine;
        IsometricBuilder _builder;

        void SetupEngine(int tileWidth, int tileHeight, int baseHeight, int tilesPerRow)
        {
            var (services, builder) = IsometricSetup.SetupEngine(Exchange,
                tileWidth, tileHeight, baseHeight, tilesPerRow,
                GraphicsBackend.Vulkan, false, null);
            AttachChild(services);
            _engine = (Engine)Resolve<IEngine>();
            _builder = builder;
            Raise(new SetSceneEvent(SceneId.IsometricBake));
            Raise(new SetClearColourEvent(0, 0, 0, 0));
            // Raise(new EngineFlagEvent(FlagOperation.Set, EngineFlags.ShowBoundingBoxes));
#pragma warning restore CA2000 // Dispose objects before losing scopes
        }

        IEnumerable<(string, byte[])> Save(LabyrinthData labyrinth, AssetInfo info, IsometricMode mode, string pngPath, string tsxPath)
        {
            var tileWidth = info.Get(AssetProperty.TileWidth, DefaultWidth);
            var tileHeight = info.Get(AssetProperty.TileHeight, DefaultHeight);
            var baseHeight = info.Get(AssetProperty.BaseHeight, DefaultBaseHeight);
            var tilesPerRow = info.Get(AssetProperty.TilesPerRow, DefaultTilesPerRow);

            if (_engine == null)
                SetupEngine(tileWidth, tileHeight, baseHeight, tilesPerRow);

            var assets = Resolve<IAssetManager>();
            var frames = _builder.Build(labyrinth, info, mode, assets);

            Image<Bgra32> image = _engine.RenderFrame(false);

            using var stream = new MemoryStream();
            image.SaveAsPng(stream);
            stream.Position = 0;
            var pngBytes = stream.ToArray();
            yield return (pngPath, pngBytes);

            var properties = new Tilemap3DProperties
            {
                TilesetId = labyrinth.Id.Id,
                IsoMode = mode,
                TileWidth = tileWidth,
                TileHeight = tileHeight,
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

        public void Dispose() => _engine?.Dispose();
    }
}
