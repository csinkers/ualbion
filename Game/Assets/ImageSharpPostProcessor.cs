using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Core.Textures;

namespace UAlbion.Game.Assets
{
    [AssetPostProcessor(typeof(Image<Rgba32>))]
    public class ImageSharpPostProcessor : IAssetPostProcessor
    {
        public object Process(string name, object asset) => new TrueColorTexture(name, (Image<Rgba32>)asset);
    }
}