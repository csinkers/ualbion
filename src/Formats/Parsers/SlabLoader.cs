using System.IO;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.Slab)]
    public class SlabLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config)
        {
            var sprite = (AlbionSprite)new FixedSizeSpriteLoader().Load(br, streamLength, key, config);
            var frames = new[] // Frame 0 = entire slab, Frame 1 = status bar only.
            {
                sprite.Frames[0],
                new AlbionSpriteFrame(0, sprite.Height - 48, sprite.Width, 48)
            };

            return new AlbionSprite(sprite.Name, sprite.Width, sprite.Height, sprite.UniformFrames, sprite.PixelData, frames);
        }
    }
}
