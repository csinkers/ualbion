using System;
using System.Collections.Generic;
using System.IO;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats;
using UAlbion.Formats.Exporters.Tiled;
using UAlbion.Formats.Parsers;
using UAlbion.Game.Assets;

namespace UAlbion.Game.Veldrid.Assets
{
    public class NpcTilesetLoader : Component, IAssetLoader
    {
        readonly PngLoader _pngLoader = new PngLoader();

        public NpcTilesetLoader() => AttachChild(_pngLoader);
        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (s == null) throw new ArgumentNullException(nameof(s));

            if (!s.IsWriting())
                return new object();

            if (existing == null) throw new ArgumentNullException(nameof(existing));
            var graphicsPattern = info.Get(AssetProperty.GraphicsPattern, "");
            bool small = info.Get(AssetProperty.IsSmall, false);

            var tiles = new List<TileProperties>();
            var assets = Resolve<IAssetManager>();
            var modApplier = Resolve<IModApplier>();
            var disk = Resolve<IFileSystem>();
            var config = Resolve<IGeneralConfig>();
            var assetIds =
                AssetMapping.Global.EnumerateAssetsOfType(small
                    ? AssetType.SmallNpcGraphics
                    : AssetType.LargeNpcGraphics);

            foreach (var id in assetIds)
            {
                var sprite = assets.LoadTexture(id); // Get sprite from source mod
                var spriteInfo = modApplier.GetAssetInfo(id, null); // But info from target mod

                // Ugh, hacky.
                int palId = spriteInfo.Get(AssetProperty.PaletteId, 0);
                if (palId == 0)
                    spriteInfo.Set(AssetProperty.PaletteId,
                        assets.GetAssetInfo(id).Get(AssetProperty.PaletteId, 0));

                var path = spriteInfo.BuildFilename(graphicsPattern,
                    9); // 9 = First frame facing west for both large and small
                path = config.ResolvePath(path);
                WriteNpcSprite(path, sprite, spriteInfo, disk, mapping);

                tiles.Add(new TileProperties
                {
                    Name = id.ToString(),
                    Frames = 1,
                    Source = path
                }); // TODO: Add object templates 
            }

            var tilemap = Tileset.FromSprites(small ? "SmallNPCs" : "LargeNPCs", "NPC", tiles);
            var bytes = FormatUtil.BytesFromTextWriter(tilemap.Serialize);
            s.Bytes(null, bytes, bytes.Length);

            return existing;
        }

        void WriteNpcSprite(string path, ITexture sprite, AssetInfo info, IFileSystem disk, AssetMapping mapping)
        {
            var dir = Path.GetDirectoryName(path);
            if (!disk.DirectoryExists(dir))
                disk.CreateDirectory(dir);

            using var s = FormatUtil.SerializeWithSerdes(s => _pngLoader.Serdes(sprite, info, mapping, s));
            int i = 0;
            foreach (var (chunk, _) in PackedChunks.Unpack(s))
            {
                if (i == 9)
                {
                    disk.WriteAllBytes(path, chunk);
                    return;
                }

                i++;
            }
        }
    }
}