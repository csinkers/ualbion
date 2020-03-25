using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Game.Assets;

namespace UAlbion.Game.Veldrid.Assets
{
    public class ImageSharpPostProcessor : IAssetPostProcessor
    {
        public IEnumerable<Type> SupportedTypes => new[] { typeof(Image<Rgba32>) };
        public object Process(ICoreFactory factory, AssetKey key, string name, object asset)
            => new ImageSharpTrueColorTexture(name, (Image<Rgba32>)asset);
    }
}
