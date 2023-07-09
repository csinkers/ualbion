using System;
using System.Collections.Generic;
using System.IO;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Formats;
using UAlbion.Formats.Exporters.Tiled;
using UAlbion.Formats.Parsers;

namespace UAlbion.Game.Veldrid.Assets;

public class NpcTilesetLoader : Component, IAssetLoader
{
    readonly Png8Loader _png8Loader = new();

    public NpcTilesetLoader() => AttachChild(_png8Loader);
    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (!s.IsWriting())
            return new object();

        if (existing == null) throw new ArgumentNullException(nameof(existing));
        var graphicsPattern = context.GetProperty(AssetProps.GraphicsPattern);
        bool small = context.GetProperty(AssetProps.IsSmall);

        var tsxDir = Path.GetDirectoryName(context.Filename);
        var tiles = new List<TileProperties>();
        var sourceAssets = Resolve<IAssetManager>();
        var pathResolver = Resolve<IPathResolver>();
        var assetIds =
            AssetMapping.Global.EnumerateAssetsOfType(small
                ? AssetType.NpcSmallGfx
                : AssetType.NpcLargeGfx);

        foreach (var id in assetIds)
        {
            var sprite = sourceAssets.LoadTexture(id); // Get sprite from source mod
            var sourceNode = sourceAssets.GetAssetInfo(id);
            // Ugh, hacky.
            var npcNode = new AssetNode(id, null);
            npcNode.SetProperty(AssetProps.Palette, sourceNode.PaletteId);

            var subContext = new AssetLoadContext(id, npcNode, context.ModContext);
            var assetPath = subContext.BuildAssetPath(9); // 9 = First frame facing west for both large and small

            var path = graphicsPattern.Format(assetPath);
            path = pathResolver.ResolvePath(path);
            WriteNpcSprite(path, sprite, subContext);

            var pathRelativeToTsx = ConfigUtil.GetRelativePath(path, tsxDir, true);

            tiles.Add(new TileProperties
            {
                Name = id.ToString(),
                Frames = 1,
                Source = pathRelativeToTsx
            }); // TODO: Add object templates 
        }

        var tilemap = TilesetMapping.FromSprites(small ? "SmallNPCs" : "LargeNPCs", "NPC", tiles);
        var bytes = FormatUtil.BytesFromTextWriter(tilemap.Serialize);
        s.Bytes(null, bytes, bytes.Length);

        return existing;
    }

    void WriteNpcSprite(string path, ITexture sprite, AssetLoadContext context)
    {
        var dir = Path.GetDirectoryName(path);
        if (!context.Disk.DirectoryExists(dir))
            context.Disk.CreateDirectory(dir);

        using var s = FormatUtil.SerializeWithSerdes(s => _png8Loader.Serdes(sprite, s, context));
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
