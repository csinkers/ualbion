using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Game.Assets;

namespace UAlbion.Game.Veldrid.Assets
{
    public class ImageSharpPostProcessor : IAssetPostProcessor
    {
        public IEnumerable<Type> SupportedTypes => new[] { typeof(Image<Rgba32>) };
        public object Process(ICoreFactory factory, AssetId key, object asset, SerializationContext context, Func<AssetId, SerializationContext, object> loaderFunc)
            => new ImageSharpTrueColorTexture(key.ToString(), (Image<Rgba32>)asset);
    }
}
