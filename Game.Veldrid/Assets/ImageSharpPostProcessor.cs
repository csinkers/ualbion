using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Assets;

namespace UAlbion.Game.Veldrid.Assets
{
    public class ImageSharpPostProcessor : IAssetPostProcessor
    {
        public IEnumerable<Type> SupportedTypes => new[] { typeof(Image<Rgba32>) };
        public object Process(ICoreFactory factory, AssetKey key, object asset, Func<AssetKey, object> loaderFunc)
            => new ImageSharpTrueColorTexture(key.ToString(), (Image<Rgba32>)asset);
    }
}
