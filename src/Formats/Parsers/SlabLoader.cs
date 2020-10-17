using System.IO;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.Slab)]
    public class SlabLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, AssetMapping mapping, AssetId id, AssetInfo config)
        {
            var sprite = (AlbionSprite)new FixedSizeSpriteLoader().Load(br, streamLength, mapping, id, config);
            var frames = new[] // Frame 0 = entire slab, Frame 1 = status bar only.
            {
                sprite.Frames[0],
                new AlbionSpriteFrame(0, sprite.Height - 48, sprite.Width, 48)
            };

            return new AlbionSprite(sprite.Name, sprite.Width, sprite.Height, sprite.UniformFrames, sprite.PixelData, frames);
        }
    }
}
