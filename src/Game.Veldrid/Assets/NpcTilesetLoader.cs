using System;
using System.Collections.Generic;
using System.IO;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Exporters.Tiled;
using UAlbion.Formats.Parsers;

namespace UAlbion.Game.Veldrid.Assets;

public class NpcTilesetLoader : Component, IAssetLoader
{
    readonly Png8Loader _png8Loader = new();

    public NpcTilesetLoader() => AttachChild(_png8Loader);
    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (!s.IsWriting())
            return new object();

        if (existing == null) throw new ArgumentNullException(nameof(existing));
        var graphicsPattern = info.GetPattern(AssetProperty.GraphicsPattern, "");
        bool small = info.Get(AssetProperty.IsSmall, false);

        var tiles = new List<TileProperties>();
        var assets = Resolve<IAssetManager>();
        var modApplier = Resolve<IModApplier>();
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

            var path = graphicsPattern.Format(new AssetPath(spriteInfo, 9)); // 9 = First frame facing west for both large and small
            path = config.ResolvePath(path);
            WriteNpcSprite(path, sprite, spriteInfo, context);

            tiles.Add(new TileProperties
            {
                Name = id.ToString(),
                Frames = 1,
                Source = path
            }); // TODO: Add object templates 
        }

        var tilemap = TilesetMapping.FromSprites(small ? "SmallNPCs" : "LargeNPCs", "NPC", tiles);
        var bytes = FormatUtil.BytesFromTextWriter(tilemap.Serialize);
        s.Bytes(null, bytes, bytes.Length);

        return existing;
    }

    void WriteNpcSprite(string path, ITexture sprite, AssetInfo info, SerdesContext context)
    {
        var dir = Path.GetDirectoryName(path);
        if (!context.Disk.DirectoryExists(dir))
            context.Disk.CreateDirectory(dir);

        using var s = FormatUtil.SerializeWithSerdes(s => _png8Loader.Serdes(sprite, info, s, context));
        int i = 0;
        foreach (var (chunk, _) in PackedChunks.Unpack(s))
        {
            if (i == 9)
            {
                context.Disk.WriteAllBytes(path, chunk);
                return;
            }

            i++;
        }
    }
}
