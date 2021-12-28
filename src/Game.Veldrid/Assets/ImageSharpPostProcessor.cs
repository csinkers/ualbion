using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Config;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Game.Assets;

namespace UAlbion.Game.Veldrid.Assets;

public class ImageSharpPostProcessor : IAssetPostProcessor
{
    public object Process(object asset, AssetInfo info)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        return ImageSharpUtil.FromImageSharp(info.AssetId, info.AssetId.ToString(), (Image<Rgba32>)asset);
    }
}